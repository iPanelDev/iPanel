using Newtonsoft.Json;
using iPanelHost.Base;
using iPanelHost.Interaction;
using iPanelHost.Server;
using iPanelHost.Service;
using iPanelHost.Utils;
using System;
using System.IO;
using System.Linq;

namespace iPanelHost;

public static class Program
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

        Win32.SetConsoleMode();
        if (!_hasShownLogo)
        {
            if (Setting.DisplayShorterLogoWhenStart)
            {
                Logger.Info(Constant.Logo);
            }
            else
            {
                Logger.Info(Constant.LogoIco.Replace("\\x1b", "\x1b"));
            }
        }

        UserManager.Read();
        HttpServer.Start();
        Runtime.StartHandleInput();
    }

    public static void ReadSetting(Setting? setting = null)
    {
        if (
            !File.Exists("setting.json") && setting is not null
            || Environment.GetCommandLineArgs().Contains("-i")
            || Environment.GetCommandLineArgs().Contains("--init")
        )
        {
            Console.WriteLine(Constant.LogoIco.Replace("\\x1b", "\x1b"));
            _hasShownLogo = true;
            _setting = Input.CreateSetting();
            return;
        }
        _setting =
            setting
            ?? JsonConvert.DeserializeObject<Setting>(File.ReadAllText("setting.json"))
            ?? throw new SettingsException("转换出现异常空值");
        _setting.Check();
    }
}
