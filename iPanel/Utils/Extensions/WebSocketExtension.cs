using EmbedIO.WebSockets;
using iPanel.Core.Server.Api;
using Spectre.Console;
using Spectre.Console.Json;
using Swan.Logging;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace iPanel.Utils.Extensions;

public static class WebSocketExtension
{
    public static async Task SendAsync(this IWebSocketContext context, string payload)
    {
        if (context.WebSocket.State != WebSocketState.Open)
            return;

        await context.WebSocket.SendAsync(ApiHelper.UTF8.GetBytes(payload), true);

        Logger.Debug($"[{context.RemoteEndPoint}] 发送数据");
        AnsiConsole.Write(
            new JsonText(payload)
                .BracesColor(Color.White)
                .BracketColor(Color.White)
                .CommaColor(Color.White)
                .MemberColor(Color.SkyBlue1)
                .StringColor(Color.LightSalmon3_1)
                .NumberColor(Color.DarkSeaGreen2)
                .BooleanColor(Color.DodgerBlue3)
                .NullColor(Color.DodgerBlue3)
        );
        AnsiConsole.WriteLine();
    }

    public static async Task CloseAsync(this IWebSocketContext context) =>
        await context.WebSocket.CloseAsync();

    public static async Task CloseAsync(
        this IWebSocketContext context,
        CloseStatusCode code,
        string reason
    ) => await context.WebSocket.CloseAsync(code, reason);
}
