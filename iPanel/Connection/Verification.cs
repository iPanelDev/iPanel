using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using iPanel.Base;
using iPanel.Core.Connection;
using iPanel.Core.Packets;
using iPanel.Core.Packets.DataBody;
using iPanel.Core.Service;
using iPanel.Utils;
using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace iPanel.Core.Connection
{
    internal static class Verification
    {
        public static void Request(IWebSocketConnection client)
        {
            if (Program.Setting is null)
            {
                return;
            }
            string clientUrl = client.GetFullAddr();
            string guid = Sys.Guid.NewGuid().ToString("N").Substring(0, 10);
            Handler.Clients.Add(clientUrl, General.GetMD5(guid + Program.Setting.Password));
            client.Send(new SentPacket("action", "verify_request", new VerifyRequest(5000, guid)).ToString()).Await();
            Logger.Info($"<{clientUrl}> 尝试连接，预期MD5值：{General.GetMD5(guid + Program.Setting.Password)}");


            Timer verifyTimer = new(5000) { AutoReset = false };
            verifyTimer.Start();
            verifyTimer.Elapsed += (_, _) =>
            {
                if (!Handler.Consoles.ContainsKey(clientUrl) && !Handler.Instances.ContainsKey(clientUrl))
                {
                    verifyTimer.Stop();
                    client.Send(new SentPacket("event", "disconnection", new Reason("验证超时")).ToString()).Await();
                    client.Close();
                }
                verifyTimer.Dispose();
            };
        }

        /// <summary>
        /// 预检
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        public static void PreCheck(IWebSocketConnection client, ReceivedPacket packet)
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

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">验证内容
        /// </param>
        /// <returns>验证结果</returns>
        private static bool Verify(IWebSocketConnection client, JToken? data)
        {
            if (data is null || !Handler.Clients.TryGetValue(client.GetFullAddr(), out string? token) || string.IsNullOrEmpty(token))
            {
                client.Send(new SentPacket("event", "verify_result", new Result(false, "数据异常")).ToString()).Await();
                Logger.Warn($"<{client.GetFullAddr()}> 验证失败：数据异常");
                return false;
            }

            VerifyBody verifyBody;
            try
            {
                verifyBody = data.ToObject<VerifyBody>();
            }
            catch (Sys.Exception e)
            {
                Logger.Error($"{client.GetFullAddr()} 获取验证内容时异常\n{e}");
                return false;
            }

            if (verifyBody.Token != token)
            {
                client.Send(new SentPacket("event", "verify_result", new Result(false, "MD5校验失败")).ToString()).Await();
                Logger.Warn($"<{client.GetFullAddr()}> 验证失败：MD5校验失败");
                return false;
            }

            if (verifyBody.ClientType?.ToLowerInvariant() == "instance")
            {
                Handler.Instances.Add(client.GetFullAddr(), new()
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
                Handler.Consoles.Add(client.GetFullAddr(), new()
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
    }
}
