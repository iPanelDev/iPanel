using iPanel.Utils;
using Spectre.Console;
using Swan.Logging;
using System;
using System.IO;
using System.Reflection;

namespace iPanel.Core.Interaction.Parser;

[Command("version", "显示详细的版本和版权信息", Priority = -1)]
public class VersionParser : CommandParser
{
    public VersionParser(App app)
        : base(app) { }

    public override void Parse(string[] args)
    {
        Logger.Info("Copyright (C) 2023 iPanelDev. All rights reserved.");
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
                .AddRow("文件名", Path.GetFileName(commandlineArgs[0]))
                .AddRow("MD5", Encryption.GetMD5(File.ReadAllBytes(commandlineArgs[0])))
                .AddRow("创建时间", File.GetCreationTime(commandlineArgs[0]).ToString("o"))
                .AddRow("修改时间", File.GetLastWriteTime(commandlineArgs[0]).ToString("o"));

        versionTable
            .AddRow("发布许可证", "GPL - 3.0")
            .AddRow("文档", "https://ipaneldev.github.io/")
            .AddRow("GitHub仓库", "https://github.com/iPanelDev/iPanel");

        AnsiConsole.Write(versionTable);
    }
}
