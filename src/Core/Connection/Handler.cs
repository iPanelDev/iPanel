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
        /// 客户端字典
        /// </summary>
        public static readonly Dictionary<string, string> Clients = new();

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
                Instances.Values.ToList().ForEach((instance) => instance?.Send(new SentPacket("action", "heartbeat").ToString()).Await());
            }
        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="client">客户端</param>
        public static void OnOpen(IWebSocketConnection client)
        {
            if (client is null)
            {
                return;
            }
            Verification.Request(client);
            UpdateTitle();
        }

        /// <summary>
        /// 关闭处理
        /// </summary>
        /// <param name="client">客户端</param>
        public static void OnClose(IWebSocketConnection client)
        {
            if (client is null)
            {
                return;
            }
            string clientUrl = client.GetFullAddr();
            Logger.Info($"<{clientUrl}> 断开了连接");
            Clients.Remove(clientUrl);
            Instances.Remove(clientUrl);
            Consoles.Remove(clientUrl);
            UpdateTitle();
        }

        /// <summary>
        /// 接收处理
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="message">接收信息</param>
        public static void OnReceive(IWebSocketConnection client, string message)
        {
            UpdateTitle();

            if (client is null)
            {
                return;
            }
            string clientUrl = client.GetFullAddr();
            bool isConsole = Consoles.TryGetValue(clientUrl, out Console? console) && console is not null,
                 isInstance = Instances.TryGetValue(clientUrl, out Instance? instance) && instance is not null;
            ReceivedPacket? packet = null;
            try
            {
                packet = JsonConvert.DeserializeObject<ReceivedPacket>(message) ?? throw new PacketException("空数据包");
            }
            catch (Sys.Exception e)
            {
                Logger.Warn($"<{clientUrl}>处理数据包异常\n{e}");
                client.Send(new SentPacket("event", (isConsole || isInstance) ? "invalid_packet" : "disconnection", new Reason($"发送的数据包存在问题：{e.Message}")).ToString()).Await();
                if (!isConsole && !isInstance)
                {
                    client.Close();
                }
                return;
            }

            if (!isConsole && !isInstance) // 对未记录的的客户端进行校验
            {
                Verification.PreCheck(client, packet);
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
                case "event":
                    EventsHandler.Handle(instance, packet);
                    break;
                default:
                    instance.Send(new SentPacket("event", "invalid_param", new Reason($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
                    break;
            }
        }

        private static void UpdateTitle()
        {
            if (Sys.Environment.OSVersion.Platform == Sys.PlatformID.Win32NT)
            {
                Sys.Console.Title = $"iPanel - 连接数：{Clients.Count}";
            }
        }
    }
}
