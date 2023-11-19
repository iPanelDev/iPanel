using System.Collections.Generic;
using iPanel.Core.Service;
using Spectre.Console;
using Swan.Logging;
using System.Linq;
using System.Text.RegularExpressions;
using iPanel.Core.Models.Users;
using System;

namespace iPanel.Core.Interaction.Parser;

[Command("user", "管理用户", Priority = 5)]
public class UserParser : CommandParser
{
    public UserParser(App app)
        : base(app) { }

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
            Logger.Error("语法错误：缺少子命令（可用值：add/remove/edit/list）");
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
                lock (_app.UserManager.Users)
                {
                    Logger.Info($"当前共有{_app.UserManager.Users.Count}个用户");
                    foreach (var kv in _app.UserManager.Users)
                        table.AddRow(
                            kv.Key,
                            UserManager.LevelNames[kv.Value.Level],
                            kv.Value.LastLoginTime?.ToString() ?? string.Empty,
                            kv.Value.IPAddresses.FirstOrDefault() ?? string.Empty,
                            kv.Value.Description ?? string.Empty
                        );
                    AnsiConsole.Write(table);
                }
                break;

            case "add":
                if (args.Length == 2)
                    Add();
                else if (args.Length == 5 || args.Length == 6)
                    Add(args);
                else
                    Logger.Error(
                        "语法错误：参数数量不正确。正确格式：\"user add <userName:string> <password:string> <level:enum/uint> [description:string?]\""
                    );
                break;

            case "edit":
                if (args.Length == 2)
                    Edit();
                else if (args.Length == 5 || args.Length == 6)
                    Edit(args);
                else
                    Logger.Error(
                        "语法错误：参数数量不正确。正确格式：\"user edit <userName:string> <password:string> <level:enum/uint> [description:string?]\""
                    );
                break;

            case "remove":
                if (args.Length == 2)
                    Remove();
                else if (_app.UserManager.Remove(args[2]))
                    Logger.Info("删除成功");
                else
                    Logger.Error("删除失败：用户不存在");

                break;

            default:
                Logger.Error("语法错误：未知的子命令（可用值：add/remove/edit/list）");
                break;
        }
    }

    private void Add()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            Logger.Error(
                "当前终端不可交互。请使用\"user add <userName:string> <password:string> <level:enum/uint> [description:string?]\""
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

        if (!_app.UserManager.Add(name, user))
            Logger.Error("添加失败");
        else
        {
            Logger.Info("添加成功");
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

    private void Add(string[] args)
    {
        if (!Regex.IsMatch(args[2], @"^[^\s\\""'@]{3,}$"))
            Logger.Error("添加失败：用户名含有特殊字符（\"'\\@）或空格");
        else if (args[3].Length < 3)
            Logger.Error("添加失败：密码长度应大于3");
        else if (
            _app.UserManager.Add(
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
                    Description = args.Length == 6 ? args[5] : null
                }
            )
        )
            Logger.Info("添加成功");
        else
            Logger.Error("添加失败：用户名重复");
    }

    private void Edit()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            Logger.Error(
                "当前终端不可交互。请使用\"user edit <userName:string> <password:string> <level:enum/uint> [description:string?]\""
            );
            return;
        }

        var kv = SelectUser(true);
        kv.Value.Password = InputPassword(kv.Value.Password);
        kv.Value.Level = SelectPermissionLevel();
        kv.Value.Description = InputDescription(kv.Value.Description);

        _app.UserManager.Save();
        Logger.Info("编辑成功");
    }

    private void Edit(string[] args)
    {
        if (!_app.UserManager.Users.TryGetValue(args[2], out User? user))
            Logger.Error("修改失败：用户不存在");
        else if (args[3].Length < 3)
            Logger.Error("修改失败：密码长度应大于3");
        else
        {
            user.Password = args[3];
            user.Level = Enum.TryParse(args[4], true, out PermissionLevel permissionLevel)
                ? permissionLevel
                : throw new ArgumentException(
                    "无效的枚举值（可用值：Guest|ReadOnly|Assistant|Administrator）",
                    nameof(args)
                );
            user.Description = args.Length == 6 ? args[5] : null;
            _app.UserManager.Save();
            Logger.Info("修改成功");
        }
    }

    private void Remove()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            Logger.Error("当前终端不可交互。请使用\"user remove <userName>\"");
            return;
        }

        var kv = SelectUser(true);
        _app.UserManager.Remove(kv.Key);
        _app.UserManager.Save();
        Logger.Info("删除成功");
    }

    private KeyValuePair<string, User> SelectUser(bool edit)
    {
        WriteDivider($"选择要{(edit ? "编辑" : "删除")}的用户");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<↑>[/] 和 [green]<↓>[/] 进行选择");
        AnsiConsole.MarkupLine("▪ 使用键盘的 [green]<Enter>[/] 或 [green]<Space>[/] 进行确认");
        AnsiConsole.WriteLine();

        var keyValuePair = AnsiConsole.Prompt(
            new SelectionPrompt<KeyValuePair<string, User>>()
                .AddChoices(_app.UserManager.Users.ToArray())
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
                    && !_app.UserManager.Users.ContainsKey(line),
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
