using System;
using System.Net;
using System.Threading.Tasks;

using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

using iPanel.Core.Server.WebSocket;
using iPanel.Core.Service;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iPanel.Core.Server.Api;

public partial class ApiMap : WebApiController
{
    private readonly IHost _host;
    private IServiceProvider Services => _host.Services;
    private ILogger<ApiMap> Logger => Services.GetRequiredService<ILogger<ApiMap>>();
    private UserManager UserManager => Services.GetRequiredService<UserManager>();
    private InstanceWsModule InstanceWsModule => Services.GetRequiredService<InstanceWsModule>();
    private BroadcastWsModule BroadcastWsModule => Services.GetRequiredService<BroadcastWsModule>();

    public ApiMap(IHost host)
    {
        _host = host;
    }

    [Route(HttpVerbs.Get, "/")]
    public async Task Root()
    {
        await HttpContext.SendPacketAsync("Hello world. :)");
    }

    [Route(HttpVerbs.Get, "/version")]
    public async Task GetVersion()
    {
        await HttpContext.SendPacketAsync(Constant.Version);
    }
}
