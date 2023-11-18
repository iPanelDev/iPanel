using iPanel.Core.Service;
using Spectre.Console;
using Swan.Logging;
using System.Linq;

namespace iPanel.Core.Interaction.Parser;

[CommandParser("user", "管理用户", Priority = 5)]
public class UserParser : CommandParser
{
    public UserParser(App app)
        : base(app) { }

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
            case "edit":
            case "remove":

            default:
                Logger.Error("语法错误：未知的子命令（可用值：add/remove/edit/list）");
                break;
        }
    }
}
