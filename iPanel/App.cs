using iPanel.Core.Interaction;
using iPanel.Core.Models.Settings;
using iPanel.Core.Server;
using iPanel.Core.Service;
using iPanel.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iPanel;

public class App : IHost
{
    private readonly IHost _host;
    public IServiceProvider Services => _host.Services;
    private ILogger<App> Logger => Services.GetRequiredService<ILogger<App>>();
    private HttpServer HttpServer => Services.GetRequiredService<HttpServer>();
    private Setting Setting => Services.GetRequiredService<Setting>();
    private UserManager UserManager => Services.GetRequiredService<UserManager>();
    private InputReader InputReader => Services.GetRequiredService<InputReader>();
    private ResourceFileManager ResourceFileManager =>
        Services.GetRequiredService<ResourceFileManager>();
    private readonly CancellationTokenSource _cancellationTokenSource;

    public App(IHost host)
    {
        _host = host;
        _cancellationTokenSource = new();

        SimpleLogger.StaticLogLevel = Setting.Debug
            ? Swan.Logging.LogLevel.Debug
            : Swan.Logging.LogLevel.Info;
        Console.CancelKeyPress += (_, _) => Dispose();
        ResourceFileManager.Release();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        UserManager.Read();
        Logger.LogInformation("启动完毕");
        InputReader.Start(cancellationToken);
        await HttpServer.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        HttpServer.Dispose();
        _cancellationTokenSource.Cancel();
        Logger.LogInformation("Goodbye.");
        return Task.CompletedTask;
    }
}
