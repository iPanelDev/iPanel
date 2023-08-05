using EmbedIO.WebSockets;
using Newtonsoft.Json;
using iPanelHost.Base;
using iPanelHost.WebSocket.Client;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.WebSocket.Service;
using iPanelHost.Utils;
using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace iPanelHost.WebSocket
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

        public static readonly Dictionary<string, string> Guids = new();

        /// <summary>
        /// 发送心跳
        /// </summary>
        public static void Heartbeat(object? sender, ElapsedEventArgs e)
        {
            lock (Instances)
            {
                Instances.Values.ToList().ForEach((instance) => instance?.Send(new SentPacket("action", "heartbeat").ToString()));
            }
        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="context">上下文</param>
        public static void OnOpen(IWebSocketContext context)
        {
            if (context is null)
            {
                return;
            }
            Verification.Request(context);
            UpdateTitle();
        }

        /// <summary>
        /// 关闭处理
        /// </summary>
        /// <param name="context">上下文</param>
        public static void OnClose(IWebSocketContext context)
        {
            if (context is null)
            {
                return;
            }
            string clientUrl = context.RemoteEndPoint.ToString();
            if (!Guids.TryGetValue(clientUrl, out string? guid))
            {
                return;
            }
            Logger.Info($"<{clientUrl}> 断开了连接");
            Instances.Remove(guid);
            Consoles.Remove(guid);
            Guids.Remove(clientUrl);
            UpdateTitle();
        }

        /// <summary>
        /// 接收处理
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="message">接收信息</param>
        public static void OnReceive(IWebSocketContext context, string message)
        {
            UpdateTitle();

            if (context is null)
            {
                return;
            }
            string clientUrl = context.RemoteEndPoint.ToString();
            if (!Guids.TryGetValue(clientUrl, out string? guid))
            {
                return;
            }
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
                context.Send(new SentPacket("event", (isConsole || isInstance) ? "invalid_packet" : "disconnection", new Result($"发送的数据包存在问题：{e.Message}")).ToString());
                if (!isConsole && !isInstance)
                {
                    context.Close();
                }
                return;
            }

            if (!isConsole && !isInstance) // 对未记录的的上下文进行校验
            {
                Verification.PreCheck(context, packet);
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
        /// <param name="console">控制台上下文</param>
        /// <param name="packet">数据包</param>
        private static void Handle(Console console, ReceivedPacket packet)
        {
            switch (packet.Type)
            {
                case "action":
                    ActionsHandler.Handle(console, packet);
                    break;
                default:
                    console.Send(new SentPacket("event", "invalid_param", new Result($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
                    break;
            }
        }

        /// <summary>
        /// 处理数据包（实例）
        /// </summary>
        /// <param name="console">实例上下文</param>
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
                    instance.Send(new SentPacket("event", "invalid_param", new Result($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
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
                Sys.Console.Title = $"iPanel Host {Constant.VERSION} [{Consoles.Count + Instances.Count} 连接]";
            }
        }
    }
}
