using EmbedIO.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace iPanelHost.WebSocket
{
    internal class WsModule : WebSocketModule
    {
        public static WsModule? This;

        /// <summary>
        /// 心跳计时器
        /// </summary>
        private static readonly System.Timers.Timer _heartbeatTimer = new(5000);

        public WsModule(string urlPath) : base(urlPath, true)
        {
            This = this;
            Encoding = new UTF8Encoding(false);
        }

        /// <summary>
        /// 活跃数量
        /// </summary>
        public int ActiveContextsCount => ActiveContexts.Count;

        protected override void OnStart(CancellationToken cancellationToken)
        {
            _heartbeatTimer.Elapsed += Handler.Heartbeat;
            _heartbeatTimer.Start();
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
            => Task.Run(() => Handler.OnReceive(context, Encoding.GetString(buffer)));

        protected override Task OnClientConnectedAsync(IWebSocketContext context)
            => Task.Run(() => Handler.OnOpen(context));

        protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
            => Task.Run(() => Handler.OnClose(context));

        public void Send(IWebSocketContext context, string payload)
            => SendAsync(context, payload).Wait();

        public void Close(IWebSocketContext context)
            => CloseAsync(context).Wait();
    }
}