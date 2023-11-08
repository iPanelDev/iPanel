using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO.WebSockets;
using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Utils;

namespace iPanelHost.Server.WebSocket;

public class BroadcastWsModule : WebSocketModule
{
    public static readonly Dictionary<string, ConsoleListener> Listeners = new();

    public BroadcastWsModule()
        : base("/broadcast", true)
    {
        Encoding = new UTF8Encoding(false);
    }

    protected override void OnStart(CancellationToken cancellationToken)
    {
        foreach (var keyValuePair in Listeners)
        {
            keyValuePair.Value.Close();
        }

        Listeners.Clear();
    }

    protected override Task OnClientConnectedAsync(IWebSocketContext context)
    {
        string? uuid = context.Session[ApiHelper.UUIDKEY]?.ToString();
        if (
            context.Session[ApiHelper.USERKEY] is not User user
            || user is null
            || string.IsNullOrEmpty(uuid)
        )
        {
            Logger.Warn($"<{context.RemoteEndPoint}> 尝试连接（401）");
            context.Close("Unauthorized");
            return Task.CompletedTask;
        }

        Listeners.Add(uuid, new(user, context));
        Logger.Info($"<{context.RemoteEndPoint}> 连接到广播WS服务器");

        return Task.CompletedTask;
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
    {
        if (!Listeners.Remove(context.Session[ApiHelper.UUIDKEY]?.ToString() ?? string.Empty))
        {
            string? key = Listeners
                .FirstOrDefault((kv) => kv.Value.Context.RemoteEndPoint == context.RemoteEndPoint)
                .Key;

            if (!string.IsNullOrEmpty(key))
            {
                Listeners.Remove(key);
            }
        }
        Logger.Info($"<{context.RemoteEndPoint}> 从广播WS服务器断开连接");
        return Task.CompletedTask;
    }

    protected override Task OnMessageReceivedAsync(
        IWebSocketContext context,
        byte[] buffer,
        IWebSocketReceiveResult result
    ) => Task.CompletedTask;
}
