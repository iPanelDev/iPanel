using iPanel.Core.Models.Client;
using iPanel.Core.Models.Packets;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace iPanel.Core.Server.WebSocket.Handlers;

public abstract class HandlerBase
{
    protected HandlerBase(IHost host)
    {
        _host = host;
    }

    protected readonly IHost _host;
    protected IServiceProvider Services => _host.Services;

    public abstract Task Handle(Instance instance, WsReceivedPacket packet);
}
