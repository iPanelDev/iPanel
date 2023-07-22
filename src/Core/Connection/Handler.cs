using Fleck;
using Newtonsoft.Json;
using iPanel.Base;
using iPanel.Core.Client;
using iPanel.Core.Packets;
using iPanel.Core.Packets.DataBody;
using iPanel.Core.Service;
using iPanel.Utils;
using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace iPanel.Core.Connection
{
    internal static class Handler
    {
        /// <summary>
        /// 控制台字典
        /// </summary>
        public static readonly Dictionary<string, Console> Consoles = new();

        /// <summary>
        /// 实例字典
        /// </summary>
        public static readonly Dictionary<string, Instance> Instances = new();

        /// <summary>
        /// 发送心跳
        /// </summary>
        public static void Heartbeat(object? sender, ElapsedEventArgs e)
        {
            lock (Instances)
            {
                Instances.ToList().Where((kv) => kv.Value.WebSocketConnection?.IsAvailable != true).ToList().ForEach((kv) => Instances.Remove(kv.Key));
                Instances.Values.ToList().ForEach((instance) => instance?.Send(new SentPacket("action", "heartbeat").ToString()).Await());
            }

            lock (Consoles)
            {
                Consoles.ToList().Where((kv) => kv.Value.WebSocketConnection?.IsAvailable != true).ToList().ForEach((kv) => Consoles.Remove(kv.Key));
            }
        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="connection">客户端</param>
        public static void OnOpen(IWebSocketConnection connection)
        {
            if (connection is null)
            {
                return;
            }
            Verification.Request(connection);
            UpdateTitle();
        }

        /// <summary>
        /// 关闭处理
        /// </summary>
        /// <param name="connection">客户端</param>
        public static void OnClose(IWebSocketConnection connection)
        {
            if (connection is null)
            {
                return;
            }
            string clientUrl = connection.GetFullAddr();
            string guid = connection.ConnectionInfo.Id.ToString("N");
            Logger.Info($"<{clientUrl}> 断开了连接");
            Instances.Remove(guid);
            Consoles.Remove(guid);
            UpdateTitle();
        }

        /// <summary>
        /// 接收处理
        /// </summary>
        /// <param name="connection">客户端</param>
        /// <param name="message">接收信息</param>
        public static void OnReceive(IWebSocketConnection connection, string message)
        {
            UpdateTitle();

            if (connection is null)
            {
                return;
            }
            string clientUrl = connection.GetFullAddr();
            string guid = connection.ConnectionInfo.Id.ToString("N");
            bool isConsole = Consoles.TryGetValue(guid, out Console? console) && console is not null,
                 isInstance = Instances.TryGetValue(guid, out Instance? instance) && instance is not null;
            ReceivedPacket? packet = null;
            try
            {
                packet = JsonConvert.DeserializeObject<ReceivedPacket>(message) ?? throw new PacketException("空数据包");
            }
            catch (Sys.Exception e)
            {
                Logger.Warn($"<{clientUrl}>处理数据包异常\n{e}");
                connection.Send(new SentPacket("event", (isConsole || isInstance) ? "invalid_packet" : "disconnection", new Reason($"发送的数据包存在问题：{e.Message}")).ToString()).Await();
                if (!isConsole && !isInstance)
                {
                    connection.Close();
                }
                return;
            }

            if (!isConsole && !isInstance) // 对未记录的的客户端进行校验
            {
                Verification.PreCheck(connection, packet);
            }
            else if (isConsole)
            {
                Handle(console!, packet);
            }
            else
            {
                Handle(instance!, packet);
            }
        }

        /// <summary>
        /// 处理数据包（控制台）
        /// </summary>
        /// <param name="console">控制台客户端</param>
        /// <param name="packet">数据包</param>
        private static void Handle(Console console, ReceivedPacket packet)
        {
            switch (packet.Type)
            {
                case "action":
                    ActionsHandler.Handle(console, packet);
                    break;
                default:
                    console.Send(new SentPacket("event", "invalid_param", new Reason($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
                    break;
            }
        }

        /// <summary>
        /// 处理数据包（实例）
        /// </summary>
        /// <param name="console">实例客户端</param>
        /// <param name="packet">数据包</param>
        private static void Handle(Instance instance, ReceivedPacket packet)
        {
            instance.LastTime = Sys.DateTime.Now;
            switch (packet.Type)
            {
                case "action":
                    ActionsHandler.Handle(instance, packet);
                    break;
                case "event":
                    EventsHandler.Handle(instance, packet);
                    break;
                default:
                    instance.Send(new SentPacket("event", "invalid_param", new Reason($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
                    break;
            }
        }

        /// <summary>
        /// 更新窗口标题
        /// </summary>
        private static void UpdateTitle()
        {
            if (Sys.Environment.OSVersion.Platform == Sys.PlatformID.Win32NT)
            {
                Sys.Console.Title = $"iPanel Host {Program.VERSION} 连接数:{Consoles.Count + Instances.Count}";
            }
        }
    }
}
