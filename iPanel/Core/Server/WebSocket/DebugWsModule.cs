using EmbedIO.WebSockets;
using iPanel.Utils;
using iPanel.Utils.Extensions;
using iPanel.Utils.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace iPanel.Core.Server.WebSocket;

public class DebugWsModule : WebSocketModule
{
    private readonly string _password = Guid.NewGuid().ToString("N");

    private readonly List<string> _ips = new();
    private readonly IHost _host;
    private IServiceProvider Services => _host.Services;
    private ILogger<BroadcastWsModule> Logger =>
        Services.GetRequiredService<ILogger<BroadcastWsModule>>();

    public DebugWsModule(IHost host)
        : base("/ws/debug", true)
    {
        _host = host;
        SimpleLogger.OnMessage = (level, lines) =>
        {
            try
            {
                BroadcastAsync(
                    JsonSerializer.Serialize(
                        new JsonObject
                        {
                            { nameof(lines), JsonSerializer.SerializeToNode(lines) },
                            { nameof(level), level }
                        },
                        JsonSerializerOptionsFactory.CamelCase
                    ),
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
        Logger.LogInformation("[{}] 从调试服务器断开", address);
        return Task.CompletedTask;
    }

    protected override Task OnClientConnectedAsync(IWebSocketContext context)
    {
        Logger.LogInformation("[{}] 连接到调试服务器", context.RemoteEndPoint);
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
        if (data == _password)
        {
            Logger.LogInformation("[{}] 通过调试服务器验证", address);
            Logger.LogWarning("[{}] 如果这不是信任的连接，请立即停止iPanel并在设置中禁用调试", address);
            _ips.Add(address);
        }
        else
            await context.CloseAsync();
    }

    protected override void OnStart(CancellationToken cancellationToken)
    {
        Logger.LogInformation("调试服务器已开启。连接密码：[{}]", _password);
    }
}
