using Sharprompt;
using Spectre.Console;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

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
            Logger.Warn($"嵌入文件“{_resourceKey}”丢失，已跳过");
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
            .AddColumns(new("文件名"), new TableColumn("大小") { Alignment = Justify.Right });

        string dir = Path.GetFullPath("dict");
        foreach (string fileName in Directory.GetFiles("dist", "*", SearchOption.AllDirectories))
        {
            table.AddRow(
                Markup.Escape(fileName.Replace(dir, string.Empty)),
                ((double)new FileInfo(fileName).Length / 1024).ToString("N1") + "KB"
            );
        }
        Logger.Info("嵌入文件解压完成");
        AnsiConsole.Write(table);
    }
}
