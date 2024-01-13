using System;
using System.Threading;
using System.Threading.Tasks;

using iPanel.Core.Interaction;
using iPanel.Core.Models.Settings;
using iPanel.Core.Server;
using iPanel.Core.Service;
using iPanel.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        Setting.Check();
        Logger.LogInformation("{}", Constant.Logo);

        SimpleLogger.StaticLogLevel = Setting.Debug
            ? Swan.Logging.LogLevel.Debug
            : Swan.Logging.LogLevel.Info;
        Console.CancelKeyPress += (_, _) => StopAsync();

        ResourceFileManager.Release();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        UserManager.Read();
        Logger.LogInformation("启动完毕");
        InputReader.Start(cancellationToken);
        HttpServer.Start(cancellationToken);

        Logger.LogInformation("讨论区/公告/Bug反馈：{}", "https://github.com/orgs/iPanelDev/discussions");
        Logger.LogInformation("文档：{}", "https://ipaneldev.github.io/");
        Logger.LogInformation("GitHub仓库：{}", "https://github.com/iPanelDev/iPanel");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        HttpServer.Dispose();
        _cancellationTokenSource.Cancel();
        Logger.LogInformation("Goodbye.");
        return Task.CompletedTask;
    }
}
