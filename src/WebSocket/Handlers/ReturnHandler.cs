using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Utils;
using iPanelHost.WebSocket.Client;
using iPanelHost.WebSocket.Client.Info;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using iPanelHost.Base.Packets.DataBody;

namespace iPanelHost.WebSocket.Handlers
{
    public static class ReturnHandler
    {
        public static void Handle(Instance instance, ReceivedPacket packet)
        {
            Logger.Info($"<{instance.Address}> 收到返回数据：{packet.SubType}，数据：{packet.Data?.ToString(Formatting.None) ?? "空"}");
            switch (packet.SubType)
            {
                case "heartbeat":
                    FullInfo? info = packet.Data?.ToObject<FullInfo?>();
                    if (info is null)
                    {
                        instance.Send(new InvalidDataPacket("“data”字段为null"));
                        break;
                    }
                    instance.FullInfo = info.Value;
                    lock (MainHandler.Consoles)
                        MainHandler.Consoles
                            .Where((kv) => kv.Value.SubscribingTarget == "*" || kv.Value.SubscribingTarget == instance.InstanceID)
                            .Select((kv) => kv.Value)
                            .ToList()
                            .ForEach((console) => console.Send(
                                new SentPacket(
                                    "return",
                                    "instance_info",
                                    null,
                                    new JObject
                                    {
                                        { "instance_id",    instance.InstanceID },
                                        { "info",           JObject.FromObject(instance.FullInfo) }
                                    }
                                )));
                    break;

                case "dir_info":
                    DirInfo? dirInfo = packet.Data?["dir_info"]?.ToObject<DirInfo>();
                    string? uuid = packet.Data?["uuid"]?.ToString();
                    if (string.IsNullOrEmpty(uuid))
                    {
                        instance.Send(new InvalidDataPacket("“data”字段为null或缺少指定值"));
                        break;
                    }

                    Send(instance, "dir_info", uuid, dirInfo, packet.Echo);
                    break;

                default:
                    instance.Send(new InvalidParamPacket($"所请求的“{packet.SubType}”类型不存在或无法调用"));
                    break;
            }
        }

        /// <summary>
        /// 发送事件数据包
        /// </summary>
        /// <param name="instance">实例客户端</param>
        /// <param name="subType">子类型</param>
        /// <param name="data">数据主体</param>
        private static void Send(Instance instance, string subType, string? uuid, object? data, JToken? echo)
        {
            if (string.IsNullOrEmpty(uuid) || !MainHandler.Consoles.TryGetValue(uuid!, out Console? console))
            {
                instance.Send(new OperationResultPacket(echo, ResultTypes.InvalidConsole));
                return;
            }
            console.Send(new SentPacket("return", subType, echo, data) { Sender = Sender.From(instance) });
        }
    }
}