using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Utils;
using iPanelHost.WebSocket.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iPanelHost.WebSocket.Service
{
    internal static class EventsHandler
    {
        public static void Handle(Instance instance, ReceivedPacket packet)
        {
            Logger.Info($"<{instance.Address}> 收到事件：{packet.SubType}，数据：{packet.Data?.ToString(Formatting.None) ?? "空"}");
            switch (packet.SubType)
            {
                case "server_input":
                case "server_output":
                    if (packet.Data?.Type != JTokenType.Array)
                    {
                        instance.Send(new InvalidDataPacket("“data”字段类型错误"));
                        break;
                    }
                    Send(instance, packet.SubType, packet.Data);
                    break;

                case "server_start":
                    Send(instance, packet.SubType, null);
                    break;

                case "server_stop":
                    if (packet.Data?.Type != JTokenType.Integer)
                    {
                        instance.Send(new InvalidParamPacket("“data”字段类型错误"));
                        break;
                    }
                    Send(instance, packet.SubType, packet.Data);
                    break;
                    
                default:
                    instance.Send(new InvalidParamPacket($"所请求的“{packet.Type}”类型不存在或无法调用"));
                    break;
            }
        }

        /// <summary>
        /// 发送事件数据包
        /// </summary>
        /// <param name="instance">实例客户端</param>
        /// <param name="subType">子类型</param>
        /// <param name="data">数据主体</param>
        private static void Send(Instance instance, string subType, object? data)
        {
            string guid = instance.GUID ?? throw new System.NullReferenceException($"{nameof(instance.GUID)}为空");
            lock (Handler.Consoles)
            {
                foreach (Console console in Handler.Consoles.Values)
                {
                    if (console.SubscribingTarget == guid || console.SubscribingTarget == "*")
                    {
                        console.Send(new SentPacket("broadcast", subType, data, Sender.From(instance)).ToString());
                    }
                }
            }
        }
    }
}