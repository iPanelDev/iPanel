using System;
using System.IO;
using System.Linq;
using System.Reflection;
using iPanelHost.Base;
using iPanelHost.Server;
using iPanelHost.Server.WebSocket;
using iPanelHost.Utils;
using Sharprompt;
using Spectre.Console;

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
    ei            管理用户允许订阅的实例
    r/reload      重新加载文件

其他
  ?/h/help      显示此页面
  r/reload      重新读取设置文件并重启服务器
  v/version     查看当前版本
  cls/clear     清屏
  logo          显示iPanel的Logo
  exit/^C       退出
  ";

    /// <summary>
    /// 解析输入
    /// </summary>
    /// <param name="line"></param>
    public static void Parse(string line)
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
                Logger.Info($"当前有{MainWsModule.Instances.Count}个实例在线");

                Table table = new();
                table.AddColumns("地址", "自定义名称", "实例信息").RoundedBorder();

                table.Columns[0].Centered();
                table.Columns[1].Centered();
                table.Columns[2].Centered();

                lock (MainWsModule.Instances)
                {
                    MainWsModule
                        .Instances
                        .ToList()
                        .ForEach(
                            (kv) =>
                                table.AddRow(
                                    kv.Value.Address ?? string.Empty,
                                    kv.Value.CustomName ?? string.Empty,
                                    $"{kv.Value.Metadata?.Name ?? "未知名称"}({kv.Value.Metadata?.Version ?? "?"})"
                                )
                        );
                }

                AnsiConsole.Write(table);
                break;

            case "d":
            case "disconnect":
                SubCommnadHandler.Disconnect();
                break;

            case "cn":
            case "changename":
                SubCommnadHandler.ChangeCustomName();
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
                Logger.Info("Repo: https://github.com/iPanelDev/iPanel-Host");

                Table versionTable = new();
                versionTable
                    .RoundedBorder()
                    .AddColumns(
                        new TableColumn("名称") { Alignment = Justify.Center },
                        new(Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty)
                        {
                            Alignment = Justify.Center
                        }
                    )
                    .AddRow(
                        "版本",
                        Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                            ?? string.Empty
                    );

                string[] commandlineArgs = Environment.GetCommandLineArgs();
                if (commandlineArgs.Length > 0 && File.Exists(commandlineArgs[0]))
                {
                    versionTable
                        .AddRow("文件名", Path.GetFileName(commandlineArgs[0]))
                        .AddRow("MD5", General.GetMD5(File.ReadAllBytes(commandlineArgs[0])))
                        .AddRow("创建时间", File.GetCreationTime(commandlineArgs[0]).ToString("o"))
                        .AddRow("修改时间", File.GetLastWriteTime(commandlineArgs[0]).ToString("o"));
                }
                AnsiConsole.Write(versionTable);
                break;

            case "?":
            case "？":
            case "h":
            case "help":
                Logger.Info(_helpMenu);
                break;

            case "u":
            case "user":
                SubCommnadHandler.ManageUsers(args);
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

            case "ban":
                SubCommnadHandler.ManageBanModule(args);
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
            return AdvancedInput.NewSetting;
        }
        catch (PromptCanceledException)
        {
            Runtime.ExitQuietly(-1);
            return null!;
        }
    }
}
