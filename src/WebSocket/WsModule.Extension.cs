using EmbedIO.WebSockets;

namespace iPanelHost.WebSocket
{
    internal static class WsModuleExtension
    {
        public static void Send(this IWebSocketContext context, string payload) => WsModule.This?.Send(context, payload);
        public static void Close(this IWebSocketContext context) => WsModule.This?.Close(context);
    }
}
