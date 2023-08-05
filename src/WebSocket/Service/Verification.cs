using EmbedIO.WebSockets;
using iPanelHost.Permissons;
using iPanelHost.WebSocket.Client;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Utils;
using Newtonsoft.Json.Linq;
using Sys = System;
using System.Timers;

namespace iPanelHost.WebSocket.Service
{
    internal static class Verification
    {
        public static void Request(IWebSocketContext context)
        {
            if (Program.Setting is null)
            {
                return;
            }
            string clientUrl = context.RemoteEndPoint.ToString();
            if (Handler.Guids.ContainsKey(clientUrl))
            {
                return;
            }
            string guid = Sys.Guid.NewGuid().ToString("N");
            Handler.Guids.Add(clientUrl, guid);
            string shortGuid = guid.Substring(0, 10);

            context.Send(new SentPacket("action", "verify_request", new VerifyRequest(5000, shortGuid)).ToString());

            Logger.Info($"<{clientUrl}> 尝试连接，预期MD5值：{General.GetMD5(shortGuid + Program.Setting.InstancePassword)}");

            Timer verifyTimer = new(5000) { AutoReset = false };
            verifyTimer.Start();
            verifyTimer.Elapsed += (_, _) =>
            {
                if (!Handler.Consoles.ContainsKey(guid) && !Handler.Instances.ContainsKey(guid))
                {
                    context.Send(new SentPacket("event", "disconnection", new Result(Result.TimeoutInVerification)).ToString());
                    context.Close();
                }
                verifyTimer.Stop();
                verifyTimer.Dispose();
            };
        }

        /// <summary>
        /// 预检
        /// </summary>
        /// <param name="context">客户端</param>
        /// <param name="packet">数据包</param>
        public static void PreCheck(IWebSocketContext context, ReceivedPacket packet)
        {
            if (packet.Type != "action" ||
                packet.SubType != "verify")
            {
                context.Send(new SentPacket("event", "disconnection", new Result(Result.NotVerifyYet)).ToString());
                context.Close();
                return;
            }
            if (!Verify(context, packet.Data))
            {
                context.Send(new SentPacket("event", "disconnection", new Result(Result.FailToVerify)).ToString());
                context.Close();
            }
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="context">客户端</param>
        /// <param name="data">验证内容
        /// </param>
        /// <returns>验证结果</returns>
        private static bool Verify(IWebSocketContext context, JToken? data)
        {
            string clientUrl = context.RemoteEndPoint.ToString();
            if (data is null)
            {
                SendVerifyResultPacket(clientUrl, context, Result.DataAnomaly);
                return false;
            }

            VerifyBody verifyBody;
            try
            {
                verifyBody = data.ToObject<VerifyBody?>() ?? throw new Sys.ArgumentNullException();
            }
            catch (Sys.Exception e)
            {
                SendVerifyResultPacket(clientUrl, context, Result.ErrorWhenGettingPacketContent);
                Logger.Fatal(e.ToString());
                return false;
            }

            if (!Handler.Guids.TryGetValue(clientUrl, out string? guid))
            {
                SendVerifyResultPacket(clientUrl, context, Result.InternalDataError);
                return false;
            }

            switch (verifyBody.ClientType?.ToLowerInvariant())
            {
                case "instance":
                    return VerifyInstance(context, clientUrl, guid, verifyBody);

                case "console":
                    return VerifyConsole(context, clientUrl, guid, verifyBody);

                default:
                    SendVerifyResultPacket(clientUrl, context, Result.IncorrectClientType);
                    return false;
            }
        }

        private static bool VerifyInstance(IWebSocketContext context, string clientUrl, string guid, VerifyBody verifyBody)
        {
            if (verifyBody.Token != General.GetMD5(guid.Substring(0, 10) + Program.Setting.InstancePassword))
            {
                SendVerifyResultPacket(clientUrl, context, Result.FailToVerify);
                return false;
            }

            Instance instance = new(guid)
            {
                Context = context,
                CustomName = verifyBody.CustomName,
            };
            Handler.Instances.Add(guid, instance);
            context.Send(new VerifyResultPacket(true).ToString());
            Logger.Info($"<{clientUrl}> 验证成功（实例），自定义名称为：{verifyBody.CustomName ?? "null"}");
            return true;
        }

        /// <summary>
        /// 验证控制台
        /// </summary>
        /// <returns>验证结果</returns>
        private static bool VerifyConsole(IWebSocketContext context, string clientUrl, string guid, VerifyBody verifyBody)
        {
            if (string.IsNullOrEmpty(verifyBody.Account))
            {
                SendVerifyResultPacket(clientUrl, context, Result.EmptyAccount);
                return false;
            }

            if (!(UserManager.Users.TryGetValue(verifyBody.Account!, out User? user) &&
                  verifyBody.Token == General.GetMD5(guid.Substring(0, 10) + verifyBody.Account! + user.Password)))
            {
                SendVerifyResultPacket(clientUrl, context, Result.IncorrectAccountOrPassword);
                return false;
            }

            Console console = new(guid)
            {
                Context = context,
                User = user
            };

            Handler.Consoles.Add(guid, console);
            SendVerifyResultPacket(clientUrl, context);
            Logger.Info($"<{clientUrl}> 验证成功（控制台）");
            return true;
        }

        private static void SendVerifyResultPacket(string clientUrl, IWebSocketContext context, string? reason = null)
        {
            if (string.IsNullOrEmpty(reason))
            {
                context.Send(new VerifyResultPacket(true).ToString());
                return;
            }
            context.Send(new VerifyResultPacket(false, reason).ToString());
            Logger.Warn($"<{clientUrl}> 验证失败：{reason}");
        }
    }
}
