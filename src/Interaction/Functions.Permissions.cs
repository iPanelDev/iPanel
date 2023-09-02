using ConsoleTables;
using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Service;
using iPanelHost.Service.Handlers;
using iPanelHost.Utils;
using Sharprompt;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System;

namespace iPanelHost.Interaction;

public static partial class Funcions
{
    private static User NewUser =>
        new()
        {
            Password = Prompt.Password(
                "密码",
                placeholder: "不要与QQ或服务器等密码重复；推荐大小写字母数字结合",
                validators: new[]
                {
                    Validators.Required("密码不可为空"),
                    Validators.MinLength(6, "密码长度过短"),
                    Validators.RegularExpression(@"^[^\s]+$", "密码不得含有空格"),
                }
            ),
            Level = (PermissonLevel)
                Prompt
                    .Select("权限等级", UserManager.LevelDescription, textSelector: (kv) => kv.Value)
                    .Key,
            Description = Prompt.Input<string>("描述", placeholder: "（可选）")?.Trim()
        };

    private static string SelectUser =>
        Prompt
            .Select(
                "选择一个用户",
                UserManager.Users,
                textSelector: (kv) =>
                    $"{kv.Key}\t权限等级: {kv.Value.Level} 上次登录时间: {kv.Value.LastLoginTime}"
            )
            .Key;

    public static void ManageUsers(string[] args)
    {
        if (args.Length == 1)
        {
            Logger.Warn("缺少参数");
            return;
        }

        switch (args[1].ToLowerInvariant())
        {
            case "a":
            case "add":
                AddUser();
                break;

            case "r":
            case "reload":
                UserManager.Read();
                Logger.Info("重新加载成功");
                break;

            case "p":
            case "perm":
                EditUserInstances();
                break;

            case "d":
            case "delete":
                DeleteUser();
                break;

            case "ls":
            case "list":
                lock (UserManager.Users)
                {
                    ConsoleTable consoleTable =
                        new("Account", "Level", "Last Login Time", "Description");
                    foreach (var keyValuePair in UserManager.Users)
                    {
                        consoleTable.AddRow(
                            keyValuePair.Key,
                            keyValuePair.Value.Level,
                            keyValuePair.Value.LastLoginTime?.ToString("g") ?? "-",
                            keyValuePair.Value.Description ?? "-"
                        );
                    }
                    Logger.Info(consoleTable.ToMinimalString());
                }
                break;

            case "e":
            case "edit":
                EditUser();
                break;

            default:
                Logger.Warn("参数<operation>无效");
                break;
        }
    }

    /// <summary>
    /// 添加用户
    /// </summary>
    private static void AddUser()
    {
        try
        {
            if (
                !UserManager.Add(
                    Prompt.Input<string>(
                        "帐号",
                        validators: new[]
                        {
                            Validators.Required("帐号不可为空"),
                            Validators.RegularExpression(@"^[^\s]+$", "帐号不得含有空格"),
                            Validators.MinLength(3, "帐号长度过短"),
                            Validators.RegularExpression(
                                @"^[\w\.-@#\$%]+$",
                                "帐号不得含有除[A-Za-z0-9._-@#$%]以外的字符"
                            ),
                            (value) =>
                                value is string account && !UserManager.Users.ContainsKey(account)
                                    ? ValidationResult.Success
                                    : new("帐号已存在")
                        }
                    ),
                    NewUser
                )
            )
            {
                Logger.Warn("因字典key重复而创建失败");
                return;
            }
            UserManager.Save();
            Logger.Info("创建成功");
        }
        catch (PromptCanceledException)
        {
            return;
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    private static void DeleteUser()
    {
        try
        {
            string key = SelectUser;

            if (UserManager.Users.Remove(key))
            {
                Logger.Info("删除成功");
                UserManager.Save();
            }
            else
            {
                Logger.Warn("因字典key变更而删除失败");
            }
        }
        catch (PromptCanceledException)
        {
            return;
        }
    }

    /// <summary>
    /// 修改用户
    /// </summary>
    private static void EditUser()
    {
        try
        {
            string key = Prompt
                .Select(
                    "选择要修改的用户",
                    UserManager.Users,
                    textSelector: (kv) =>
                        $"{kv.Key}\t权限等级: {kv.Value.Level} 上次登录时间: {kv.Value.LastLoginTime:d t}"
                )
                .Key;
            User user = NewUser;
            lock (UserManager.Users)
            {
                if (!UserManager.Users.ContainsKey(key))
                {
                    Logger.Warn("因字典key丢失而修改失败");
                }
                else
                {
                    UserManager.Users[key] = user;
                }
            }
        }
        catch (PromptCanceledException)
        {
            return;
        }
        Logger.Info("修改成功");
        UserManager.Save();
    }

    /// <summary>
    /// 修改用户实例
    /// </summary>
    private static void EditUserInstances()
    {
        try
        {
            string key = SelectUser;

            if (!UserManager.Users.TryGetValue(key, out User? user))
            {
                Logger.Warn("因字典key丢失而修改失败");
                return;
            }

            Dictionary<string, Instance> dict = user.Instances
                .Distinct()
                .Select(
                    i =>
                        new KeyValuePair<string, Instance>(
                            Guid.NewGuid().ToString("N"),
                            new(string.Empty) { InstanceID = i }
                        )
                )
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            lock (MainHandler.Instances)
            {
                foreach (KeyValuePair<string, Instance> online in MainHandler.Instances)
                {
                    foreach (KeyValuePair<string, Instance> loaded in dict)
                    {
                        if (online.Value.InstanceID == loaded.Value.InstanceID)
                        {
                            dict.Remove(loaded.Key);
                            dict.Add(online.Key, online.Value);
                            break;
                        }
                    }
                }
            }
            IEnumerable<KeyValuePair<string, Instance>> all = dict.Concat(MainHandler.Instances)
                .Distinct();

            if (!all.Any())
            {
                Logger.Warn("没有在线实例且该用户的当前可使用实例为空");
                return;
            }

            user.Instances = Prompt
                .MultiSelect(
                    "选择实例",
                    all,
                    minimum: 0,
                    defaultValues: dict,
                    textSelector: (kv) =>
                        $"{kv.Value.InstanceID}({kv.Value?.Address ?? "-"}) \t自定义名称：{kv.Value?.CustomName}"
                )
                .Select(kv => kv.Value.InstanceID)
                .Where(instanceID => !string.IsNullOrEmpty(instanceID))
                .ToArray()!;

            if (UserManager.Users.ContainsKey(key))
            {
                UserManager.Users[key] = user;
                UserManager.Save();
                Logger.Info("修改权限成功");
            }
            else
            {
                Logger.Warn("因字典key丢失而修改失败");
                return;
            }
        }
        catch (PromptCanceledException)
        {
            return;
        }
    }
}
