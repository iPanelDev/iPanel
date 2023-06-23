using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using iPanel.Core.Client;
using iPanel.Core.Packets;
using iPanel.Core.Packets.DataBody;
using iPanel.Utils;

namespace iPanel.Core.Service
{
    internal static class Events
    {
        public static void Handle(Instance instance, ReceivedPacket packet)
        {
            Logger.Info($"<{instance.Address}> 收到事件：{packet.SubType}，数据：{packet.Data?.ToString(Formatting.None) ?? "空"}");
            switch (packet.SubType)
            {
                case "server_input":
                case "server_output":
                    if (packet.Data is null || packet.Data.Type != JTokenType.Array)
                    {
                        instance.Send(new SentPacket("event", "invalid_param", new Reason("“data”字段类型错误")).ToString());
                        break;
                    }
                    Send(instance, packet.SubType, packet.Data.Type == JTokenType.String ? new[] { packet.Data } : packet.Data);
                    break;
                case "server_start":
                    Send(instance, packet.SubType, null);
                    break;
                case "server_stop":
                    if (packet.SubType == "server_stop" && (packet.Data is null || packet.Data?.Type != JTokenType.Integer))
                    {
                        instance.Send(new SentPacket("event", "invalid_param", new Reason("“data”字段类型错误")).ToString());
                        break;
                    }
                    Send(instance, packet.SubType, new[] { packet.Data });
                    break;
                default:
                    instance.Send(new SentPacket("event", "invalid_param", new Reason($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString()).Await();
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
            string address = instance.Address ?? throw new System.ArgumentNullException();
            lock (Connections.Consoles)
            {
                foreach (Console console in Connections.Consoles.Values)
                {
                    if (console.Address == address || console.Address == "*")
                    {
                        console.Send(new SentPacket("event", subType, data, Sender.From(instance)).ToString()).Await();
                    }
                }
            }
        }
    }
}