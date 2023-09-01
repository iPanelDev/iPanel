using iPanelHost.Base;
using iPanelHost.Service.Handlers;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Sharprompt;
using Sys = System;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

using System.Linq;

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
  v/version     查看当前版本
  cls/clear     清屏
  logo          显示iPanel的Logo
  exit/^C       退出
  ";

    public static void ReadLine(string line)
    {
#if NET
        string[] args = line.Split('\x20', Sys.StringSplitOptions.RemoveEmptyEntries | Sys.StringSplitOptions.TrimEntries);
#else
            string[] args = line.Split('\x20');
#endif
        switch (args.FirstOrDefault()?.ToLowerInvariant())
        {
            case "ls":
            case "list":
                Logger.Info($"当前有{MainHandler.Consoles.Count}个控制台和{MainHandler.Instances.Count}个面板在线");
                lock (MainHandler.Consoles)
                {
                    MainHandler.Consoles.Keys.ToList().ForEach((key) => Logger.Info($"{"[控制台]",-5}{MainHandler.Consoles[key].Address,-20}"));
                }
                lock (MainHandler.Instances)
                {
                    MainHandler.Instances.Keys.ToList().ForEach((key) => Logger.Info($"{"[实例]",-5}{MainHandler.Instances[key].Address,-20}{MainHandler.Instances[key].CustomName}"));
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
                Sys.Console.Clear();
                break;

            case "exit":
                Runtime.Exit();
                break;

            case "logo":
                Logger.Info(Constant.LogoIco.Replace("\\x1b", "\x1b"));
                break;

            case "v":
            case "version":
                Logger.Info(Constant.VERSION);
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

            default:
                Logger.Warn("未知的命令。请输入“help”查看更多信息");
                break;
        }
    }


    /// <summary>
    /// 初始化设置
    /// </summary>
    public static Setting CreateSetting()
    {
        try
        {
            bool toPublic = Prompt.Confirm("将Http服务器开放到公网", false);
            int port = Prompt.Input<int>(
                "Http服务器的端口",
                30000,
                "1~65535",
                new Func<object, ValidationResult?>[] {
                        (obj) => obj is int value && value > 0 && value <= 65535 ? ValidationResult.Success : new("端口无效")
                });

            Setting setting = new()
            {
                InstancePassword = Prompt.Password(
                    "实例连接密码",
                    placeholder: "不要与QQ或服务器等密码重复；推荐大小写字母数字结合",
                    validators: new[] {
                            Validators.Required("密码不可为空"),
                            Validators.MinLength(6, "密码长度过短"),
                            Validators.RegularExpression(@"^[^\s]+$", "密码不得含有空格"),
                    }),
                WebServer =
                    {
                        UrlPrefixes = new[] { $"http://{(toPublic ? "+" : "127.0.0.1")}:{port}" },
                        AllowCrossOrigin = Prompt.Confirm("允许跨源资源共享（CORS）", false)
                    }
            };

            File.WriteAllText("setting.json", JsonConvert.SerializeObject(setting, Formatting.Indented));
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
