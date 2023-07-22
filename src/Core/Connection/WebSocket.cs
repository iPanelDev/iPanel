using Fleck;
using iPanel.Utils;
using Sys = System;
using System.Timers;

namespace iPanel.Core.Connection
{
    internal static class WebSocket
    {
        /// <summary>
        /// WS服务器
        /// </summary>
        private static WebSocketServer? _server;

        /// <summary>
        /// 心跳计时器
        /// </summary>
        private static readonly Timer _heartbeatTimer = new(5000);

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            FleckLog.LogAction = (level, message, e) =>
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        Logger.Debug($"{message} {e}");
                        break;
                    case LogLevel.Info:
                        Logger.Info($"{message} {e}");
                        break;
                    case LogLevel.Warn:
                        Logger.Warn($"{message} {e}");
                        break;
                    case LogLevel.Error:
                        Logger.Error($"{message} {e}");
                        break;
                    default:
                        throw new Sys.NotSupportedException();
                }
            };

            _server = new(Program.Setting.WebSocket.Addr)
            {
                RestartAfterListenError = true
            };
            _server.Start(socket =>
            {
                socket.OnOpen = () => Handler.OnOpen(socket);
                socket.OnClose = () => Handler.OnClose(socket);
                socket.OnMessage = (message) => Handler.OnReceive(socket, message);
                socket.OnError = (e) => Logger.Error(e.ToString());
            });

            _heartbeatTimer.Elapsed += Handler.Heartbeat;
            _heartbeatTimer.Start();
        }
    }
}
