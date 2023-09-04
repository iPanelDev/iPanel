using iPanelHost.Base;
using iPanelHost.Server;
using iPanelHost.Service.Handlers;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Sharprompt;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace iPanelHost.Interaction;

public static class Input
{
    private const string _helpMenu =
        @"
连接
  ls/list       查看当前连接列表
  d/disconnect  强制断开实例
  cn/changename 修改实例名称

权限
  u/user <...>  管理用户
    ls/list       查看用户列表
    a/add         添加用户
    d/delete      删除用户
    e/edit        编辑用户
    p/perm        管理用户权限
    r/reload      重新加载文件
  
其他
  ?/h/help      显示此页面
  r/reload      重新读取设置文件并重启服务器
  v/version     查看当前版本
  cls/clear     清屏
  logo          显示iPanel的Logo
  exit/^C       退出
  ";

    public static void ReadLine(string line)
    {
#if NET
        string[] args = line.Split(
            '\x20',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
#else
        string[] args = line.Split('\x20');
#endif
        switch (args.FirstOrDefault()?.ToLowerInvariant())
        {
            case "ls":
            case "list":
                Logger.Info(
                    $"当前有{MainHandler.Consoles.Count}个控制台和{MainHandler.Instances.Count}个面板在线"
                );
                lock (MainHandler.Consoles)
                {
                    MainHandler.Consoles.Keys
                        .ToList()
                        .ForEach(
                            (key) =>
                                Logger.Info(
                                    $"{"[控制台]", -5}{MainHandler.Consoles[key].Address, -20}"
                                )
                        );
                }
                lock (MainHandler.Instances)
                {
                    MainHandler.Instances.Keys
                        .ToList()
                        .ForEach(
                            (key) =>
                                Logger.Info(
                                    $"{"[实例]", -5}{MainHandler.Instances[key].Address, -20}{MainHandler.Instances[key].CustomName}"
                                )
                        );
                }
                break;

            case "d":
            case "disconnect":
                Funcions.Disconnect();
                break;

            case "cn":
            case "changename":
                Funcions.ChangeCustomName();
                break;

            case "cls":
            case "clear":
                Console.Clear();
                break;

            case "exit":
                Runtime.Exit();
                break;

            case "logo":
                Logger.Info(Constant.LogoIco.Replace("\\x1b", "\x1b"));
                break;

            case "v":
            case "version":
                Logger.Info($"Name={Assembly.GetExecutingAssembly().GetName().Name}");
                Logger.Info($"Version={Assembly.GetExecutingAssembly().GetName().Version}");
                string[] commandlineArgs = Environment.GetCommandLineArgs();
                if (commandlineArgs.Length > 0 && File.Exists(commandlineArgs[0]))
                {
                    Logger.Info($"FileName={Path.GetFileName(commandlineArgs[0])}");
                    Logger.Info($"MD5={General.GetMD5(File.ReadAllBytes(commandlineArgs[0]))}");
                    Logger.Info($"LastWriteTime={File.GetLastWriteTime(commandlineArgs[0]):o}");
                    Logger.Info($"CreationTime={File.GetCreationTime(commandlineArgs[0]):o}");
                }
                break;

            case "?":
            case "？":
            case "h":
            case "help":
                Logger.Info(_helpMenu);
                break;

            case "u":
            case "user":
                Funcions.ManageUsers(args);
                break;

            case "r":
            case "reload":
                try
                {
                    Program.ReadSetting();
                    Logger.Info("设置已更新");
                    HttpServer.Restart();
                }
                catch (Exception e)
                {
                    Logger.Warn(e.Message);
                }
                break;

            default:
                Logger.Warn("未知的命令。请输入“help”查看更多信息");
                break;
        }
    }

    /// <summary>
    /// 确保输出流未被重定向
    /// </summary>
    public static bool EnsureOutputNotRedirected()
    {
        if (Console.IsOutputRedirected)
        {
            Logger.Warn("输出流被重定向，高级输入模式不可用");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 创建设置文件
    /// </summary>
    public static Setting CreateSetting()
    {
        if (EnsureOutputNotRedirected())
        {
            Logger.Warn("请打开设置文件自行修改");
            return new();
        }
        try
        {
            bool toPublic = Prompt.Confirm("将Http服务器开放到公网", false);
            int port = Prompt.Input<int>(
                "Http服务器的端口",
                30000,
                "1~65535",
                new[]
                {
                    (object obj) =>
                        obj is int value && value > 0 && value <= 65535
                            ? ValidationResult.Success
                            : new("端口无效")
                }
            );

            Setting setting =
                new()
                {
                    InstancePassword = Prompt.Password(
                        "实例连接密码",
                        placeholder: "不要与QQ或服务器等密码重复；推荐大小写字母数字结合",
                        validators: new[]
                        {
                            Validators.Required("密码不可为空"),
                            Validators.MinLength(6, "密码长度过短"),
                            Validators.RegularExpression(@"^[^\s]+$", "密码不得含有空格"),
                        }
                    ),
                    WebServer = new()
                    {
                        UrlPrefixes = new[] { $"http://{(toPublic ? "+" : "127.0.0.1")}:{port}" },
                        AllowCrossOrigin = Prompt.Confirm("允许跨源资源共享（CORS）", false)
                    }
                };

            File.WriteAllText(
                "setting.json",
                JsonConvert.SerializeObject(setting, Formatting.Indented)
            );
            Directory.CreateDirectory("logs");
            Directory.CreateDirectory("dist");

            Console.WriteLine(Environment.NewLine);
            Logger.Info("初始化设置成功");

            return setting;
        }
        catch (PromptCanceledException)
        {
            Runtime.ExitQuietly();
            return null!;
        }
    }
}
