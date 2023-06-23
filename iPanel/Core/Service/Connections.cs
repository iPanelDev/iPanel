using System.Linq;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using iPanel.Core.Client;
using iPanel.Core.Packets;
using iPanel.Core.Packets.DataBody;
using iPanel.Utils;
using Sys = System;
using System.Collections.Generic;
using System.Timers;

namespace iPanel.Core.Service
{
    internal static class Connections
    {
        /// <summary>
        /// WS服务器
        /// </summary>
        private static WebSocketServer? _server;

        /// <summary>
        /// 客户端字典
        /// </summary>
        public static readonly Dictionary<string, string> Clients = new();

        /// <summary>
        /// 控制台字典
        /// </summary>
        public static readonly Dictionary<string, Console> Consoles = new();

        /// <summary>
        /// 实例字典
        /// </summary>
        public static readonly Dictionary<string, Instance> Instances = new();

        /// <summary>
        /// 心跳计时器
        /// </summary>
        private static readonly Timer _heartbeatTimer = new(10000)
        {
            AutoReset = true
        };

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

            _server = new(Program.Setting?.Addr)
            {
                RestartAfterListenError = true
            };
            _server.Start(socket =>
            {
                socket.OnOpen = () => OnOpen(socket);
                socket.OnClose = () => OnClose(socket);
                socket.OnMessage = (message) => OnReceive(socket, message);
            });

            _heartbeatTimer.Elapsed += Heartbeat;
            _heartbeatTimer.Start();
        }

        /// <summary>
        /// 发送心跳
        /// </summary>
        private static void Heartbeat(object? sender, ElapsedEventArgs e)
        {
            lock (Instances)
            {
                Instances.Values.ToList().ForEach((instance) => instance?.Send(new SentPacket("action", "heartbeat").ToString()).Await());
            }
        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="client">客户端</param>
        private static void OnOpen(IWebSocketConnection client)
        {
            if (client is null || Program.Setting is null)
            {
                return;
            }

            string clientUrl = client.GetFullAddr();
            string guid = Sys.Guid.NewGuid().ToString("N").Substring(0, 10);
            Clients.Add(clientUrl, General.GetMD5(guid + Program.Setting.Password));
            client.Send(new SentPacket("action", "verify_request", new VerifyRequest(5000, guid)).ToString()).Await();
            Logger.Info($"<{clientUrl}> 尝试连接，预期MD5值：{General.GetMD5(guid + Program.Setting.Password)}");

            Sys.Console.Title = $"iPanel - 连接数：{Clients.Count}";

            Timer verifyTimer = new(5000) { AutoReset = false, };
            verifyTimer.Start();
            verifyTimer.Elapsed += (_, _) =>
            {
                if (!Consoles.ContainsKey(clientUrl) && !Instances.ContainsKey(clientUrl))
                {
                    verifyTimer.Stop();
                    client.Send(new SentPacket("event", "disconnection", new Reason("验证超时")).ToString()).Await();
                    client.Close();
                }
                verifyTimer.Dispose();
            };
        }

        /// <summary>
        /// 关闭处理
        /// </summary>
        /// <param name="client">客户端</param>
        private static void OnClose(IWebSocketConnection client)
        {
            if (client is null)
            {
                return;
            }
            string clientUrl = client.GetFullAddr();
            Logger.Info($"<{clientUrl}> 断开了连接");
            Clients.Remove(clientUrl);
            Instances.Remove(clientUrl);
            Consoles.Remove(clientUrl);
            System.Console.Title = $"iPanel - 连接数：{Clients.Count}";
        }

        /// <summary>
        /// 接收处理
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="message">接收信息</param>
        private static void OnReceive(IWebSocketConnection client, string message)
        {
            System.Console.Title = $"iPanel - 连接数：{Clients.Count}";
            if (client is null)
            {
                return;
            }
            string clientUrl = client.GetFullAddr();
            bool isConsole = Consoles.TryGetValue(clientUrl, out Console? console) && console is not null,
                 isInstance = Instances.TryGetValue(clientUrl, out Instance? instance) && instance is not null;
            ReceivedPacket? packet = null;
            try
            {
                packet = JsonConvert.DeserializeObject<ReceivedPacket>(message) ?? throw new Sys.ArgumentNullException();
            }
            catch (Sys.Exception e)
            {
                Logger.Warn($"<{clientUrl}>发送的数据包存在问题：{e}");
                client.Send(new SentPacket("event", (isConsole || isInstance) ? "invalid_packet" : "disconnection", new Reason($"发送的数据包存在问题：{e.Message}")).ToString()).Await();
                if (!isConsole && !isInstance)
                {
                    client.Close();
                }
                return;
            }

            if (!isConsole && !isInstance) // 对未记录的的客户端进行校验
            {
                if (packet.Type != "action" ||
                    packet.SubType != "verify")
                {
                    client.Send(new SentPacket("event", "disconnection", new Reason("你还未通过验证")).ToString()).Await();
                    client.Close();
                    return;
                }
                if (!Verify(client, packet.Data))
                {
                    client.Send(new SentPacket("event", "disconnection", new Reason("验证失败，请稍后重试")).ToString()).Await();
                    client.Close();
                }
            }
            else if (isConsole)
            {
                Handle(console!, packet);
            }
            else
            {
                Handle(instance!, packet);
            }
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">验证主体</param>
        /// <returns>验证结果</returns>
        private static bool Verify(IWebSocketConnection client, JToken? data)
        {
            if (data is null || !Clients.TryGetValue(client.GetFullAddr(), out string? md5) || string.IsNullOrEmpty(md5))
            {
                client.Send(new SentPacket("event", "verify_result", new Result(false, "数据异常")).ToString()).Await();
                Logger.Info($"<{client.GetFullAddr()}> 验证失败：数据异常");
                return false;
            }
            VerifyBody verifyBody = data.ToObject<VerifyBody>();
            if (verifyBody.MD5 != md5)
            {
                client.Send(new SentPacket("event", "verify_result", new Result(false, "MD5校验失败")).ToString()).Await();
                Logger.Info($"<{client.GetFullAddr()}> 验证失败：MD5校验失败");
                return false;
            }
            if (verifyBody.ClientType?.ToLowerInvariant() == "instance")
            {
                Instances.Add(client.GetFullAddr(), new()
                {
                    WebSocketConnection = client,
                    CustomName = verifyBody.CustomName ?? "Unknown",
                });
                client.Send(new SentPacket("event", "verify_result", new Result(true, null)).ToString()).Await();
                Logger.Info($"<{client.GetFullAddr()}> 验证成功（实例），自定义名称为：{verifyBody.CustomName ?? "Unknown"}");
                return true;
            }
            if (verifyBody.ClientType?.ToLowerInvariant() == "console")
            {
                Consoles.Add(client.GetFullAddr(), new()
                {
                    WebSocketConnection = client,
                    CustomName = verifyBody.CustomName ?? "Unknown",
                });
                client.Send(new SentPacket("event", "verify_result", new Result(true, null)).ToString()).Await();
                Logger.Info($"<{client.GetFullAddr()}> 验证成功（控制台），自定义名称为：{verifyBody.CustomName ?? "Unknown"}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 处理数据包（控制台）
        /// </summary>
        /// <param name="console">控制台客户端</param>
        /// <param name="packet">数据包</param>
        private static void Handle(Console console, ReceivedPacket packet)
        {
            switch (packet.Type)
            {
                case "action":
                    Actions.Handle(console, packet);
                    break;
                default:
                    console.Send(new SentPacket("event", "invalid_param", new Reason($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
                    break;
            }
        }

        /// <summary>
        /// 处理数据包（实例）
        /// </summary>
        /// <param name="console">实例客户端</param>
        /// <param name="packet">数据包</param>
        private static void Handle(Instance instance, ReceivedPacket packet)
        {
            instance.LastTime = Sys.DateTime.Now;
            switch (packet.Type)
            {
                case "event":
                    Events.Handle(instance, packet);
                    break;
                default:
                    instance.Send(new SentPacket("event", "invalid_param", new Reason($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
                    break;
            }
        }
    }
}
