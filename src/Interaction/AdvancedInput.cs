using System.Collections.Generic;
using System.Text.RegularExpressions;
using iPanelHost.Base;
using iPanelHost.Service;
using Sharprompt;
using Spectre.Console;

namespace iPanelHost.Interaction;

public static class AdvancedInput
{
    private static readonly Dictionary<PermissionLevel, string> _levelDescription =
        new()
        {
            { PermissionLevel.Guest, "游客:   禁止登录" },
            { PermissionLevel.ReadOnly, "只读:   仅可查看" },
            { PermissionLevel.Assistant, "助手:   允许控制服务器" },
            { PermissionLevel.Administrator, "管理员: 允许控制服务器、新建修改删除用户" }
        };

    /// <summary>
    /// 输入一个用户
    /// </summary>
    public static User InputUser() =>
        new()
        {
            Password = InputPassword(),
            Level = SelectPermission(),
            Description = InputDescription(),
        };

    /// <summary>
    /// 编辑用户
    /// </summary>
    /// <param name="user">旧用户</param>
    /// <returns>新用户</returns>
    public static User EditUser(User user) =>
        new()
        {
            Password = InputPassword(user.Password),
            Level = SelectPermission(),
            Description = InputDescription(user.Description),
        };

    /// <summary>
    /// 输入用户名
    /// </summary>
    /// <returns>用户名</returns>
    public static string InputNewUserName()
    {
        WriteDivider("用户名");
        AnsiConsole.MarkupLine("▪ 长度应大于3");
        AnsiConsole.MarkupLine("▪ 不含有空格[gray]（[underline] [/]）[/]");
        AnsiConsole.MarkupLine("▪ 不含有敏感字符[gray]（[underline]\"'\\[/]）[/]");
        AnsiConsole.MarkupLine("▪ 不与已有的用户名重复");
        AnsiConsole.WriteLine();

        return AnsiConsole.Prompt(
            new TextPrompt<string>(">").Validate(
                (line) =>
                    line.Length >= 3
                    && Regex.IsMatch(line, @"^[^\s\\""']+$")
                    && !UserManager.Users.ContainsKey(line),
                "[red]用户名不合上述要求[/]"
            )
        );
    }

    /// <summary>
    /// 输入用户的密码
    /// </summary>
    /// <param name="default">默认值</param>
    /// <returns>新的密码</returns>
    private static string InputPassword(string? @default = null)
    {
        WriteDivider("输入密码");
        AnsiConsole.MarkupLine("▪ 长度应大于6");
        AnsiConsole.MarkupLine("▪ 不含有空格[gray]（[underline] [/]）[/]");
        AnsiConsole.MarkupLine("▪ 不含有敏感字符[gray]（[underline]\"'\\[/]）[/]");
        AnsiConsole.MarkupLine("▪ 不建议于其他密码相同[gray]（如服务器连接密码、QQ或微信密码）[/]");
        AnsiConsole.MarkupLine("▪ 推荐大小写字母数字结合");

        TextPrompt<string> prompt = new(">");

        if (!string.IsNullOrEmpty(@default))
        {
            prompt.DefaultValue(@default).HideDefaultValue();
            AnsiConsole.MarkupLine("▪ 你可以使用键盘的 [green]<Enter>[/] 选择跳过");
        }

        AnsiConsole.WriteLine();
        prompt
            .Secret()
            .Validate(
                (line) => line.Length >= 6 && Regex.IsMatch(line, @"^[^\s\\""']+$"),
                "[red]密码不合上述要求[/]"
            );

        return AnsiConsole.Prompt(prompt);
    }

    /// <summary>
    /// 选择权限
    /// </summary>
    /// <returns>权限等级</returns>
    private static PermissionLevel SelectPermission()
    {
        WriteDivider("用户权限");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<↑>[/] 和 [green]<↓>[/] 进行选择");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<Enter>[/] 或 [green]<Space>[/] 进行确认");
        AnsiConsole.WriteLine();

        PermissionLevel level = AnsiConsole.Prompt(
            new SelectionPrompt<PermissionLevel>()
                .AddChoices(_levelDescription.Keys)
                .UseConverter((level) => _levelDescription[level])
        );
        AnsiConsole.MarkupLine("[blue]{0}[/]", _levelDescription[level]);
        return level;
    }

    /// <summary>
    /// 输入用户的描述文本
    /// </summary>
    /// <param name="default">默认值</param>
    /// <returns>新的描述文本</returns>
    private static string InputDescription(string? @default = null)
    {
        WriteDivider("描述（可选）");
        AnsiConsole.MarkupLine("▪ 你可以使用键盘的 [green]<Enter>[/] 选择跳过");
        AnsiConsole.WriteLine();

        TextPrompt<string> prompt = new(string.Empty);
        prompt.AllowEmpty();
        prompt.HideDefaultValue();

        if (!string.IsNullOrEmpty(@default))
        {
            prompt.DefaultValue(@default);
        }
        return AnsiConsole.Prompt(prompt);
    }

    /// <summary>
    /// 选择用户
    /// </summary>
    /// <param name="title">标题</param>
    /// <returns>用户键值对</returns>
    public static KeyValuePair<string, User> SelectUser(string title)
    {
        WriteDivider(title);
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<↑>[/] 和 [green]<↓>[/] 进行选择");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<Enter>[/] 或 [green]<Space>[/] 进行确认");
        AnsiConsole.WriteLine();

        KeyValuePair<string, User> keyValuePair = AnsiConsole.Prompt(
            new SelectionPrompt<KeyValuePair<string, User>>()
                .AddChoices(UserManager.Users)
                .UseConverter(
                    (kv) =>
                        $"{Markup.Escape(kv.Key)} [gray]({UserManager.LevelNames[kv.Value.Level]})[/]"
                )
        );

        AnsiConsole.MarkupLine(
            $"[blue]{Markup.Escape(keyValuePair.Key)} ({UserManager.LevelNames[keyValuePair.Value.Level]})[/]"
        );
        return keyValuePair;
    }

    /// <summary>
    /// 绘制分割线
    /// </summary>
    /// <param name="text">文本</param>
    private static void WriteDivider(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[white]{text}[/]").RuleStyle("grey").LeftJustified());
    }

    public static Setting NewSetting
    {
        get
        {
            bool toPublic = Prompt.Confirm("将Http服务器开放到公网", false);
            int port = Prompt.Input<int>(
                "Http服务器的端口",
                30000,
                "1~65535",
                new[]
                {
                    (object obj) =>
                        obj is int value && value > 0 && value <= 65535
                            ? System.ComponentModel.DataAnnotations.ValidationResult.Success
                            : new("端口无效")
                }
            );

            return new()
            {
                InstancePassword = Prompt.Password(
                    "实例连接密码",
                    placeholder: "不要与QQ或服务器等密码重复；推荐大小写字母数字结合",
                    validators: new[]
                    {
                        Validators.Required("密码不可为空"),
                        Validators.MinLength(6, "密码长度过短"),
                        Validators.RegularExpression(@"^[^\s]+$", "密码不得含有空格"),
                    }
                ),
                WebServer = new()
                {
                    UrlPrefixes = new[] { $"http://{(toPublic ? "+" : "127.0.0.1")}:{port}" },
                    AllowCrossOrigin = Prompt.Confirm("允许跨源资源共享（CORS）", false)
                }
            };
        }
    }
}
