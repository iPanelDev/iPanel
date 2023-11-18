using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO.WebSockets;
using iPanel.Utils;
using iPanel.Utils.Extensions;
using iPanel.Utils.Json;
using Swan.Logging;

namespace iPanel.Core.Server.WebSocket;

public class DebugWsModule : WebSocketModule
{
    private readonly string _pwd = Guid.NewGuid().ToString("N");

    private readonly List<string> _ips = new();

    public DebugWsModule()
        : base("/debug", true)
    {
        LocalLogger.OnMessage = (e) =>
        {
            try
            {
                BroadcastAsync(
                    JsonSerializer.Serialize(e, JsonSerializerOptionsFactory.CamelCase),
                    (ctx) => _ips.Contains(ctx.RemoteEndPoint.ToString())
                );
            }
            catch { }
        };
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
    {
        var address = context.RemoteEndPoint.ToString();
        _ips.Remove(address);
        Logger.Info($"[{address}] 从调试服务器断开");
        return Task.CompletedTask;
    }

    protected override Task OnClientConnectedAsync(IWebSocketContext context)
    {
        Logger.Info($"[{context.RemoteEndPoint}] 连接到调试服务器");
        return Task.CompletedTask;
    }

    protected override async Task OnMessageReceivedAsync(
        IWebSocketContext context,
        byte[] buffer,
        IWebSocketReceiveResult result
    )
    {
        var data = Encoding.GetString(buffer);
        var address = context.RemoteEndPoint.ToString();
        if (data == _pwd)
        {
            Logger.Info($"[{address}] 通过调试服务器验证");
            Logger.Warn($"[{address}] 如果这不是信任的连接，请立即停止iPanel并在设置中禁用调试");
            _ips.Add(address);
        }
        else
            await context.CloseAsync();
    }

    protected override void OnStart(CancellationToken cancellationToken)
    {
        Logger.Info($"调试服务器已开启。连接密码：[{_pwd}]");
    }
}
