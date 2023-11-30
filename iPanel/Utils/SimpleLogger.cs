using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;
using ISwanLogger = Swan.Logging.ILogger;
using Spectre.Console;
using Swan.Logging;
using System;
using System.Text;

namespace iPanel.Utils;

public class SimpleLogger : IMicrosoftLogger, ISwanLogger
{
    private static readonly object _lock = new();

    public static LogLevel StaticLogLevel { get; set; } = LogLevel.Debug;
    public LogLevel LogLevel => StaticLogLevel;
    public static Action<uint, string[]>? OnMessage { get; set; }

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
        OnMessage?.Invoke((uint)logEvent.MessageType, lines);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void Log<TState>(
        MicrosoftLogLevel logLevel,
        Microsoft.Extensions.Logging.EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        var stringBuilder = new StringBuilder();
        var stringState = state?.ToString() ?? string.Empty;

        if (!string.IsNullOrEmpty(stringState))
            stringBuilder.AppendLine(stringState);

        if (exception is not null)
            stringBuilder.Append(exception);

        var lines = stringBuilder.ToString().TrimEnd('\r', '\n').Replace("\r", null).Split('\n');

        switch (logLevel)
        {
            case MicrosoftLogLevel.Debug:
                Debug(lines);
                break;

            case MicrosoftLogLevel.Information:
                Info(lines);
                break;

            case MicrosoftLogLevel.Warning:
                Warn(lines);
                break;

            case MicrosoftLogLevel.Error:
                Error(lines);
                break;

            case MicrosoftLogLevel.Critical:
                Fatal(lines);
                break;
        }

        var level = logLevel switch
        {
            MicrosoftLogLevel.Trace => LogLevel.Trace,
            MicrosoftLogLevel.Debug => LogLevel.Debug,
            MicrosoftLogLevel.Information => LogLevel.Info,
            MicrosoftLogLevel.Warning => LogLevel.Warning,
            MicrosoftLogLevel.Error => LogLevel.Error,
            MicrosoftLogLevel.Critical => LogLevel.Fatal,
            MicrosoftLogLevel.None => LogLevel.None,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
        OnMessage?.Invoke((uint)level, lines);
    }

    public bool IsEnabled(MicrosoftLogLevel logLevel)
    {
        var level = logLevel switch
        {
            MicrosoftLogLevel.Trace => LogLevel.Trace,
            MicrosoftLogLevel.Debug => LogLevel.Debug,
            MicrosoftLogLevel.Information => LogLevel.Info,
            MicrosoftLogLevel.Warning => LogLevel.Warning,
            MicrosoftLogLevel.Error => LogLevel.Error,
            MicrosoftLogLevel.Critical => LogLevel.Fatal,
            MicrosoftLogLevel.None => LogLevel.None,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };

        return LogLevel >= level;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }
}
