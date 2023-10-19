using EmbedIO.WebSockets;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Base.Client;
using iPanelHost.Utils;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.Linq;
using System.Timers;
using System.Text.RegularExpressions;

namespace iPanelHost.Server.WebSocket.Handlers;

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
        string uuid = Guid.NewGuid().ToString("N");
        context.Session["uuid"] = uuid;

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
            if (string.IsNullOrEmpty(context.Session["instanceId"]?.ToString()))
            {
                context.Send(
                    new SentPacket(
                        "event",
                        "disconnection",
                        new Result(ResultTypes.TimeoutInVerification)
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
                new SentPacket(
                    "event",
                    "disconnection",
                    new Result(ResultTypes.NotVerifyYet)
                ).ToString()
            );
            context.Close();
            return;
        }
        if (!Verify(context, packet.Data))
        {
            context.Send(
                new SentPacket(
                    "event",
                    "disconnection",
                    new Result(ResultTypes.FailToVerify)
                ).ToString()
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
        string? uuid = context.Session["uuid"]?.ToString();

        if (data is null || string.IsNullOrEmpty(uuid) || uuid.Length != 32)
        {
            SendVerifyResultPacket(context, ResultTypes.DataAnomaly);
            return false;
        }

        VerifyBody verifyBody;
        try
        {
            verifyBody = data.ToObject<VerifyBody?>() ?? throw new NullReferenceException();
        }
        catch (Exception e)
        {
            SendVerifyResultPacket(context, ResultTypes.ErrorWhenGettingPacketContent);
            Logger.Error(e);
            return false;
        }
        if (verifyBody.Token != General.GetMD5(uuid + Program.Setting.InstancePassword))
        {
            SendVerifyResultPacket(context, ResultTypes.FailToVerify);
            return false;
        }

        if (
            string.IsNullOrEmpty(verifyBody.InstanceID)
            || !Regex.IsMatch(verifyBody.InstanceID, @"^\w{32}$")
        )
        {
            SendVerifyResultPacket(context, ResultTypes.IncorrectInstanceID);
            return false;
        }

        if (
            MainWsModule.Instances.Values
                .Select((i) => i.InstanceID)
                .Contains(verifyBody.InstanceID)
        )
        {
            SendVerifyResultPacket(context, ResultTypes.DuplicateInstanceID);
            return false;
        }

        Instance instance =
            new(verifyBody.InstanceID)
            {
                Context = context,
                CustomName = verifyBody.CustomName,
                Metadata = verifyBody.Metadata ?? new(),
            };
        MainWsModule.Instances.Add(verifyBody.InstanceID, instance);
        context.Session["instanceId"] = verifyBody.InstanceID;
        context.Send(new VerifyResultPacket(true).ToString());
        Logger.Info($"<{clientUrl}> 验证成功");

        AnsiConsole.Write(
            new Table()
                .AddColumns(
                    new TableColumn("类型") { Alignment = Justify.Center },
                    new("实例") { Alignment = Justify.Center }
                )
                .AddRow("地址", context.RemoteEndPoint.ToString())
                .AddRow("自定义名称", Markup.Escape(verifyBody.CustomName ?? string.Empty))
                .AddRow("实例ID", Markup.Escape(verifyBody.InstanceID))
                .AddRow("实例名称", Markup.Escape(verifyBody.Metadata?.Name ?? string.Empty))
                .AddRow("实例版本", Markup.Escape(verifyBody.Metadata?.Version ?? string.Empty))
                .RoundedBorder()
        );
        AnsiConsole.WriteLine();
        return true;
    }

    /// <summary>
    /// 发送验证数据包
    /// </summary>
    /// <param name="clientUrl"></param>
    /// <param name="context"></param>
    /// <param name="reason"></param>
    private static void SendVerifyResultPacket(
        IWebSocketContext context,
        ResultTypes reason = ResultTypes.Success
    )
    {
        if (reason == ResultTypes.Success)
        {
            context.Send(new VerifyResultPacket(true).ToString());
            return;
        }
        context.Send(new VerifyResultPacket(false, reason.ToString()).ToString());
        Logger.Warn($"<{context.RemoteEndPoint}> 验证失败：{reason}");
    }
}
