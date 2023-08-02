using System;
using Swan.Logging;
using System.Linq;

namespace iPanelHost.Utils
{
    internal class Logger : ILogger
    {
        public LogLevel LogLevel => LogLevel.Info;

        public void Dispose() { }

        public void Log(LogMessageReceivedEventArgs e)
        {
            string line = string.Empty;
            if (Program.Setting.Output.DisplayCallerMemberName)
            {
                line += $"[{e.CallerMemberName}] ";
            }
            line += e.Message;
            Log(e.MessageType, e.Exception is null ? line : $"[{e.CallerMemberName}] {e.Exception.InnerException?.Message ?? e.Exception.Message}\n  at {e.CallerFilePath}");
        }

        private void Log(LogLevel type, string line)
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
            }
        }

        public static void Info(string line)
        {
            if (line.Contains('\n'))
            {
                line.Split('\n').ToList().ForEach((singleLine) => Info(singleLine.Trim('\r')));
                return;
            }
            Console.WriteLine($"{DateTime.Now:T} \x1b[96m[Info]\x1b[0m {line}");
        }

        public static void Warn(string line)
        {
            if (line.Contains('\n'))
            {
                line.Split('\n').ToList().ForEach((singleLine) => Warn(singleLine.Trim('\r')));
                return;
            }
            Console.WriteLine($"{DateTime.Now:T} \x1b[33m[Warn] {line}\x1b[0m");
        }

        public static void Error(string line)
        {
            if (line.Contains('\n'))
            {
                line.Split('\n').ToList().ForEach((singleLine) => Error(singleLine.Trim('\r')));
                return;
            }
            Console.WriteLine($"{DateTime.Now:T} \x1b[91m[Error]{line}\x1b[0m");
        }

        public static void Fatal(string line)
        {
            if (line.Contains('\n'))
            {
                line.Split('\n').ToList().ForEach((singleLine) => Fatal(singleLine.Trim('\r')));
                return;
            }
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
            Console.WriteLine($"{DateTime.Now:T} \x1b[95m[Debug]{line}\x1b[0m");
        }
    }
}
