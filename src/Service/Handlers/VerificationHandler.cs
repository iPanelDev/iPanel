using EmbedIO.WebSockets;
using iPanelHost.Base;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Server;
using iPanelHost.Base.Client;
using iPanelHost.Utils;
using Newtonsoft.Json.Linq;
using Sys = System;
using System.Linq;
using System.Timers;
using System.Text.RegularExpressions;

namespace iPanelHost.Service.Handlers;

public static class VerificationHandler
{
    /// <summary>
    /// 请求验证
    /// </summary>
    /// <param name="context">上下文</param>
    public static void Request(IWebSocketContext context)
    {
        if (Program.Setting is null)
        {
            return;
        }
        string clientUrl = context.RemoteEndPoint.ToString();
        if (MainHandler.UUIDs.ContainsKey(clientUrl))
        {
            return;
        }
        string uuid = Sys.Guid.NewGuid().ToString("N");
        MainHandler.UUIDs.Add(clientUrl, uuid);

        context.Send(
            new SentPacket("request", "verify_request", new VerifyRequest(5000, uuid)).ToString()
        );

        Logger.Info(
            $"<{clientUrl}> 尝试连接，预期实例MD5值：{General.GetMD5(uuid + Program.Setting.InstancePassword)}"
        );

        Timer verifyTimer = new(5000) { AutoReset = false };
        verifyTimer.Start();
        verifyTimer.Elapsed += (_, _) =>
        {
            if (!MainHandler.Consoles.ContainsKey(uuid) && !MainHandler.Instances.ContainsKey(uuid))
            {
                context.Send(
                    new SentPacket(
                        "event",
                        "disconnection",
                        new Result(Result.TimeoutInVerification)
                    ).ToString()
                );
                context.Close();
            }
            verifyTimer.Stop();
            verifyTimer.Dispose();
        };
    }

    /// <summary>
    /// 检查数据包
    /// </summary>
    /// <param name="context">上下文</param>
    /// <param name="packet">数据包</param>
    public static void Check(IWebSocketContext context, ReceivedPacket packet)
    {
        if (packet.Type != "request" || packet.SubType != "verify")
        {
            context.Send(
                new SentPacket("event", "disconnection", new Result(Result.NotVerifyYet)).ToString()
            );
            context.Close();
            return;
        }
        if (!Verify(context, packet.Data))
        {
            context.Send(
                new SentPacket("event", "disconnection", new Result(Result.FailToVerify)).ToString()
            );
            context.Close();
        }
    }

    /// <summary>
    /// 验证
    /// </summary>
    /// <param name="context">上下文</param>
    /// <param name="data">验证内容</param>
    /// <returns>验证结果</returns>
    private static bool Verify(IWebSocketContext context, JToken? data)
    {
        string clientUrl = context.RemoteEndPoint.ToString();
        if (data is null)
        {
            SendVerifyResultPacket(context, Result.DataAnomaly);
            return false;
        }

        VerifyBody verifyBody;
        try
        {
            verifyBody = data.ToObject<VerifyBody?>() ?? throw new Sys.NullReferenceException();
        }
        catch (Sys.Exception e)
        {
            SendVerifyResultPacket(context, Result.ErrorWhenGettingPacketContent);
            Logger.Fatal(e.ToString());
            return false;
        }

        if (!MainHandler.UUIDs.TryGetValue(clientUrl, out string? uuid))
        {
            SendVerifyResultPacket(context, Result.InternalDataError);
            return false;
        }

        switch (verifyBody.ClientType?.ToLowerInvariant())
        {
            case "instance":
                return VerifyInstance(context, clientUrl, uuid, verifyBody);

            case "console":
                return VerifyConsole(context, clientUrl, uuid, verifyBody);

            default:
                SendVerifyResultPacket(context, Result.IncorrectClientType);
                return false;
        }
    }

    /// <summary>
    /// 验证实例
    /// </summary>
    /// <returns>验证结果</returns>
    private static bool VerifyInstance(
        IWebSocketContext context,
        string clientUrl,
        string uuid,
        VerifyBody verifyBody
    )
    {
        if (verifyBody.Token != General.GetMD5(uuid + Program.Setting.InstancePassword))
        {
            SendVerifyResultPacket(context, Result.FailToVerify);
            return false;
        }

        if (
            string.IsNullOrEmpty(verifyBody.InstanceID)
            || !Regex.IsMatch(verifyBody.InstanceID, @"^\w{32}$")
        )
        {
            SendVerifyResultPacket(context, Result.IncorrectInstanceID);
            return false;
        }

        if (
            MainHandler.Instances.Values.Select((i) => i.InstanceID).Contains(verifyBody.InstanceID)
        )
        {
            SendVerifyResultPacket(context, Result.DuplicateInstanceID);
            return false;
        }

        Instance instance =
            new(uuid)
            {
                Context = context,
                CustomName = verifyBody.CustomName,
                InstanceID = verifyBody.InstanceID,
                Metadata = verifyBody.MetaData,
            };
        MainHandler.Instances.Add(uuid, instance);
        context.Send(new VerifyResultPacket(true).ToString());
        Logger.Info($"<{clientUrl}> 验证成功（实例），自定义名称为：{verifyBody.CustomName ?? "null"}");
        return true;
    }

    /// <summary>
    /// 验证控制台
    /// </summary>
    /// <returns>验证结果</returns>
    private static bool VerifyConsole(
        IWebSocketContext context,
        string clientUrl,
        string uuid,
        VerifyBody verifyBody
    )
    {
        if (string.IsNullOrEmpty(verifyBody.Account))
        {
            SendVerifyResultPacket(context, Result.EmptyAccount);
            return false;
        }

        if (
            !(
                UserManager.Users.TryGetValue(verifyBody.Account!, out User? user)
                && verifyBody.Token == General.GetMD5(uuid + verifyBody.Account! + user.Password)
            )
        )
        {
            SendVerifyResultPacket(context, Result.IncorrectAccountOrPassword);
            return false;
        }

        user.LastLoginTime = Sys.DateTime.Now;
        user.IPAddresses.Insert(0, context.RemoteEndPoint.ToString());
        if (user.IPAddresses.Count > 10)
        {
            user.IPAddresses.RemoveRange(10, user.IPAddresses.Count - 10);
        }

        Console console = new(uuid) { Context = context, User = user };

        MainHandler.Consoles.Add(uuid, console);
        SendVerifyResultPacket(context);
        Logger.Info($"<{clientUrl}> 验证成功（控制台）");
        return true;
    }

    /// <summary>
    /// 发送验证数据包
    /// </summary>
    /// <param name="clientUrl"></param>
    /// <param name="context"></param>
    /// <param name="reason"></param>
    private static void SendVerifyResultPacket(IWebSocketContext context, string? reason = null)
    {
        if (string.IsNullOrEmpty(reason))
        {
            context.Send(new VerifyResultPacket(true).ToString());
            return;
        }
        context.Send(new VerifyResultPacket(false, reason).ToString());
        Logger.Warn($"<{context.RemoteEndPoint}> 验证失败：{reason}");
    }
}
