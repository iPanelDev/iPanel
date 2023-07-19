using System;
using System.IO;
using System.Threading.Tasks;

namespace iPanel.Utils
{
    internal static class CrashInterception
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += PrintException;
            TaskScheduler.UnobservedTaskException += (_, e) => Logger.Error(e.Exception.ToString() ?? string.Empty);
        }

        /// <summary>
        /// 打印错误消息
        /// </summary>
        private static void PrintException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject.ToString() ?? string.Empty);
            Directory.CreateDirectory("logs/crash");
            File.AppendAllText(
                $"log/crash/{DateTime.Now:yyyy-MM-dd}.txt",
                $"{DateTime.Now:T} | iPanel@{Program.VERSION} | NET@{Environment.Version}{Environment.NewLine}{e.ExceptionObject.ToString() ?? string.Empty}{Environment.NewLine}"
                );
            Logger.Error($"崩溃日志已保存在 {Path.GetFullPath(Path.Combine("logs", "crash", $"{DateTime.Now:yyyy-MM-dd}.txt"))}");

            if (e.IsTerminating)
            {
                Runtime.Exit(-1);
            }
        }
    }
}
