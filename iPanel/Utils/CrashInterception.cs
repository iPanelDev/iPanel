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
            AppDomain.CurrentDomain.UnhandledException += (_, e) => PrintException(e.ExceptionObject);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => Console.ReadLine();
            TaskScheduler.UnobservedTaskException += (_, e) => PrintException(e.Exception);
        }

        /// <summary>
        /// 打印错误消息
        /// </summary>
        /// <param name="e">错误消息</param>
        private static void PrintException(object e)
        {
            Logger.Error(e.ToString() ?? string.Empty);
            Directory.CreateDirectory("log/crash");
            File.AppendAllText(
                $"log/crash/{DateTime.Now:yyyy-MM-dd}.txt",
                $"{DateTime.Now:T} | iPanel@{Program.VERSION} | NET@{Environment.Version}{Environment.NewLine}{e.ToString() ?? string.Empty}{Environment.NewLine}"
                );
            Logger.Error($"崩溃日志已保存在 {Path.GetFullPath(Path.Combine("logs", "crash", $"{DateTime.Now:yyyy-MM-dd}.txt"))}");
            Task.Delay(1500).Await();
        }
    }
}
