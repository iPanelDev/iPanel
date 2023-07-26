using System.Collections.Generic;
using iPanel.Core.Client;
using iPanel.Core.Connection;
using iPanel.Core.Packets;
using iPanel.Core.Packets.DataBody;
using iPanel.Utils;
using Sharprompt;
using Sys = System;
using System.Linq;
using Swan.Logging;

namespace iPanel.InputProcess
{
    internal static class Processor
    {
        private const string _help =
@"
连接
  i/info        查看当前连接信息
  d/disconnect  强制断开实例
  cn/changename 修改实例名称
  
其他
  ?/h/help      显示此页面
  v/version     查看当前版本
  cls/clear     清屏
  exit/^C       退出
  ";

        public static void Process(string line)
        {
#if NET
            string[] args = line.Split('\x20', Sys.StringSplitOptions.RemoveEmptyEntries | Sys.StringSplitOptions.TrimEntries);
#else
            string[] args = line.Split('\x20');
#endif
            switch (args.FirstOrDefault()?.ToLowerInvariant())
            {
                case "i":
                case "info":
                    Logger.Info($"当前有{Handler.Consoles.Count}个控制台和{Handler.Instances.Count}个面板在线");
                    lock (Handler.Consoles)
                    {
                        Handler.Consoles.Keys.ToList().ForEach((key) => Logger.Info($"控制台\t{Handler.Consoles[key].Address,-18}\t{Handler.Consoles[key].CustomName}"));
                    }
                    lock (Handler.Instances)
                    {
                        Handler.Instances.Keys.ToList().ForEach((key) => Logger.Info($"面板\t{Handler.Instances[key].Address,-18}\t{Handler.Instances[key].CustomName}"));
                    }
                    break;

                case "d":
                case "disconnect":
                    Disconnect();
                    break;

                case "cn":
                case "changename":
                    ChangeCustomName();
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
                    Logger.Info($"iPanel Host@{Program.VERSION}");
                    break;

                case "?":
                case "？":
                case "h":
                case "help":
                    Logger.Info(_help);
                    break;

                default:
                    Logger.Warn("未知的命令。请输入“help”查看更多信息");
                    break;
            }
        }

        private static void Disconnect()
        {
            if (Handler.Instances.Count == 0)
            {
                Logger.Warn("当前没有实例在线");
                return;
            }
            KeyValuePair<string, Instance> keyValuePair = Prompt.Select<KeyValuePair<string, Instance>>("请选择要断开的实例", Handler.Instances.ToList(), textSelector: (kv) => $"{kv.Value.Address}\t{kv.Value.CustomName ?? "未知名称"}");
            if (keyValuePair.Value?.WebSocketConnection?.IsAvailable == true)
            {
                keyValuePair.Value?.Send(new SentPacket("event", "disconnection", new Reason("被用户手动断开"))).Await();
                keyValuePair.Value?.Close();
                return;
            }
            Logger.Warn("所选实例无效");
        }

        private static void ChangeCustomName()
        {
            if (Handler.Instances.Count == 0)
            {
                Logger.Warn("当前没有实例在线");
                return;
            }
            KeyValuePair<string, Instance> keyValuePair = Prompt.Select<KeyValuePair<string, Instance>>("请选择要修改名称的实例", Handler.Instances.ToList(), textSelector: (kv) => $"{kv.Value.Address}\t{kv.Value.CustomName ?? "未知名称"}");
            if (Handler.Instances.ContainsKey(keyValuePair.Key))
            {
                string? newName = Prompt.Input<string>("请输入新的名称", null, Handler.Instances[keyValuePair.Key].CustomName);
                if (Handler.Instances.ContainsKey(keyValuePair.Key))
                {
                    Handler.Instances[keyValuePair.Key].CustomName = newName;
                    Logger.Info("实例修改成功");
                    return;
                }
            }
            Logger.Warn("所选实例无效");
        }
    }
}