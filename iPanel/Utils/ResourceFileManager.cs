using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace iPanel.Utils;

public class ResourceFileManager
{
    private IServiceProvider Services => _host.Services;
    private ILogger<ResourceFileManager> Logger =>
        Services.GetRequiredService<ILogger<ResourceFileManager>>();
    private const string _resourceKey = "iPanel.Sources.dist.zip";
    private readonly IHost _host;

    public ResourceFileManager(IHost host)
    {
        _host = host;
    }

    public void Release()
    {
        if (Directory.Exists("dist"))
            return;

        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_resourceKey);
        if (stream is null)
        {
            Logger.LogWarning(
                "嵌入文件“{}”丢失。此文件为此软件对应版本的前端网页的压缩包，请自行到 https://github.com/iPanelDev/WebConsole/releases/latest 下载最新的版本，解压后放在“dist”文件夹下并重启",
                _resourceKey
            );
            return;
        }

        Directory.CreateDirectory("dist");

        var fileStream = new FileStream(_resourceKey, FileMode.Create);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        fileStream.Write(bytes, 0, bytes.Length);
        fileStream.Close();

        Logger.LogInformation("嵌入文件“{}”已释放", _resourceKey);
        ZipFile.ExtractToDirectory(_resourceKey, "dist");

        var table = new Table()
            .RoundedBorder()
            .AddColumns(new("文件名"), new TableColumn("大小") { Alignment = Justify.Right })
            .Expand();

        var dir = Path.GetFullPath("dict");
        foreach (string fileName in Directory.GetFiles("dist", "*", SearchOption.AllDirectories))
        {
            var color = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".js" => "yellow",
                ".html" => "cadetblue",
                ".css" => "dodgerblue1",
                ".ico" => "darkorange",
                ".png" => "mediumpurple",
                _ => null
            };

            table.AddRow(
                Markup.Escape(Path.GetDirectoryName(fileName)!.Replace(dir, string.Empty))
                    + (
                        color is null
                            ? Path.DirectorySeparatorChar
                                + Markup.Escape(Path.GetFileName(fileName))
                            : $"{Path.DirectorySeparatorChar}[{color}]{Markup.Escape(Path.GetFileName(fileName))}[/]"
                    ),
                ((double)new FileInfo(fileName).Length / 1024).ToString("N1") + "KB"
            );
        }

        Logger.LogInformation("嵌入文件解压完成");
        AnsiConsole.Write(table);
    }
}
