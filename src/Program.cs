using Newtonsoft.Json;
using iPanel.Base;
using iPanel.Core.Connection;
using iPanel.HttpServer;
using iPanel.Utils;
using Sharprompt;
using Swan.Logging;
using System;
using System.IO;
using System.Linq;

namespace iPanel
{
    internal static class Program
    {
        /// <summary>
        /// 版本
        /// </summary>
        public static readonly string VERSION = new Version(2, 1, 7, 26).ToString();

        /// <summary>
        /// 设置
        /// </summary>
        public static Setting Setting => _setting!;

        private static Setting? _setting;

        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        /// <param name="args">启动参数</param>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Contains("-v") || args.Contains("--version"))
            {
                Console.WriteLine($"iPanel Host@{VERSION}");
                return;
            }
            EntryPoint();
        }

        private static void EntryPoint()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Prompt.ThrowExceptionOnCancel = false;
            CrashInterception.Init();
            Runtime.SetConsole();
            Logger.UnregisterLogger<ConsoleLogger>();
            Logger.RegisterLogger<iPanelLogger>();
            ReadSetting();
            Logger.Info($"iPanel Host {VERSION} 已启动");
            WebSocket.Start();
            Server.Start();
            Runtime.StartHandleInput();
        }

        private static void ReadSetting()
        {
            if (!File.Exists("setting.json"))
            {
                File.WriteAllText("setting.json", JsonConvert.SerializeObject(new Setting(), Formatting.Indented));
                Console.WriteLine($"{DateTime.Now:T} [Warn] 配置文件已生成，请修改后重新启动");
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey(true);
                }
                Environment.Exit(1);
            }
            _setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText("setting.json")) ?? throw new SettingsException("转换出现异常空值");
            _setting.Check();
        }
    }
}
