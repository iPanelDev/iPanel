using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

using iPanel.Core.Models.Settings;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace iPanel.Utils;

public class ResourceFileManager
{
    private IServiceProvider Services => _host.Services;
    private ILogger<ResourceFileManager> Logger =>
        Services.GetRequiredService<ILogger<ResourceFileManager>>();
    private Setting Setting => Services.GetRequiredService<Setting>();
    private const string _resourceKey = "iPanel.Sources.webconsole.zip";
    private readonly IHost _host;

    public ResourceFileManager(IHost host)
    {
        _host = host;
    }

    public void Release()
    {
        if (Directory.Exists(Setting.WebServer.Directory))
            return;

        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_resourceKey);
        if (stream is null)
        {
            Logger.LogWarning(
                "嵌入文件“{}”丢失，请自行检查编译环境。此文件为此软件对应版本的前端网页的压缩包，请自行到 https://github.com/iPanelDev/WebConsole/releases/latest 下载最新版本，解压后放在“{}”文件夹下并重启",
                _resourceKey,
                Setting.WebServer.Directory
            );
            return;
        }

        Directory.CreateDirectory(Setting.WebServer.Directory);

        var fileStream = new FileStream(_resourceKey, FileMode.Create);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        fileStream.Write(bytes, 0, bytes.Length);
        fileStream.Close();

        Logger.LogInformation("嵌入文件“{}”已释放", _resourceKey);
        ZipFile.ExtractToDirectory(_resourceKey, Setting.WebServer.Directory);

        var table = new Table()
            .RoundedBorder()
            .AddColumns(new("文件名"), new TableColumn("大小") { Alignment = Justify.Right })
            .Expand();

        var dir = Path.GetFullPath(Setting.WebServer.Directory);
        foreach (
            string fileName in Directory.GetFiles(
                Setting.WebServer.Directory,
                "*",
                SearchOption.AllDirectories
            )
        )
        {
            table.AddRow(
                Markup.Escape(Path.GetDirectoryName(fileName)!.Replace(dir, string.Empty))
                    + Path.DirectorySeparatorChar
                    + Markup.Escape(Path.GetFileName(fileName)),
                ((double)new FileInfo(fileName).Length / 1024).ToString("N1") + "KB"
            );
        }

        Logger.LogInformation("嵌入文件解压完成");
        AnsiConsole.Write(table);
    }
}
