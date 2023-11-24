using iPanel.Core.Models.Users;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using iPanel.Utils.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

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

    private Dictionary<string, User> _users = new();
    public IReadOnlyDictionary<string, User> Users => _users;

    private readonly Timer _timer = new(60_000) { AutoReset = true };

    public UserManager(IHost host)
    {
        _host = host;
        _timer.Elapsed += (_, _) => Save();
        _timer.Start();
    }

    private const string _path = "data/users.json";

    private readonly object _lock = new();
    private readonly IHost _host;

    private IServiceProvider Services => _host.Services;
    private ILogger<UserManager> Logger => Services.GetRequiredService<ILogger<UserManager>>();

    public void Read()
    {
        Directory.CreateDirectory("data");
        lock (_lock)
        {
            if (!File.Exists(_path))
            {
                Logger.LogWarning("用户列表为空，请使用\"user create\"创建一个用户");
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
                    Logger.LogWarning("用户列表为空，请使用\"user create\"创建一个用户");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "加载用户文件{}时出错", _path);
            }
        }
    }

    public void Save()
    {
        Directory.CreateDirectory("data");
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

    public static bool ValidatePassword(
        [NotNullWhen(true)] string? password,
        bool ignoreNull,
        [NotNullWhen(false)] out string? message
    )
    {
        message = null;
        if (ignoreNull && password is null)
            return true;

        if (password is null || password.Length < 6)
        {
            message = "密码长度过短";
            return false;
        }

        if (
            password.Contains('\\')
            || password.Contains('"')
            || password.Contains('\'')
            || password.Contains(' ')
            || ContainsControlChars(password)
        )
        {
            message = "密码不得含有特殊字符";
            return false;
        }
        return true;
    }

    public static bool ValidateUserName(
        [NotNullWhen(true)] string? userName,
        [NotNullWhen(false)] out string? message
    )
    {
        message = null;

        if (userName is null || userName.Length < 3)
        {
            message = "用户名长度过短";
            return false;
        }

        if (
            userName.Contains('\\')
            || userName.Contains('"')
            || userName.Contains('\'')
            || userName.Contains('@')
            || userName.Contains(' ')
            || ContainsControlChars(userName)
        )
        {
            message = "用户名不得含有特殊字符";
            return false;
        }
        return true;
    }

    private static bool ContainsControlChars(string text)
    {
        foreach (var c in text)
            if (c <= 31 || c == 127)
                return true;
        return false;
    }
}
