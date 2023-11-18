using EmbedIO;
using EmbedIO.WebApi;
using iPanel.Core.Server.Api;
using iPanel.Core.Server.WebSocket;
using Swan.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iPanel.Core.Server;

public class HttpServer : IDisposable
{
    public readonly CookieManager CookieManager;
    public readonly InstanceWsModule InstanceWsModule;
    public readonly BroadcastWsModule BroadcastWsModule;
    public readonly IPBannerModule IPBannerModule;
    private readonly WebServer _server;
    private readonly App _app;

    public HttpServer(App app)
    {
        _app = app;
        _server = new(CreateOptions());
        CookieManager = new(_app);
        InstanceWsModule = new(_app);
        BroadcastWsModule = new(_app);
        IPBannerModule = new(_app);

        if (_app.Setting.WebServer.AllowCrossOrigin)
            _server.WithCors();

        if (_app.Setting.Debug)
            _server.WithModule(nameof(DebugWsModule), new DebugWsModule());

        _server.OnUnhandledException += HandleException;
        _server.WithLocalSessionManager((module) => module.CookieHttpOnly = false);
        _server.WithModule(nameof(IPBannerModule), IPBannerModule);
        _server.WithModule(nameof(InstanceWsModule), InstanceWsModule);
        _server.WithModule(nameof(BroadcastWsModule), BroadcastWsModule);
        _server.WithModule(nameof(CookieManager), CookieManager);
        _server.WithWebApi(
            "/api",
            (module) =>
                module
                    .WithController(() => new ApiMap(_app))
                    .HandleHttpException(ApiHelper.HandleHttpException)
                    .HandleUnhandledException(ApiHelper.HandleException)
        );

        if (Directory.Exists(_app.Setting.WebServer.Directory))
        {
            _server.WithStaticFolder(
                "/",
                _app.Setting.WebServer.Directory,
                _app.Setting.WebServer.DisableFilesHotUpdate
            );
            _server.HandleHttpException(HandleHttpException);
        }
        else
            Logger.Warn("静态网页目录不存在");
    }

    private static async Task HandleException(IHttpContext httpContext, Exception e)
    {
        Logger.Fatal(e, string.Empty, $"[{httpContext.Id}]");
        await Task.CompletedTask;
    }

    private WebServerOptions CreateOptions()
    {
        WebServerOptions options = new();

        try
        {
            _app.Setting.WebServer.UrlPrefixes.ToList().ForEach((url) => options.AddUrlPrefix(url));
        }
        catch (Exception e)
        {
            Logger.Error(e, nameof(HttpServer), string.Empty);
        }

        if (_app.Setting.WebServer.Certificate.Enable)
        {
            options.AutoLoadCertificate = _app.Setting.WebServer.Certificate.AutoLoadCertificate;
            options.AutoRegisterCertificate = _app.Setting
                .WebServer
                .Certificate
                .AutoRegisterCertificate;

            if (string.IsNullOrEmpty(_app.Setting.WebServer.Certificate.Path))
                return options;

            if (File.Exists(_app.Setting.WebServer.Certificate.Path))
                options.Certificate = string.IsNullOrEmpty(
                    _app.Setting.WebServer.Certificate.Password
                )
                    ? new(_app.Setting.WebServer.Certificate.Path!)
                    : new(
                        _app.Setting.WebServer.Certificate.Path!,
                        _app.Setting.WebServer.Certificate.Password
                    );
            else
                Logger.Warn($"“{_app.Setting.WebServer.Certificate.Path}”不存在");
        }

        return options;
    }

    private async Task HandleHttpException(IHttpContext context, IHttpException exception)
    {
        if (exception.StatusCode == 404)
        {
            if (_app.Setting.WebServer.DisableFilesHotUpdate)
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
                await context.SendStringAsync(_404HtmlContent!, "text/html", Encoding.UTF8);
                Logger.Info(
                    $"[{context.Id}] {context.Request.HttpMethod} {context.RequestedPath}: 404 -> 200"
                );
                return;
            }
        }
        context.Response.StatusCode = exception.StatusCode;
        await context.SendStandardHtmlAsync(exception.StatusCode);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _server.RunAsync(cancellationToken);
    }

    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    private string? _404HtmlPath =>
        Path.Combine(_app.Setting.WebServer.Directory, _app.Setting.WebServer.Page404);

    private string? _404HtmlContent;
}
