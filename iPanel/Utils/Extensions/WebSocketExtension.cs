using System.Net.WebSockets;
using System.Threading.Tasks;

using EmbedIO.WebSockets;

using Swan.Logging;

namespace iPanel.Utils.Extensions;

public static class WebSocketExtension
{
    public static async Task SendAsync(this IWebSocketContext context, string payload)
    {
        if (context.WebSocket.State != WebSocketState.Open)
            return;

        await context.WebSocket.SendAsync(EncodingsMap.UTF8.GetBytes(payload), true);

        Logger.Debug($"[{context.RemoteEndPoint}] 发送数据\n{payload}");
    }

    public static async Task CloseAsync(this IWebSocketContext context) =>
        await context.WebSocket.CloseAsync();

    public static async Task CloseAsync(
        this IWebSocketContext context,
        CloseStatusCode code,
        string reason
    ) => await context.WebSocket.CloseAsync(code, reason);
}
