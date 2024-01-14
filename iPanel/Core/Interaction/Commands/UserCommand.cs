using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using iPanel.Core.Models.Users;
using iPanel.Core.Service;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace iPanel.Core.Interaction.Commands;

[CommandDescription("user", "管理用户", Priority = 114514)]
[CommandUsage("user create/remove/edit", "操作用户（此功能需要可交互的终端）")]
[CommandUsage("user list", "列出所有用户")]
[CommandUsage(
    "user create <userName:string> <password:string> <level:enum/uint> [description:string?]",
    "创建用户"
)]
[CommandUsage(
    "user edit <userName:string> <password:string> <level:enum/uint> [description:string?]",
    "编辑指定用户"
)]
[CommandUsage("user remove <userName:string>", "删除指定用户")]
public class UserCommand : Command
{
    public UserCommand(IHost host)
        : base(host) { }

    private UserManager UserManager => Services.GetRequiredService<UserManager>();

    private ILogger<ListConnectionCommand> Logger =>
        Services.GetRequiredService<ILogger<ListConnectionCommand>>();

    private static readonly Dictionary<PermissionLevel, string> _levelDescription =
        new()
        {
            { PermissionLevel.Guest, "游客:   禁止登录" },
            { PermissionLevel.ReadOnly, "只读:   仅可查看" },
            { PermissionLevel.Assistant, "助手:   允许控制服务器" },
            { PermissionLevel.Administrator, "管理员: 允许控制服务器、新建修改删除用户" }
        };

    public override void Parse(string[] args)
    {
        if (args.Length < 2)
        {
            Logger.LogError("语法错误：缺少子命令（可用值：create/remove/edit/list）");
            return;
        }

        switch (args[1])
        {
            case "list":
                var table = new Table()
                    .AddColumns("用户名", "用户等级", "上一次登录时间", "最近登录IP", "描述")
                    .RoundedBorder();

                table.Columns[0].Centered();
                table.Columns[1].Centered();
                table.Columns[2].Centered();
                table.Columns[3].Centered();
                table.Columns[4].Centered();
                lock (UserManager.Users)
                {
                    Logger.LogInformation("当前共有{}个用户", UserManager.Users.Count);
                    foreach (var kv in UserManager.Users)
                        table.AddRow(
                            kv.Key,
                            UserManager.LevelNames[kv.Value.Level],
                            kv.Value.LastLoginTime?.ToString() ?? string.Empty,
                            (kv.Value.IPAddresses.FirstOrDefault() ?? string.Empty).EscapeMarkup(),
                            kv.Value.Description.EscapeMarkup()
                        );
                    AnsiConsole.Write(table);
                }
                break;

            case "create":
                if (args.Length == 2)
                    Create();
                else if (args.Length == 5 || args.Length == 6)
                    Create(args);
                else
                    Logger.LogError(
                        "语法错误：参数数量不正确。正确格式：\"user create <userName:string> <password:string> <level:enum/uint> [description:string?]\""
                    );
                break;

            case "edit":
                if (args.Length == 2)
                    Edit();
                else if (args.Length == 5 || args.Length == 6)
                    Edit(args);
                else
                    Logger.LogError(
                        "语法错误：参数数量不正确。正确格式：\"user edit <userName:string> <password:string> <level:enum/uint> [description:string?]\""
                    );
                break;

            case "remove":
                if (args.Length == 2)
                    Remove();
                else if (UserManager.Remove(args[2]))
                    Logger.LogInformation("删除成功");
                else
                    Logger.LogError("删除失败：用户不存在");

                break;

            default:
                Logger.LogError("语法错误：未知的子命令（可用值：create/remove/edit/list）");
                break;
        }
    }

    private void Create()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            Logger.LogError(
                "当前终端不可交互。请使用\"user create <userName:string> <password:string> <level:enum/uint> [description:string?]\""
            );
            return;
        }

        var name = InputNewUserName();
        var user = new User()
        {
            Password = InputPassword(),
            Level = SelectPermissionLevel(),
            Description = InputDescription(),
        };

        if (!UserManager.Add(name, user))
            Logger.LogError("创建失败");
        else
        {
            Logger.LogInformation("创建成功");
            AnsiConsole.Write(
                new Table()
                    .AddColumns(
                        new TableColumn("用户名") { Alignment = Justify.Center },
                        new(Markup.Escape(name)) { Alignment = Justify.Center }
                    )
                    .AddRow("权限等级", UserManager.LevelNames[user.Level])
                    .AddRow("密码", string.Empty.PadRight(user.Password!.Length, '*'))
                    .AddRow("描述", user.Description ?? string.Empty)
                    .RoundedBorder()
            );
        }
    }

    private void Create(string[] args)
    {
        if (!Regex.IsMatch(args[2], @"^[^\s\\""'@]{3,}$"))
            Logger.LogError("创建失败：用户名过短或含有特殊字符（\"'\\@）或空格");
        else if (args[3].Length <= 6)
            Logger.LogError("创建失败：密码长度至少为6");
        else if (
            UserManager.Add(
                args[2],
                new()
                {
                    Password = args[3],
                    Level = Enum.TryParse(args[4], true, out PermissionLevel permissionLevel)
                        ? permissionLevel
                        : throw new ArgumentException(
                            "无效的枚举值（可用值：Guest|ReadOnly|Assistant|Administrator）",
                            nameof(args)
                        ),
                    Description = args.Length == 6 ? args[5] : string.Empty
                }
            )
        )
            Logger.LogInformation("创建成功");
        else
            Logger.LogError("创建失败：用户名重复");
    }

    private void Edit()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            Logger.LogError(
                "当前终端不可交互。请使用\"user edit <userName:string> <password:string> <level:enum/uint> [description:string?]\""
            );
            return;
        }

        var kv = SelectUser(true);
        kv.Value.Password = InputPassword(kv.Value.Password);
        kv.Value.Level = SelectPermissionLevel();
        kv.Value.Description = InputDescription(kv.Value.Description);

        UserManager.Save();
        Logger.LogInformation("编辑成功");
    }

    private void Edit(string[] args)
    {
        if (!UserManager.Users.TryGetValue(args[2], out User? user))
            Logger.LogError("修改失败：用户不存在");
        else if (args[3].Length <= 6)
            Logger.LogError("修改失败：密码长度至少为6");
        else
        {
            user.Password = args[3];
            user.Level = Enum.TryParse(args[4], true, out PermissionLevel permissionLevel)
                ? permissionLevel
                : throw new ArgumentException(
                    "无效的枚举值（可用值：Guest|ReadOnly|Assistant|Administrator）",
                    nameof(args)
                );
            user.Description = args.Length == 6 ? args[5] : string.Empty;
            UserManager.Save();
            Logger.LogInformation("修改成功");
        }
    }

    private void Remove()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            Logger.LogError("当前终端不可交互。请使用\"user remove <userName>\"");
            return;
        }

        var kv = SelectUser(true);
        UserManager.Remove(kv.Key);
        UserManager.Save();
        Logger.LogInformation("删除成功");
    }

    private KeyValuePair<string, User> SelectUser(bool edit)
    {
        WriteDivider($"选择要{(edit ? "编辑" : "删除")}的用户");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<↑>[/] 和 [green]<↓>[/] 进行选择");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<Enter>[/] 或 [green]<Space>[/] 进行确认");
        AnsiConsole.WriteLine();

        var keyValuePair = AnsiConsole.Prompt(
            new SelectionPrompt<KeyValuePair<string, User>>()
                .AddChoices(UserManager.Users.ToArray())
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

    private static void WriteDivider(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[white]{text}[/]").RuleStyle("grey").LeftJustified());
    }

    private string InputNewUserName()
    {
        WriteDivider("用户名");
        AnsiConsole.MarkupLine("▪ 长度应大于3");
        AnsiConsole.MarkupLine("▪ 不含有空格[gray]（[underline] [/]）[/]");
        AnsiConsole.MarkupLine("▪ 不含有特殊字符[gray]（[underline]\"'\\@[/]）[/]");
        AnsiConsole.MarkupLine("▪ 不与已有的用户名重复");
        AnsiConsole.WriteLine();

        return AnsiConsole.Prompt(
            new TextPrompt<string>(">").Validate(
                (line) =>
                    line.Length >= 3
                    && Regex.IsMatch(line, @"^[^\s\\""'@]+$")
                    && !UserManager.Users.ContainsKey(line),
                "[red]用户名不合上述要求[/]"
            )
        );
    }

    private static string InputPassword(string? defaultPwd = null)
    {
        WriteDivider("输入密码");
        AnsiConsole.MarkupLine("▪ 长度应大于6");
        AnsiConsole.MarkupLine("▪ 不含有空格[gray]（[underline] [/]）[/]");
        AnsiConsole.MarkupLine("▪ 不含有敏感字符[gray]（[underline]\"'\\[/]）[/]");
        AnsiConsole.MarkupLine("▪ 不建议于其他密码相同[gray]（如服务器连接密码、QQ或微信密码）[/]");
        AnsiConsole.MarkupLine("▪ 推荐大小写字母数字结合");

        var prompt = new TextPrompt<string>(">");

        if (!string.IsNullOrEmpty(defaultPwd))
        {
            prompt.DefaultValue(defaultPwd).HideDefaultValue();
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

    private static PermissionLevel SelectPermissionLevel()
    {
        WriteDivider("用户权限");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<↑>[/] 和 [green]<↓>[/] 进行选择");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<Enter>[/] 或 [green]<Space>[/] 进行确认");
        AnsiConsole.WriteLine();

        var level = AnsiConsole.Prompt(
            new SelectionPrompt<PermissionLevel>()
                .AddChoices(_levelDescription.Keys)
                .UseConverter((level) => _levelDescription[level])
        );
        AnsiConsole.MarkupLine("[blue]{0}[/]", _levelDescription[level]);
        return level;
    }

    private static string InputDescription(string? defaultValue = null)
    {
        WriteDivider("描述（可选）");
        AnsiConsole.MarkupLine("▪ 你可以使用键盘的 [green]<Enter>[/] 选择跳过");
        AnsiConsole.WriteLine();

        var prompt = new TextPrompt<string>(string.Empty);
        prompt.AllowEmpty();
        prompt.HideDefaultValue();

        if (!string.IsNullOrEmpty(defaultValue))
        {
            prompt.DefaultValue(defaultValue);
        }
        return AnsiConsole.Prompt(prompt);
    }
}
