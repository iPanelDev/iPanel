using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Base.Client;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iPanelHost.Service.Handlers;

public static class BroadcastHandler
{
    public static void Handle(Instance instance, ReceivedPacket packet)
    {
        Logger.Info(
            $"<{instance.Address}> 收到广播：{packet.SubType}，数据：{packet.Data?.ToString(Formatting.None) ?? "空"}"
        );
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
    private static void Send(Instance instance, string subType, object? data)
    {
        string instanceID =
            instance.InstanceID
            ?? throw new System.NullReferenceException($"{nameof(instance.InstanceID)}为空");
        lock (MainHandler.Consoles)
        {
            foreach (Console console in MainHandler.Consoles.Values)
            {
                if (
                    console.InstanceIdSubscribed == instanceID
                    || console.InstanceIdSubscribed == "*"
                )
                {
                    console.Send(
                        new SentPacket("broadcast", subType, data, Sender.From(instance)).ToString()
                    );
                }
            }
        }
    }
}
