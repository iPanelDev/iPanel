using System.Threading.Tasks;
using iPanel.Core.Models.Client;
using iPanel.Core.Models.Packets;

namespace iPanel.Core.Server.WebSocket.Handlers;

public abstract class HandlerBase
{
    protected readonly App _app;

    protected HandlerBase(App app)
    {
        _app = app;
    }

    public abstract Task Handle(Instance instance, WsReceivedPacket packet);
}
