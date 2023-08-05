using Newtonsoft.Json;
using iPanelHost.Base;
using iPanelHost.Http;
using iPanelHost.Permissons;
using iPanelHost.Utils;
using System;
using System.IO;
using System.Linq;

namespace iPanelHost
{
    internal static class Program
    {
        private static bool _hasShownLogo;

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
                Console.WriteLine($"iPanel Host - {Constant.VERSION}");
                return;
            }
            EntryPoint();
        }

        private static void EntryPoint()
        {
            Initialization.InitEnv();

            ReadSetting();
            UserManager.Read();

            Win32.SetConsoleMode();
            if (!_hasShownLogo)
            {
                Logger.Info(Constant.Logo);
            }

            HttpServer.Start();
            Runtime.StartHandleInput();
        }

        private static void ReadSetting()
        {
            if (!File.Exists("setting.json") || Environment.GetCommandLineArgs().Contains("-i") || Environment.GetCommandLineArgs().Contains("--init"))
            {
                Console.WriteLine(Constant.Logo);
                _hasShownLogo = true;
                _setting = Initialization.InitSetting();
                return;
            }
            _setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText("setting.json")) ?? throw new SettingsException("转换出现异常空值");
            _setting.Check();
        }
    }
}
