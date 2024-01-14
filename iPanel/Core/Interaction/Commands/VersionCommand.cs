using System;
using System.IO;
using System.Reflection;

using iPanel.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace iPanel.Core.Interaction.Commands;

[CommandDescription("version", "显示详细的版本和版权信息", Priority = -1)]
public class VersionCommand : Command
{
    public VersionCommand(IHost host)
        : base(host) { }

    private ILogger<VersionCommand> Logger =>
        Services.GetRequiredService<ILogger<VersionCommand>>();

    public override void Parse(string[] args)
    {
        Logger.LogInformation("Copyright (C) 2023 iPanelDev. All rights reserved.");
        var versionTable = new Table()
            .RoundedBorder()
            .AddColumns(
                new TableColumn("名称") { Alignment = Justify.Center },
                new(Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty)
                {
                    Alignment = Justify.Center
                }
            )
            .AddRow(
                "版本",
                Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty
            );
        var commandlineArgs = Environment.GetCommandLineArgs();
        if (commandlineArgs.Length > 0 && File.Exists(commandlineArgs[0]))
            versionTable
                .AddRow("文件名", Path.GetFileName(commandlineArgs[0]).EscapeMarkup())
                .AddRow("MD5", Encryption.GetMD5(File.ReadAllBytes(commandlineArgs[0])))
                .AddRow("创建时间", File.GetCreationTime(commandlineArgs[0]).ToString("o"))
                .AddRow("修改时间", File.GetLastWriteTime(commandlineArgs[0]).ToString("o"));

        AnsiConsole.Write(versionTable);
    }
}
