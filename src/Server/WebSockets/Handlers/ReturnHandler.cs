using System;
using iPanelHost.Base.Client;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Utils;
using Newtonsoft.Json;

namespace iPanelHost.Server.WebSocket.Handlers;

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
                InstanceInfo? info = packet.Data?.ToObject<InstanceInfo?>();
                if (info is null)
                {
                    instance.Send(new InvalidDataPacket("“data”字段为null"));
                    break;
                }
                instance.Info = info;

                break;

            case "dir_info":
                HandleAsRequest(instance, packet);
                break;

            default:
                instance.Send(new InvalidParamPacket($"所请求的“{packet.SubType}”类型不存在或无法调用"));
                break;
        }
    }

    private static void HandleAsRequest(Instance instance, ReceivedPacket packet)
    {
        if (string.IsNullOrEmpty(packet.RequestId))
        {
            return;
        }
        try
        {
            RequestsFactory.MarkAsReceived(packet.RequestId, instance.InstanceID, packet.Data);
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }
}
