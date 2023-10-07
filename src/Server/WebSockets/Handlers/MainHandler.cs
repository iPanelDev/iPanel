using EmbedIO.WebSockets;
using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace iPanelHost.Server.WebSocket.Handlers;

public static class MainHandler
{
    /// <summary>
    /// 实例字典
    /// </summary>
    public static readonly Dictionary<string, Instance> Instances = new();

    /// <summary>
    /// UUID字典
    /// </summary>
    public static readonly Dictionary<string, string> UUIDs = new();

    /// <summary>
    /// 发送心跳
    /// </summary>
    public static void Heartbeat(object? sender, ElapsedEventArgs e)
    {
        lock (Instances)
        {
            Instances.Values
                .ToList()
                .ForEach(
                    (instance) => instance?.Send(new SentPacket("request", "heartbeat").ToString())
                );
        }
    }

    /// <summary>
    /// 连接处理
    /// </summary>
    /// <param name="context">上下文</param>
    public static void OnOpen(IWebSocketContext context)
    {
        if (context is null)
        {
            return;
        }
        VerificationHandler.Request(context);
        UpdateTitle();
    }

    /// <summary>
    /// 关闭处理
    /// </summary>
    /// <param name="context">上下文</param>
    public static void OnClose(IWebSocketContext context)
    {
        if (context is null)
        {
            return;
        }

        string clientUrl = context.RemoteEndPoint.ToString();
        if (!UUIDs.TryGetValue(clientUrl, out string? uuid))
        {
            return;
        }

        Logger.Info($"<{clientUrl}> 断开了连接");
        Instances.Remove(uuid);
        UUIDs.Remove(clientUrl);
        UpdateTitle();
    }

    /// <summary>
    /// 接收处理
    /// </summary>
    /// <param name="context">上下文</param>
    /// <param name="message">接收信息</param>
    public static void OnReceive(IWebSocketContext context, string message)
    {
        if (context is null)
        {
            return;
        }
        UpdateTitle();

        string clientUrl = context.RemoteEndPoint.ToString();
        if (!UUIDs.TryGetValue(clientUrl, out string? uuid))
        {
            return;
        }
        bool isInstance =
            Instances.TryGetValue(uuid, out Instance? instance) && instance is not null;

        ReceivedPacket? packet;
        try
        {
            packet =
                JsonConvert.DeserializeObject<ReceivedPacket>(message)
                ?? throw new PacketException("空数据包");

            if (Program.Setting.Debug)
            {
                Logger.Debug($"<{clientUrl}> 收到数据");
                AnsiConsole.Write(
                    new JsonText(message)
                        .BracesColor(Color.White) // 中括号
                        .BracketColor(Color.White) // 大括号
                        .CommaColor(Color.White) // 逗号
                        .MemberColor(Color.SkyBlue1)
                        .StringColor(Color.LightSalmon3_1)
                        .NumberColor(Color.DarkSeaGreen2)
                        .BooleanColor(Color.DodgerBlue3)
                        .NullColor(Color.DodgerBlue3)
                );
                AnsiConsole.WriteLine();
            }
        }
        catch (Exception e)
        {
            if (Program.Setting.Debug)
            {
                Logger.Debug($"<{clientUrl}> 收到数据：{message}");
            }

            Logger.Warn($"<{clientUrl}> 处理数据包异常\n{e}");
            context.Send(
                new SentPacket(
                    "event",
                    isInstance ? "invalid_packet" : "disconnection",
                    new Result($"发送的数据包存在问题：{e.Message}")
                ).ToString()
            );
            if (!isInstance)
            {
                context.Close();
            }
            return;
        }

        if (!isInstance) // 对未记录的的上下文进行校验
        {
            VerificationHandler.Check(context, packet);
        }
        else
        {
            Handle(instance!, packet);
        }
    }

    /// <summary>
    /// 处理数据包（实例）
    /// </summary>
    /// <param name="console">实例上下文</param>
    /// <param name="packet">数据包</param>
    private static void Handle(Instance instance, ReceivedPacket packet)
    {
        switch (packet.Type)
        {
            case "broadcast":
                BroadcastHandler.Handle(instance, packet);
                break;

            case "return":
                ReturnHandler.Handle(instance, packet);
                break;

            default:
                instance.Send(
                    new SentPacket(
                        "event",
                        "invalid_param",
                        new Result($"所请求的“{packet.Type}”类型不存在或无法调用")
                    ).ToString()
                );
                break;
        }
    }

    /// <summary>
    /// 更新窗口标题
    /// </summary>
    private static void UpdateTitle()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            Console.Title =
                $"iPanel Host {Constant.VERSION} [{MainWsModule.Instance?.ActiveContextsCount ?? 0} 连接]";
        }
    }
}
