using iPanel.Core.Interaction;
using iPanel.Core.Models.Settings;
using iPanel.Core.Server;
using iPanel.Core.Server.WebSocket;
using iPanel.Core.Service;
using iPanel.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iPanel;

public class AppBuilder
{
    private IServiceCollection Services => _hostAppBuilder.Services;

    private readonly SimpleLoggerProvider _loggerProvider;
    private readonly HostApplicationBuilder _hostAppBuilder;

    public AppBuilder(Setting setting)
    {
        _loggerProvider = new();
        _hostAppBuilder = new HostApplicationBuilder();
        _hostAppBuilder.Logging.ClearProviders();
        _hostAppBuilder.Logging.AddProvider(_loggerProvider);

        Services.AddSingleton(setting);
        Services.AddSingleton<InputReader>();
        Services.AddSingleton<UserManager>();
        Services.AddSingleton<ResourceFileManager>();
        Services.AddSingleton<InstanceWsModule>();
        Services.AddSingleton<DebugWsModule>();
        Services.AddSingleton<IPBannerModule>();
        Services.AddSingleton<BroadcastWsModule>();
        Services.AddSingleton<HttpServer>();
    }

    public App Build() => new(_hostAppBuilder.Build());
}
