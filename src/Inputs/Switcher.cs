using iPanelHost.WebSocket.Handlers;
using iPanelHost.Utils;
using Sys = System;
using System.Linq;

namespace iPanelHost.Inputs
{
    public static class Switcher
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

        public static void Input(string line)
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

    }
}