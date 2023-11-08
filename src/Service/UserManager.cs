using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Interaction;
using iPanelHost.Server.WebSocket;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Sharprompt;
using Spectre.Console;

namespace iPanelHost.Service;

public static class UserManager
{
    public static readonly Dictionary<PermissionLevel, string> LevelNames =
        new()
        {
            { PermissionLevel.Guest, "游客" },
            { PermissionLevel.ReadOnly, "只读" },
            { PermissionLevel.Assistant, "助手" },
            { PermissionLevel.Administrator, "管理员" }
        };

    private static readonly Timer _timer = new(60_000) { AutoReset = true };

    static UserManager()
    {
        _timer.Elapsed += (_, _) => Save();
        _timer.Start();
    }

    /// <summary>
    /// 用户字典
    /// </summary>
    public static Dictionary<string, User> Users { get; private set; } = new();

    /// <summary>
    ///保存路径
    /// </summary>
    private const string _path = "users.json";

    public static readonly object _lock = new();

    /// <summary>
    /// 读取用户文件
    /// </summary>
    public static void Read()
    {
        lock (_lock)
        {
            if (!File.Exists(_path))
            {
                Logger.Warn("用户列表为空。请输入“user create”或“u c”添加一个用户");
                Save();
                return;
            }

            try
            {
                Users =
                    JsonConvert.DeserializeObject<Dictionary<string, User>>(File.ReadAllText(_path))
                    ?? throw new FileLoadException("文件数据异常");

                if (Users.Count == 0)
                {
                    Logger.Warn("用户列表为空。请输入“user create”或“u c”添加一个用户");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"加载用户文件{_path}时出错");
                Logger.Error(e);
            }
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    public static void Save()
    {
        lock (_lock)
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(Users, Formatting.Indented));
        }
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    public static void Create()
    {
        if (Input.EnsureOutputNotRedirected())
        {
            return;
        }
        string name = AdvancedInput.InputNewUserName();
        User user = AdvancedInput.InputUser();

        if (!Add(name, user))
        {
            Logger.Warn("因字典key重复而创建失败");
            return;
        }

        Save();

        Logger.Info("创建成功");
        AnsiConsole.Write(
            new Table()
                .AddColumns(
                    new TableColumn("用户名") { Alignment = Justify.Center },
                    new(Markup.Escape(name)) { Alignment = Justify.Center }
                )
                .AddRow("权限等级", LevelNames[user.Level])
                .AddRow("密码", "*".PadRight(user.Password!.Length, '*'))
                .AddRow("描述", user.Description ?? string.Empty)
                .RoundedBorder()
        );
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    public static void Delete()
    {
        if (Input.EnsureOutputNotRedirected())
        {
            return;
        }

        string key = AdvancedInput.SelectUser("选择要删除的用户").Key;

        if (Users.Remove(key))
        {
            Logger.Info("删除成功");
            Save();
        }
        else
        {
            Logger.Warn("因字典key变更而删除失败");
        }
    }

    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="item">用户对象</param>
    /// <returns>添加结果</returns>
    public static bool Add(string name, User user)
    {
        if (Users.ContainsKey(name))
        {
            return false;
        }
        Users.Add(name, user);
        Save();
        return true;
    }

    /// <summary>
    /// 修改用户
    /// </summary>
    public static void Edit()
    {
        if (Input.EnsureOutputNotRedirected())
        {
            return;
        }

        KeyValuePair<string, User> kv = AdvancedInput.SelectUser("选择要编辑的用户");
        User user = AdvancedInput.EditUser(kv.Value);
        lock (Users)
        {
            if (!Users.ContainsKey(kv.Key))
            {
                Logger.Warn("因字典key丢失而修改失败");
            }
            else
            {
                Users[kv.Key] = user;
            }
        }
        Logger.Info("修改成功");
        Save();
    }

    /// <summary>
    /// 打印所有
    /// </summary>
    public static void PrintAll()
    {
        Table table = new();
        table.AddColumns("用户名", "用户等级", "上一次登录时间", "描述").RoundedBorder();

        table.Columns[0].Centered();
        table.Columns[1].Centered();
        table.Columns[2].Centered();
        table.Columns[3].Centered();
        lock (Users)
        {
            Users
                .ToList()
                .ForEach(
                    (kv) =>
                        table.AddRow(
                            kv.Key,
                            LevelNames[kv.Value.Level],
                            kv.Value.LastLoginTime?.ToString("g") ?? string.Empty,
                            kv.Value.Description ?? string.Empty
                        )
                );
            Logger.Info(string.Empty);
            AnsiConsole.Write(table);
        }
    }

    /// <summary>
    /// 修改用户实例
    /// </summary>
    public static void EditUserInstances()
    {
        var kv = AdvancedInput.SelectUser("选择要修改实例列表的用户");
        string key = kv.Key;
        User user = kv.Value;

        Dictionary<string, Instance> dict = user.Instances
            .Distinct()
            .Select(id => new KeyValuePair<string, Instance>(Guid.NewGuid().ToString("N"), new(id)))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        lock (MainWsModule.Instances)
        {
            foreach (KeyValuePair<string, Instance> online in MainWsModule.Instances)
            {
                foreach (KeyValuePair<string, Instance> loaded in dict)
                {
                    if (online.Key == loaded.Key)
                    {
                        dict.Remove(loaded.Key);
                        dict.Add(online.Key, online.Value);
                        break;
                    }
                }
            }
        }
        IEnumerable<KeyValuePair<string, Instance>> all = dict.Concat(MainWsModule.Instances)
            .Distinct();

        if (!all.Any())
        {
            Logger.Warn("没有在线实例且该用户的当前可使用实例为空");
            return;
        }

        try
        {
            user.Instances = Prompt
                .MultiSelect(
                    "选择实例",
                    all,
                    minimum: 0,
                    defaultValues: dict,
                    textSelector: (kv) => $"{kv.Key}\t自定义名称：{kv.Value?.CustomName}"
                )
                .Select(kv => kv.Key)
                .Where(instanceID => !string.IsNullOrEmpty(instanceID))
                .ToArray()!;
        }
        catch (PromptCanceledException)
        {
            return;
        }

        if (Users.ContainsKey(key))
        {
            Users[key] = user;
            Save();
            Logger.Info("修改权限成功");
        }
        else
        {
            Logger.Warn("因字典key丢失而修改失败");
        }
    }
}
