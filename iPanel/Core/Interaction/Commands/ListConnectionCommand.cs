using iPanel.Core.Server.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace iPanel.Core.Interaction.Commands;

[CommandDescription("list", "列出所有实例", Priority = 3)]
public class ListConnectionCommand : Command
{
    public ListConnectionCommand(IHost host)
        : base(host) { }

    private ILogger<ListConnectionCommand> Logger =>
        Services.GetRequiredService<ILogger<ListConnectionCommand>>();

    private InstanceWsModule InstanceWsModule => Services.GetRequiredService<InstanceWsModule>();

    public override void Parse(string[] args)
    {
        Logger.LogInformation("当前有{}个实例在线", InstanceWsModule.Instances.Count);

        var table = new Table().AddColumns("地址", "自定义名称", "实例信息").RoundedBorder();

        table.Columns[0].Centered();
        table.Columns[1].Centered();
        table.Columns[2].Centered();

        lock (InstanceWsModule.Instances)
            foreach (var kv in InstanceWsModule.Instances)
            {
                table.AddRow(
                    (kv.Value.Address ?? string.Empty).EscapeMarkup(),
                    (kv.Value.CustomName ?? string.Empty).EscapeMarkup(),
                    $"{kv.Value.Metadata?.Name ?? "未知名称"}({kv.Value.Metadata?.Version ?? "?"})".EscapeMarkup()
                );
            }

        AnsiConsole.Write(table);
    }
}
