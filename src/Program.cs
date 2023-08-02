using Newtonsoft.Json;
using iPanelHost.Base;
using iPanelHost.WebSocket;
using iPanelHost.Http;
using iPanelHost.Utils;
using Sharprompt;
using System;
using System.IO;
using System.Linq;

namespace iPanelHost
{
    internal static class Program
    {
        /// <summary>
        /// 版本
        /// </summary>
        public static readonly string VERSION = new Version(2, 2, 0).ToString();

        public static string Logo = @"
  _ ____                  _   _   _           _   
 (_)  _ \ __ _ _ __   ___| | | | | | ___  ___| |_ 
 | | |_) / _` | '_ \ / _ \ | | |_| |/ _ \/ __| __|
 | |  __/ (_| | | | |  __/ | |  _  | (_) \__ \ |_ 
 |_|_|   \__,_|_| |_|\___|_| |_| |_|\___/|___/\__|
 ";

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
            Initialization.InitEnv();
            ReadSetting();
            Logger.Info(Logo);
            WsServer.Start();
            HttpServer.Start();
            Runtime.StartHandleInput();
        }

        private static void ReadSetting()
        {
            if (!File.Exists("setting.json") || Environment.GetCommandLineArgs().Contains("-i") || Environment.GetCommandLineArgs().Contains("--init"))
            {
                Initialization.InitSetting();
            }
            _setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText("setting.json")) ?? throw new SettingsException("转换出现异常空值");
            _setting.Check();
        }
    }
}
