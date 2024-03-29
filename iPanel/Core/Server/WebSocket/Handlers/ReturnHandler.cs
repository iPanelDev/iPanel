using System;
using System.Threading.Tasks;

using iPanel.Core.Models.Client;
using iPanel.Core.Models.Client.Infos;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Packets.Event;
using iPanel.Utils.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iPanel.Core.Server.WebSocket.Handlers;

[Handler("return.*")]
public class ReturnHandler : HandlerBase
{
    public ReturnHandler(IHost host)
        : base(host) { }

    private ILogger<ReturnHandler> Logger => Services.GetRequiredService<ILogger<ReturnHandler>>();

    public override async Task Handle(Instance instance, WsReceivedPacket packet)
    {
        switch (packet.SubType)
        {
            case "heartbeat":
                InstanceInfo? info = packet.Data?.ToObject<InstanceInfo>();
                if (info is null)
                {
                    await instance.SendAsync(new InvalidDataPacket("“data”字段为null"));
                    break;
                }
                instance.Info = info;

                break;

            case "dir_info":
                HandleAsRequest(instance, packet);
                break;

            default:
                await instance.SendAsync(
                    new InvalidParamPacket($"所请求的“{packet.SubType}”类型不存在或无法调用")
                );
                break;
        }
    }

    private void HandleAsRequest(Instance instance, WsReceivedPacket packet)
    {
        if (string.IsNullOrEmpty(packet.RequestId))
            return;

        try
        {
            RequestsFactory.MarkAsReceived(packet.RequestId, instance.InstanceId, packet.Data);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
        }
    }
}
