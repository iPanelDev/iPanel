using System.Threading.Tasks;

using EmbedIO;
using EmbedIO.Security;

using iPanel.Core.Models.Settings;
using iPanel.Core.Server.Api;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace iPanel.Core.Server;

public class IPBannerModule : IPBanningModule
{
    private readonly IHost _host;
    private Setting Setting => _host.Services.GetRequiredService<Setting>();

    public IPBannerModule(IHost host)
        : base("/")
    {
        _host = host;
        this.WithMaxRequestsPerSecond(Setting.WebServer.MaxRequestsPerSecond);
        this.WithWhitelist(Setting.WebServer.WhiteList);
        OnHttpException = Handle403;
    }

    public static async Task Handle403(IHttpContext context, IHttpException exception)
    {
        if (exception.StatusCode == 403 && context.RequestedPath.StartsWith("/api"))
        {
            await ApiHelper.HandleHttpException(context, exception);
        }
    }
}
