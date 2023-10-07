using EmbedIO.WebSockets;
using iPanelHost.Utils;
using Spectre.Console;
using Spectre.Console.Json;

namespace iPanelHost.Server;

public static class WebSocketExtension
{
    /// <summary>
    /// 发送文本
    /// </summary>
    /// <param name="payload">载荷</param>
    public static void Send(this IWebSocketContext context, string payload)
    {
        context.WebSocket.SendAsync(ApiHelper.UTF8.GetBytes(payload), true);

        if (Program.Setting.Debug)
        {
            Logger.Debug($"<{context.RemoteEndPoint}> 发送数据");
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
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    public static void Close(this IWebSocketContext context) =>
        context.WebSocket.CloseAsync().GetAwaiter().GetResult();

    /// <summary>
    /// 关闭连接
    /// </summary>
    public static void Close(this IWebSocketContext context, string reason) =>
        context.WebSocket.CloseAsync(CloseStatusCode.Normal, reason).GetAwaiter().GetResult();
}
