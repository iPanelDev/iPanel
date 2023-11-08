using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Sharprompt;
using Spectre.Console;

namespace iPanelHost.Utils;

public static class Initialization
{
    /// <summary>
    /// 初始化环境
    /// </summary>
    public static void InitEnv()
    {
        // 基础
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        CrashInterception.Init();

        // 控制台
        Console.BackgroundColor = ConsoleColor.Black;
        Console.OutputEncoding = Encoding.UTF8;
        Console.CancelKeyPress += HandleCancelEvent;
        Win32.EnableVirtualTerminal();

        // Logger
        Swan.Logging.Logger.UnregisterLogger<Swan.Logging.ConsoleLogger>();
        Swan.Logging.Logger.RegisterLogger<Logger>();

        // Prompt输入设置
        Prompt.ThrowExceptionOnCancel = true;
        Prompt.Symbols.Done = new("√", "V");
        Prompt.ColorSchema.PromptSymbol = ConsoleColor.Blue;
        Prompt.ColorSchema.Select = ConsoleColor.DarkGray;
        Prompt.ColorSchema.Answer = ConsoleColor.Gray;
    }

    /// <summary>
    /// 上一次触发时间
    /// </summary>
    private static DateTime _lastTime;

    /// <summary>
    /// 处理Ctrl+C事件
    /// </summary>
    public static void HandleCancelEvent(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        Console.WriteLine("^C");
        if ((DateTime.Now - _lastTime).TotalSeconds > 1)
        {
            Logger.Warn("请在1s内再次按下`Ctrl+C`以退出。");
            _lastTime = DateTime.Now;
        }
        else
        {
            Runtime.Exit();
        }
    }

    private const string _resourceKey = "iPanel-Host.Sources.dist.zip";

    /// <summary>
    /// 释放资源文件
    /// </summary>
    public static void ReleaseResourceFile()
    {
        if (Directory.Exists("dist"))
        {
            return;
        }
        Directory.CreateDirectory("dist");

        Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_resourceKey);
        if (stream is null)
        {
            Logger.Warn(
                $"嵌入文件“{_resourceKey}”丢失，已跳过提取和解压。此文件为此软件对应版本的前端网页的压缩包，请自行到 https://github.com/iPanelDev/WebConsole/releases/latest 下载最新的版本，解压后放在“dist”文件夹下并重启"
            );
            return;
        }

        FileStream fileStream = new(_resourceKey, FileMode.Create);
        byte[] bytes = new byte[stream.Length];
        _ = stream.Read(bytes, 0, bytes.Length);
        fileStream.Write(bytes, 0, bytes.Length);
        fileStream.Close();
        Logger.Info($"嵌入文件“{_resourceKey}”已释放");

        ZipFile.ExtractToDirectory(_resourceKey, "dist");

        Table table = new();
        table
            .RoundedBorder()
            .AddColumns(new("文件名"), new TableColumn("大小") { Alignment = Justify.Right })
            .Expand();

        string dir = Path.GetFullPath("dict");
        foreach (string fileName in Directory.GetFiles("dist", "*", SearchOption.AllDirectories))
        {
            string? color = Path.GetExtension(fileName).ToLowerInvariant() switch
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

        Logger.Info("嵌入文件解压完成");
        AnsiConsole.Write(table);
    }
}
