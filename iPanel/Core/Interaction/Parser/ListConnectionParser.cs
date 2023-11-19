using Spectre.Console;
using Swan.Logging;

namespace iPanel.Core.Interaction.Parser;

[Command("list", "列出所有实例", Priority = 3)]
public class ListConnectionParser : CommandParser
{
    public ListConnectionParser(App app)
        : base(app) { }

    public override void Parse(string[] args)
    {
        Logger.Info($"当前有{_app.HttpServer.InstanceWsModule.Instances.Count}个实例在线");

        var table = new Table().AddColumns("地址", "自定义名称", "实例信息").RoundedBorder();

        table.Columns[0].Centered();
        table.Columns[1].Centered();
        table.Columns[2].Centered();

        lock (_app.HttpServer.InstanceWsModule.Instances)
            foreach (var kv in _app.HttpServer.InstanceWsModule.Instances)
            {
                table.AddRow(
                    kv.Value.Address ?? string.Empty,
                    kv.Value.CustomName ?? string.Empty,
                    $"{kv.Value.Metadata?.Name ?? "未知名称"}({kv.Value.Metadata?.Version ?? "?"})"
                );
            }

        AnsiConsole.Write(table);
    }
}
