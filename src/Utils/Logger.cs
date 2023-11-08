using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Swan.Logging;

namespace iPanelHost.Utils;

public class Logger : ILogger
{
    public LogLevel LogLevel => LogLevel.Info;

    private static readonly object _lock = new();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void Log(LogMessageReceivedEventArgs e)
    {
        string line = string.Empty;
        line += e.Message;
        if (e.Exception is null)
        {
            Log(e.MessageType, line);
            return;
        }
        Log(
            e.MessageType,
            $"{e.Exception.InnerException?.Message ?? e.Exception.Message}\n  in {e.CallerFilePath} L{e.CallerLineNumber}"
        );
    }

    private static void Log(LogLevel type, string line)
    {
        switch (type)
        {
            case LogLevel.Debug:
                Debug(line);
                break;

            case LogLevel.Info:
                Info(line);
                break;

            case LogLevel.Warning:
                Warn(line);
                break;

            case LogLevel.Fatal:
                Fatal(line);
                break;

            case LogLevel.Error:
                Error(line);
                break;

            default:
                return;
        }
    }

    public static void Info() => Info(string.Empty);

    public static void Info(string line)
    {
        if (line.Contains('\n'))
        {
            line.Split('\n').ToList().ForEach((singleLine) => Info(singleLine.Trim('\r')));
            return;
        }
        lock (_lock)
            Console.WriteLine($"{DateTime.Now:T} \x1b[96m[Info]\x1b[0m {line}");
    }

    public static void Warn(string line)
    {
        if (line.Contains('\n'))
        {
            line.Split('\n').ToList().ForEach((singleLine) => Warn(singleLine.Trim('\r')));
            return;
        }
        lock (_lock)
            Console.WriteLine($"{DateTime.Now:T} \x1b[33m[Warn] {line}\x1b[0m");
    }

    public static void Error(
        Exception e,
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1
    )
    {
        Error($"{e.InnerException?.Message ?? e.Message}\n  in {path} L{line}");
    }

    public static void Error(string line)
    {
        if (line.Contains('\n'))
        {
            line.Split('\n').ToList().ForEach((singleLine) => Error(singleLine.Trim('\r')));
            return;
        }
        lock (_lock)
            Console.WriteLine($"{DateTime.Now:T} \x1b[91m[Error]{line}\x1b[0m");
    }

    public static void Fatal(string line)
    {
        if (line.Contains('\n'))
        {
            line.Split('\n').ToList().ForEach((singleLine) => Fatal(singleLine.Trim('\r')));
            return;
        }
        lock (_lock)
            Console.WriteLine($"{DateTime.Now:T} \x1b[31m[Fatal]{line}\x1b[0m");
    }

    public static void Debug(string line)
    {
        if (!Program.Setting.Debug)
        {
            return;
        }

        if (line.Contains('\n'))
        {
            line.Split('\n').ToList().ForEach((singleLine) => Debug(singleLine.Trim('\r')));
            return;
        }
        lock (_lock)
            Console.WriteLine($"{DateTime.Now:T} \x1b[95m[Debug]{line}\x1b[0m");
    }
}
