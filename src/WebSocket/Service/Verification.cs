using Fleck;
using Newtonsoft.Json.Linq;
using iPanelHost.WebSocket.Client;
using iPanelHost.WebSocket.Packets;
using iPanelHost.WebSocket.Packets.DataBody;
using iPanelHost.WebSocket.Packets.Event;
using iPanelHost.Utils;
using Sys = System;
using System.Timers;

namespace iPanelHost.WebSocket.Service
{
    internal static class Verification
    {
        public static void Request(IWebSocketConnection connection)
        {
            if (Program.Setting is null)
            {
                return;
            }
            string clientUrl = connection.GetFullAddr();
            string guid = connection.ConnectionInfo.Id.ToString("N");
            string shortGuid = guid.Substring(0, 10);

            connection.Send(new SentPacket("action", "verify_request", new VerifyRequest(5000, shortGuid)).ToString()).Await();

            Logger.Info($"<{clientUrl}> 尝试连接，预期MD5值：{General.GetMD5(shortGuid + Program.Setting.WebSocket.Password)}");

            Timer verifyTimer = new(5000) { AutoReset = false };
            verifyTimer.Start();
            verifyTimer.Elapsed += (_, _) =>
            {
                if (!Handler.Consoles.ContainsKey(guid) && !Handler.Instances.ContainsKey(guid) && connection.IsAvailable)
                {
                    connection.Send(new SentPacket("event", "disconnection", new Reason("验证超时")).ToString()).Await();
                    connection.Close();
                }
                verifyTimer.Stop();
                verifyTimer.Dispose();
            };
        }

        /// <summary>
        /// 预检
        /// </summary>
        /// <param name="connection">客户端</param>
        /// <param name="packet">数据包</param>
        public static void PreCheck(IWebSocketConnection connection, ReceivedPacket packet)
        {
            if (packet.Type != "action" ||
                packet.SubType != "verify")
            {
                connection.Send(new SentPacket("event", "disconnection", new Reason("你还未通过验证")).ToString()).Await();
                connection.Close();
                return;
            }
            if (!Verify(connection, packet.Data))
            {
                connection.Send(new SentPacket("event", "disconnection", new Reason("验证失败，请稍后重试")).ToString()).Await();
                connection.Close();
            }
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="connection">客户端</param>
        /// <param name="data">验证内容
        /// </param>
        /// <returns>验证结果</returns>
        private static bool Verify(IWebSocketConnection connection, JToken? data)
        {
            if (data is null)
            {
                connection.Send(new VerifyResultPacket(false, "数据异常").ToString()).Await();
                Logger.Warn($"<{connection.GetFullAddr()}> 验证失败：数据异常");
                return false;
            }

            VerifyBody verifyBody;
            try
            {
                verifyBody = data.ToObject<VerifyBody?>() ?? throw new Sys.ArgumentNullException();
            }
            catch (Sys.Exception e)
            {
                Logger.Error($"{connection.GetFullAddr()} 获取验证内容时异常\n{e}");
                return false;
            }

            if (verifyBody.Token != General.GetMD5(connection.ConnectionInfo.Id.ToString("N").Substring(0, 10) + Program.Setting.WebSocket.Password))
            {
                connection.Send(new VerifyResultPacket(false, "MD5校验失败").ToString()).Await();
                Logger.Warn($"<{connection.GetFullAddr()}> 验证失败：MD5校验失败");
                return false;
            }

            string guid = connection.ConnectionInfo.Id.ToString("N");
            if (verifyBody.ClientType?.ToLowerInvariant() == "instance")
            {
                Instance instance = new(guid)
                {
                    WebSocketConnection = connection,
                    CustomName = verifyBody.CustomName,
                };
                Handler.Instances.Add(guid, instance);
                connection.Send(new VerifyResultPacket(true).ToString()).Await();
                Logger.Info($"<{connection.GetFullAddr()}> 验证成功（实例），自定义名称为：{verifyBody.CustomName ?? "null"}");
                return true;
            }

            if (verifyBody.ClientType?.ToLowerInvariant() == "console")
            {
                Console console = new(guid)
                {
                    WebSocketConnection = connection,
                    CustomName = verifyBody.CustomName,
                };
                Handler.Consoles.Add(guid, console);
                connection.Send(new VerifyResultPacket(true).ToString()).Await();
                Logger.Info($"<{connection.GetFullAddr()}> 验证成功（控制台），自定义名称为：{verifyBody.CustomName ?? "null"}");
                return true;
            }
            return false;
        }
    }
}
