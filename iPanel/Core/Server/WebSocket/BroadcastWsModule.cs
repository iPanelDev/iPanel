using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using EmbedIO.WebSockets;

using iPanel.Core.Models.Client;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Users;
using iPanel.Utils;
using iPanel.Utils.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iPanel.Core.Server.WebSocket;

public class BroadcastWsModule : WebSocketModule
{
    public readonly Dictionary<string, ConsoleListener> Listeners = new();
    private readonly string _salt = Guid.NewGuid().ToString("N");

    private readonly IHost _host;
    private IServiceProvider Services => _host.Services;
    private ILogger<BroadcastWsModule> Logger =>
        Services.GetRequiredService<ILogger<BroadcastWsModule>>();

    public BroadcastWsModule(IHost host)
        : base("/ws/broadcast", true)
    {
        _host = host;
        Encoding = EncodingsMap.UTF8;
    }

    protected override void OnStart(CancellationToken cancellationToken)
    {
        Logger.LogInformation("广播WS服务器已开启");
    }

    protected override async Task OnClientConnectedAsync(IWebSocketContext context)
    {
        var clientUrl = context.RemoteEndPoint.ToString();
        if (context.Session[SessionKeyConstants.User] is not User user || user is null)
        {
            Logger.LogWarning("[{}] 尝试连接广播WS服务器（401）", clientUrl);
            await context.CloseAsync(CloseStatusCode.Abnormal, "Unauthorized");

            return;
        }

        var connectionId = Encryption.GetMD5($"{_salt}.{context.RemoteEndPoint}");
        context.SendAsync(new WsSentPacket("return", "connection_id", connectionId));
        Listeners[connectionId] = new(user, context, connectionId);

        Logger.LogInformation("[{}] 连接到广播WS服务器", clientUrl);
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
    {
        Listeners.Remove(Encryption.GetMD5($"{_salt}.{context.RemoteEndPoint}"));
        Logger.LogInformation("[{}] 从广播WS服务器断开连接", context.RemoteEndPoint);
        return Task.CompletedTask;
    }

    protected override Task OnMessageReceivedAsync(
        IWebSocketContext context,
        byte[] buffer,
        IWebSocketReceiveResult result
    ) => Task.CompletedTask;
}
