using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Packets.Event;
using iPanel.Core.Models.Client;
using iPanel.Utils;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Swan.Logging;

namespace iPanel.Core.Server.WebSocket.Handlers;

[Handler("broadcast.server_input")]
[Handler("broadcast.server_output")]
[Handler("broadcast.server_start")]
[Handler("broadcast.server_stop")]
public class BroadcastHandler : HandlerBase
{
    public BroadcastHandler(App app)
        : base(app) { }

    private void Send(Instance instance, string subType, object? data)
    {
        var instanceID =
            instance.InstanceID
            ?? throw new NullReferenceException($"{nameof(instance.InstanceID)}为空");

        lock (_app.HttpServer.BroadcastWsModule.Listeners)
        {
            foreach (ConsoleListener console in _app.HttpServer.BroadcastWsModule.Listeners.Values)
            {
                if (
                    console.InstanceIdSubscribed == instanceID
                    || console.InstanceIdSubscribed == "*"
                )
                {
                    console.SendAsync(
                        new SentPacket("broadcast", subType, data, Sender.From(instance)).ToString()
                    );
                }
            }
        }
    }

    public override async Task Handle(Instance instance, ReceivedPacket packet)
    {
        Logger.Info($"[{instance.Address}] 收到广播：{packet.SubType}，数据：{packet.Data ?? "空"}");
        switch (packet.SubType)
        {
            case "server_input":
            case "server_output":
                if (packet.Data is JsonArray)
                {
                    await instance.SendAsync(new InvalidDataPacket("“data”字段类型错误"));
                    break;
                }
                Send(instance, packet.SubType, packet.Data);
                break;

            case "server_start":
                Send(instance, packet.SubType, null);
                break;

            case "server_stop":
                if (
                    packet.Data is not JsonValue jsonValue
                    || jsonValue.AsValue().GetValueKind() != JsonValueKind.Number
                )
                {
                    instance.SendAsync(new InvalidParamPacket("“data”字段类型错误"));
                    break;
                }
                Send(instance, packet.SubType, packet.Data);
                break;

            default:
                instance.SendAsync(new InvalidParamPacket($"所请求的“{packet.SubType}”类型不存在或无法调用"));
                break;
        }
    }
}
