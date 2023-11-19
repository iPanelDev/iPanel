using EmbedIO.WebSockets;
using iPanel.Core.Models.Client;
using iPanel.Core.Models.Exceptions;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Packets.Data;
using iPanel.Core.Models.Packets.Event;
using iPanel.Core.Server.WebSocket.Handlers;
using iPanel.Utils;
using iPanel.Utils.Extensions;
using iPanel.Utils.Json;
using Spectre.Console;
using Spectre.Console.Json;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace iPanel.Core.Server.WebSocket;

public class InstanceWsModule : WebSocketModule
{
    private readonly Timer _heartbeatTimer = new(5000);

    private readonly Dictionary<string, HandlerBase> _handlers = new();

    public InstanceWsModule(App app)
        : base("/instance", true)
    {
        Encoding = new UTF8Encoding(false);

        _app = app;
        _heartbeatTimer.Elapsed += async (_, _) =>
            await BroadcastAsync(
                new WsSentPacket("request", "heartbeat"),
                (context) =>
                    !string.IsNullOrEmpty(context.Session[SessionKeyConstants.InstanceId] as string)
            );

        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            var attributes = type.GetCustomAttributes<HandlerAttribute>();
            if (!attributes.Any())
                continue;

            var handler = (HandlerBase?)Activator.CreateInstance(type, _app);

            if (handler is null)
                continue;

            foreach (var attribute in attributes)
                _handlers[attribute.Path] = handler;
        }
    }

    public readonly Dictionary<string, Instance> Instances = new();
    private readonly App _app;

    protected override void OnStart(CancellationToken cancellationToken)
    {
        _heartbeatTimer.Start();
        Logger.Info("实例WS服务器已开启");
    }

    protected override async Task OnMessageReceivedAsync(
        IWebSocketContext context,
        byte[] buffer,
        IWebSocketReceiveResult result
    )
    {
        WsReceivedPacket? packet;
        var instanceId = context.Session[SessionKeyConstants.InstanceId] as string ?? string.Empty;
        var clientUrl = context.RemoteEndPoint.ToString();
        var message = Encoding.GetString(buffer);
        var verified = Instances.TryGetValue(instanceId, out Instance? instance);

        try
        {
            packet =
                JsonSerializer.Deserialize<WsReceivedPacket>(
                    message,
                    JsonSerializerOptionsFactory.CamelCase
                ) ?? throw new PacketException("空数据包");

            if (_app.Setting.Debug)
            {
                Logger.Debug($"[{clientUrl}] 收到数据");

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
            Logger.Warn(e, nameof(InstanceWsModule), $"[{clientUrl}] 处理数据包异常");
            await context.SendAsync(
                new WsSentPacket(
                    "event",
                    verified ? "invalid_packet" : "disconnection",
                    new Result($"发送的数据包存在问题：{e.Message}")
                )
            );

            if (!verified)
                await context.CloseAsync();

            return;
        }

        var path = $"{packet.Type}.{packet.SubType}";

        if (!verified || instance is null)
        {
            if (path != "request.verify")
                await context.CloseAsync();

            await Verify(context, packet);
            return;
        }

        if (_handlers.TryGetValue(path, out var handler))
            await handler.Handle(instance, packet);
        else if (_handlers.TryGetValue($"{packet.Type}.*", out handler))
            await handler.Handle(instance, packet);
        else
            await context.SendAsync(
                new WsSentPacket(
                    "event",
                    "invalid_param",
                    new Result($"所请求的“{packet.Type}”类型不存在或无法调用")
                )
            );
    }

    private async Task Verify(IWebSocketContext context, WsReceivedPacket packet)
    {
        try
        {
            var verifyBody =
                packet.Data?.ToObject<VerifyBody>() ?? throw new PacketException("数据为空");

            if (string.IsNullOrEmpty(verifyBody.Time))
                throw new PacketException($"{nameof(verifyBody.Time)}为空");

            if (!DateTime.TryParse(verifyBody.Time, out DateTime dateTime))
                throw new PacketException($"{nameof(verifyBody.Time)}无效");

            var span = dateTime - DateTime.Now;
            if (span.TotalSeconds < -10 || span.TotalMinutes > 10)
                throw new PacketException($"{nameof(verifyBody.Time)}过期");

            if (
                string.IsNullOrEmpty(verifyBody.InstanceId)
                || verifyBody.InstanceId.Length != 32
                || !Regex.IsMatch(verifyBody.InstanceId, @"^\w{32}$")
                || Instances.ContainsKey(verifyBody.InstanceId)
            )
                throw new PacketException($"{nameof(verifyBody.InstanceId)}无效");

            var expectedValue = Encryption.GetMD5(
                $"{verifyBody.Time}.{_app.Setting.InstancePassword}"
            );
            if (verifyBody.MD5 == expectedValue)
            {
                Instances.Add(
                    verifyBody.InstanceId,
                    new(verifyBody.InstanceId)
                    {
                        CustomName = verifyBody.CustomName,
                        Context = context,
                        Metadata = verifyBody.Metadata ?? new()
                    }
                );

                await context.SendAsync(new VerifyResultPacket(true));
                context.Session[SessionKeyConstants.InstanceId] = verifyBody.InstanceId;
                Logger.Info($"[{context.RemoteEndPoint}] 验证成功");
                return;
            }

            Logger.Warn(
                $"[{context.RemoteEndPoint}] 预期MD5：\"{expectedValue}\"，实际接收：\"{verifyBody.MD5}\""
            );
            throw new PacketException("验证失败");
        }
        catch (Exception e)
        {
            Logger.Warn($"[{context.RemoteEndPoint}] 验证失败：{e.Message}");
            await context.SendAsync(new VerifyResultPacket(false, e.Message));
            await context.CloseAsync();
        }
    }

    protected override Task OnClientConnectedAsync(IWebSocketContext context)
    {
        context.Session[SessionKeyConstants.InstanceId] = string.Empty;
        Logger.Info($"[{context.RemoteEndPoint}] 连接到实例WS服务器");

        Task.Run(async () =>
        {
            await Task.Delay(5000);

            var instanceId =
                context.Session[SessionKeyConstants.InstanceId] as string ?? string.Empty;
            if (
                (string.IsNullOrEmpty(instanceId) || !Instances.ContainsKey(instanceId))
                && context.WebSocket.State == WebSocketState.Open
            )
            {
                await context.SendAsync(new VerifyResultPacket(false, "验证超时"));
                await context.CloseAsync();
                Logger.Warn($"[{context.RemoteEndPoint}] 验证超时");
            }
        });

        return Task.CompletedTask;
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
    {
        Logger.Info($"[{context.RemoteEndPoint}] 从实例WS服务器断开了连接");

        var instanceId = context.Session[SessionKeyConstants.InstanceId]?.ToString();
        context.Session[SessionKeyConstants.InstanceId] = string.Empty;

        if (!string.IsNullOrEmpty(instanceId))
            Instances.Remove(instanceId);

        return Task.CompletedTask;
    }
}
