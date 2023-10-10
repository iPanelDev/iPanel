using EmbedIO.WebSockets;
using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Server.WebSocket.Handlers;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace iPanelHost.Server.WebSocket;

public class MainWsModule : WebSocketModule
{
    /// <summary>
    /// 心跳计时器
    /// </summary>
    private static readonly System.Timers.Timer _heartbeatTimer = new(5000);

    public MainWsModule()
        : base("/ws", true)
    {
        Encoding = new UTF8Encoding(false);
    }

    /// <summary>
    /// 实例字典
    /// </summary>
    public static readonly Dictionary<string, Instance> Instances = new();

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

    protected override void OnStart(CancellationToken cancellationToken)
    {
        Instances.Clear();
        _heartbeatTimer.Elapsed += Heartbeat;
        _heartbeatTimer.Start();
    }

    protected override Task OnMessageReceivedAsync(
        IWebSocketContext context,
        byte[] buffer,
        IWebSocketReceiveResult result
    )
    {
        ReceivedPacket? packet;
        string instanceId = context.Session["instanceId"]?.ToString() ?? string.Empty;
        string clientUrl = context.RemoteEndPoint.ToString();
        string message = Encoding.GetString(buffer);
        bool isInstance =
            Instances.TryGetValue(instanceId, out Instance? instance) && instance is not null;

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
            return Task.CompletedTask;
        }

        if (!isInstance || instance is null) // 对未记录的的上下文进行校验
        {
            VerificationHandler.Check(context, packet);
        }
        else
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
        return Task.CompletedTask;
    }

    protected override Task OnClientConnectedAsync(IWebSocketContext context)
    {
        if (context is not null)
        {
            context.Session.Delete();
            VerificationHandler.Request(context);
        }
        return Task.CompletedTask;
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
    {
        Logger.Info($"<{context.RemoteEndPoint}> 断开了连接");

        string? instanceId = context.Session["instanceId"]?.ToString();
        context.Session.Delete();
        if (!string.IsNullOrEmpty(instanceId))
        {
            Instances.Remove(instanceId);
        }
        return Task.CompletedTask;
    }
}
