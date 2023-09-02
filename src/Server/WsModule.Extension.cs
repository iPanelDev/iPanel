using EmbedIO.WebSockets;

namespace iPanelHost.Server;

public static class WsModuleExtension
{
    public static void Send(this IWebSocketContext context, string payload) =>
        WsModule.Instance?.Send(context, payload);

    public static void Close(this IWebSocketContext context) => WsModule.Instance?.Close(context);
}
