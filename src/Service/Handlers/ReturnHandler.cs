using iPanelHost.Base;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Utils;
using iPanelHost.Base.Client;
using iPanelHost.Base.Client.Info;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sys = System;
using System.Linq;

namespace iPanelHost.Service.Handlers;

public static class ReturnHandler
{
    public static void Handle(Instance instance, ReceivedPacket packet)
    {
        Logger.Info(
            $"<{instance.Address}> 收到返回数据：{packet.SubType}，数据：{packet.Data?.ToString(Formatting.None) ?? "空"}"
        );
        switch (packet.SubType)
        {
            case "heartbeat":
                FullInfo? info = packet.Data?.ToObject<FullInfo?>();
                if (info is null)
                {
                    instance.Send(new InvalidDataPacket("“data”字段为null"));
                    break;
                }
                instance.FullInfo = info;
                lock (MainHandler.Consoles)
                    MainHandler.Consoles
                        .Where(
                            (kv) =>
                                kv.Value.InstanceIdSubscribed == "*"
                                || kv.Value.InstanceIdSubscribed == instance.InstanceID
                        )
                        .Select((kv) => kv.Value)
                        .ToList()
                        .ForEach(
                            (console) =>
                                console.Send(
                                    new SentPacket(
                                        "return",
                                        "instance_info",
                                        null,
                                        new JObject
                                        {
                                            { "instance_id", instance.InstanceID },
                                            { "info", JObject.FromObject(instance.FullInfo) }
                                        }
                                    )
                                )
                        );
                break;

            case "dir_info":
                DirInfo dirInfo =
                    packet.Data?.ToObject<DirInfo>() ?? throw new Sys.NullReferenceException();
                HandleReturnPacket(instance, packet, dirInfo);
                break;

            default:
                instance.Send(new InvalidParamPacket($"所请求的“{packet.SubType}”类型不存在或无法调用"));
                break;
        }
    }

    /// <summary>
    /// 处理返回数据包
    /// </summary>
    /// <param name="instance">实例</param>
    /// <param name="packet">数据包</param>
    public static void HandleReturnPacket(
        Instance instance,
        ReceivedPacket packet,
        object? data = null
    )
    {
        if (
            string.IsNullOrEmpty(packet.RequestId)
            || !RequestsHandler.ReuqestsDict.TryGetValue(packet.RequestId, out Request? request)
            || request.InstanceID != instance.InstanceID
        )
        {
            return;
        }

        if (MainHandler.Consoles.TryGetValue(request.CallerUUID, out Console? console))
        {
            console.Send(
                new SentPacket
                {
                    Type = packet.Type,
                    SubType = packet.SubType,
                    Echo = request.Echo,
                    Data = data,
                    Sender = Sender.From(instance)
                }
            );
            RequestsHandler.ReuqestsDict.Remove(packet.RequestId);
        }
    }
}
