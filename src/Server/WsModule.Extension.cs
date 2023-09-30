using EmbedIO.WebSockets;
using iPanelHost.Utils;
using Spectre.Console;
using Spectre.Console.Json;

namespace iPanelHost.Server;

public static class WsModuleExtension
{
    public static void Send(this IWebSocketContext context, string payload)
    {
        WsModule.Instance?.Send(context, payload);

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

    public static void Close(this IWebSocketContext context) => WsModule.Instance?.Close(context);
}
