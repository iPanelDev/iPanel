using iPanelHost.WebSocket;
using iPanelHost.Utils;
using Sys = System;
using System.Linq;

namespace iPanelHost.Inputs
{
    internal static class Switcher
    {
        private const string _help =
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
                    Logger.Info($"当前有{Handler.Consoles.Count}个控制台和{Handler.Instances.Count}个面板在线");
                    lock (Handler.Consoles)
                    {
                        Handler.Consoles.Keys.ToList().ForEach((key) => Logger.Info($"{"[控制台]",-5}{Handler.Consoles[key].Address,-20}"));
                    }
                    lock (Handler.Instances)
                    {
                        Handler.Instances.Keys.ToList().ForEach((key) => Logger.Info($"{"[实例]",-5}{Handler.Instances[key].Address,-20}{Handler.Instances[key].CustomName}"));
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

                case "v":
                case "version":
                    Logger.Info($"iPanel Host@{Constant.VERSION}");
                    break;

                case "?":
                case "？":
                case "h":
                case "help":
                    Logger.Info(_help);
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