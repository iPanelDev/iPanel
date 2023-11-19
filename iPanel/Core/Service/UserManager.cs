using iPanel.Core.Models.Users;
using iPanel.Utils.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Text.Json;
using Swan.Logging;

namespace iPanel.Core.Service;

public class UserManager
{
    public static readonly IReadOnlyDictionary<PermissionLevel, string> LevelNames = new Dictionary<
        PermissionLevel,
        string
    >()
    {
        { PermissionLevel.Guest, "游客" },
        { PermissionLevel.ReadOnly, "只读" },
        { PermissionLevel.Assistant, "助手" },
        { PermissionLevel.Administrator, "管理员" }
    };

    private readonly Timer _timer = new(60_000) { AutoReset = true };

    public UserManager()
    {
        _timer.Elapsed += (_, _) => Save();
        _timer.Start();
    }

    public IReadOnlyDictionary<string, User> Users => _users;

    private const string _path = "data/users.json";

    private readonly object _lock = new();

    private Dictionary<string, User> _users = new();

    public void Read()
    {
        lock (_lock)
        {
            if (!File.Exists(_path))
            {
                Logger.Warn("用户列表为空");
                Save();
                return;
            }

            try
            {
                _users =
                    JsonSerializer.Deserialize<Dictionary<string, User>>(
                        File.ReadAllText(_path),
                        JsonSerializerOptionsFactory.CamelCase
                    ) ?? throw new FileLoadException("文件数据异常");

                if (_users.Count == 0)
                    Logger.Warn("用户列表为空");
            }
            catch (Exception e)
            {
                Logger.Error($"加载用户文件{_path}时出错");
                Logger.Error(e, nameof(UserManager), string.Empty);
            }
        }
    }

    public void Save()
    {
        lock (_lock)
        {
            File.WriteAllText(
                _path,
                JsonSerializer.Serialize(
                    Users,
                    options: new(JsonSerializerOptionsFactory.CamelCase) { WriteIndented = true }
                )
            );
        }
    }

    public bool Add(string name, User user)
    {
        if (Users.ContainsKey(name))
            return false;

        _users[name] = user;
        Save();
        return true;
    }

    public bool Remove(string name)
    {
        var result = _users.Remove(name);
        Save();
        return result;
    }
}
