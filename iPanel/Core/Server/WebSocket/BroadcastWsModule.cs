using EmbedIO.WebSockets;
using iPanel.Core.Models.Client;
using iPanel.Core.Models.Users;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using iPanel.Utils.Extensions;
using Swan.Logging;

namespace iPanel.Core.Server.WebSocket;

public class BroadcastWsModule : WebSocketModule
{
    public readonly Dictionary<string, ConsoleListener> Listeners = new();
    private readonly App _app;

    public BroadcastWsModule(App app)
        : base("/broadcast", true)
    {
        Encoding = new UTF8Encoding(false);
        _app = app;
    }

    protected override void OnStart(CancellationToken cancellationToken)
    {
        Logger.Info("广播WS服务器已开启");
    }

    protected override async Task OnClientConnectedAsync(IWebSocketContext context)
    {
        var clientUrl = context.RemoteEndPoint.ToString();
        if (context.Session[SessionKeyConstants.User] is not User user || user is null)
        {
            Logger.Warn($"[{clientUrl}] 尝试连接广播WS服务器（401）");
            await context.CloseAsync(CloseStatusCode.Abnormal, "Unauthorized");

            return;
        }

        Listeners[clientUrl] = new(user, context);

        Logger.Info($"[{clientUrl}] 连接到广播WS服务器");
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
    {
        var clientUrl = context.RemoteEndPoint.ToString();
        Listeners.Remove(clientUrl);
        Logger.Info($"[{clientUrl}] 从广播WS服务器断开连接");
        return Task.CompletedTask;
    }

    protected override Task OnMessageReceivedAsync(
        IWebSocketContext context,
        byte[] buffer,
        IWebSocketReceiveResult result
    ) => Task.CompletedTask;
}
