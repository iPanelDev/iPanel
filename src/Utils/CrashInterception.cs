using System;
using System.IO;
using System.Threading.Tasks;

namespace iPanelHost.Utils
{
    public static class CrashInterception
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += PrintException;
            TaskScheduler.UnobservedTaskException += (_, e) => Logger.Fatal(e.Exception.ToString() ?? string.Empty);
        }

        /// <summary>
        /// 打印错误消息
        /// </summary>
        private static void PrintException(object sender, UnhandledExceptionEventArgs e)
        {
            string exceptionMsg = (e.ExceptionObject as Exception)?.ToString() ?? string.Empty;
            Logger.Fatal(exceptionMsg);
            Directory.CreateDirectory("logs/crash");
            File.AppendAllText(
                $"log/crash/{DateTime.Now:yyyy-MM-dd}.txt",
                $"{DateTime.Now:T} | iPanel@{Constant.VERSION} | NET@{Environment.Version}{Environment.NewLine}{exceptionMsg}{Environment.NewLine}"
                );
            Logger.Fatal($"崩溃日志已保存在 {(Path.GetFullPath(Path.Combine("logs", "crash", $"{DateTime.Now:yyyy-MM-dd}.txt")))}");

            if (e.IsTerminating && !Console.IsInputRedirected)
            {
                Console.ReadKey(true);
            }
        }
    }
}
