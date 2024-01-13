using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbedIO;
using EmbedIO.Sessions;
using EmbedIO.WebApi;

using iPanel.Core.Models.Settings;
using iPanel.Core.Server.Api;
using iPanel.Core.Server.WebSocket;
using iPanel.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iPanel.Core.Server;

public class HttpServer : IDisposable
{
    private readonly IHost _host;
    private IServiceProvider Services => _host.Services;
    private ILogger<HttpServer> Logger => Services.GetRequiredService<ILogger<HttpServer>>();
    private InstanceWsModule InstanceWsModule => Services.GetRequiredService<InstanceWsModule>();
    private BroadcastWsModule BroadcastWsModule => Services.GetRequiredService<BroadcastWsModule>();
    private IPBannerModule IPBannerModule => Services.GetRequiredService<IPBannerModule>();
    private DebugWsModule DebugWsModule => Services.GetRequiredService<DebugWsModule>();
    private Setting Setting => Services.GetRequiredService<Setting>();
    private readonly WebServer _server;

    public HttpServer(IHost host)
    {
        _host = host;
        _server = new(CreateOptions());

        if (Setting.WebServer.AllowCrossOrigin)
            _server.WithCors();

        if (Setting.Debug)
            _server.WithModule(nameof(DebugWsModule), DebugWsModule);

        _server.OnUnhandledException += HandleException;
        _server.WithLocalSessionManager(ConfigureLocalSessionManager);
        _server.WithModule(nameof(IPBannerModule), IPBannerModule);
        _server.WithModule(nameof(InstanceWsModule), InstanceWsModule);
        _server.WithModule(nameof(BroadcastWsModule), BroadcastWsModule);
        _server.WithWebApi(
            "/api",
            (module) =>
                module
                    .WithController(() => new ApiMap(_host))
                    .HandleHttpException(ApiHelper.HandleHttpException)
                    .HandleUnhandledException(ApiHelper.HandleException)
        );

        if (Directory.Exists(Setting.WebServer.Directory))
        {
            _server.WithStaticFolder(
                "/",
                Setting.WebServer.Directory,
                Setting.WebServer.DisableFilesHotUpdate
            );
            _server.HandleHttpException(HandleHttpException);
        }
        else
            Logger.LogWarning("静态网页目录不存在");
    }

    private static void ConfigureLocalSessionManager(LocalSessionManager localSessionManager)
    {
        localSessionManager.CookieHttpOnly = false;
        localSessionManager.SessionDuration = TimeSpan.FromHours(1);
    }

    private async Task HandleException(IHttpContext httpContext, Exception e)
    {
        Logger.LogCritical(e, "[{}]", httpContext.Id);
        await Task.CompletedTask;
    }

    private WebServerOptions CreateOptions()
    {
        WebServerOptions options = new();

        try
        {
            Setting.WebServer.UrlPrefixes.ToList().ForEach((url) => options.AddUrlPrefix(url));
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
        }

        if (Setting.WebServer.Certificate.Enable)
        {
            options.AutoLoadCertificate = Setting.WebServer.Certificate.AutoLoadCertificate;
            options.AutoRegisterCertificate = Setting.WebServer.Certificate.AutoRegisterCertificate;

            if (string.IsNullOrEmpty(Setting.WebServer.Certificate.Path))
                return options;

            if (File.Exists(Setting.WebServer.Certificate.Path))
                options.Certificate = string.IsNullOrEmpty(Setting.WebServer.Certificate.Password)
                    ? new(Setting.WebServer.Certificate.Path!)
                    : new(
                        Setting.WebServer.Certificate.Path!,
                        Setting.WebServer.Certificate.Password
                    );
            else
                Logger.LogWarning("证书文件“{}”不存在", Setting.WebServer.Certificate.Path);
        }

        return options;
    }

    private async Task HandleHttpException(IHttpContext context, IHttpException exception)
    {
        if (exception.StatusCode == 404)
        {
            if (Setting.WebServer.DisableFilesHotUpdate)
                _404HtmlContent ??= File.Exists(_404HtmlPath)
                    ? File.ReadAllText(_404HtmlPath)
                    : null;
            else
                _404HtmlContent = File.Exists(_404HtmlPath)
                    ? File.ReadAllText(_404HtmlPath)
                    : _404HtmlContent;

            if (!string.IsNullOrEmpty(_404HtmlContent))
            {
                context.Response.StatusCode = 200;
                await context.SendStringAsync(_404HtmlContent!, "text/html", EncodingsMap.UTF8);
                Logger.LogInformation(
                    "[{}] {} {}: 404 -> 200",
                    context.Id,
                    context.Request.HttpMethod,
                    context.RequestedPath
                );
                return;
            }
        }
        context.Response.StatusCode = exception.StatusCode;
        await context.SendStandardHtmlAsync(exception.StatusCode);
    }

    public void Start(CancellationToken cancellationToken)
    {
        _server.Start(cancellationToken);
    }

    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    private string? _404HtmlPath =>
        Path.Combine(Setting.WebServer.Directory, Setting.WebServer.Page404);

    private string? _404HtmlContent;
}
