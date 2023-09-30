using Sharprompt;
using System;
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
}
