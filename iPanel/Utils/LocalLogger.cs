using Spectre.Console;
using Swan.Logging;
using System;
using System.Text;

namespace iPanel.Utils;

public class LocalLogger : ILogger
{
    private static readonly object _lock = new();

    public static LogLevel StaticLogLevel { get; set; } = LogLevel.Debug;
    public LogLevel LogLevel => StaticLogLevel;
    public static Action<LogMessageReceivedEventArgs>? OnMessage { get; set; }

    private static void Info(string[] lines)
    {
        lock (_lock)
            foreach (var line in lines)
                AnsiConsole.MarkupLineInterpolated(
                    $"{DateTime.Now:T} [cadetblue_1]Info[/]  {line}"
                );
    }

    private static void Warn(string[] lines)
    {
        lock (_lock)
            foreach (var line in lines)
                AnsiConsole.MarkupLineInterpolated(
                    $"{DateTime.Now:T} [yellow bold]Warn  {line}[/]"
                );
    }

    private static void Error(string[] lines)
    {
        lock (_lock)
            foreach (var line in lines)
                AnsiConsole.MarkupLineInterpolated($"{DateTime.Now:T} [red bold]Error {line}[/]");
    }

    private static void Fatal(string[] lines)
    {
        lock (_lock)
            foreach (var line in lines)
                AnsiConsole.MarkupLineInterpolated(
                    $"{DateTime.Now:T} [maroon blod]Fatal {line}[/]"
                );
    }

    private static void Debug(string[] lines)
    {
        lock (_lock)
            foreach (var line in lines)
                AnsiConsole.MarkupLineInterpolated(
                    $"{DateTime.Now:T} [mediumpurple4]Debug[/] {line}"
                );
    }

    public void Log(LogMessageReceivedEventArgs logEvent)
    {
        var stringBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(logEvent.Message))
            stringBuilder.AppendLine(logEvent.Message);

        if (logEvent.Exception is not null)
            stringBuilder.Append(logEvent.Exception);

        var lines = stringBuilder.ToString().TrimEnd('\r', '\n').Replace("\r", null).Split('\n');

        switch (logEvent.MessageType)
        {
            case LogLevel.Debug:
                Debug(lines);
                break;

            case LogLevel.Info:
                Info(lines);
                break;

            case LogLevel.Warning:
                Warn(lines);
                break;

            case LogLevel.Error:
                Error(lines);
                break;

            case LogLevel.Fatal:
                Fatal(lines);
                break;
        }
        OnMessage?.Invoke(logEvent);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
