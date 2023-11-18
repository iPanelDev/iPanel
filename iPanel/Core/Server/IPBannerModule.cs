using EmbedIO;
using EmbedIO.Security;
using iPanel.Core.Server.Api;
using System.Threading.Tasks;

namespace iPanel.Core.Server;

public class IPBannerModule : IPBanningModule
{
    public IPBannerModule(App app)
        : base("/", app.Setting.WebServer.WhiteList, app.Setting.WebServer.BanMinutes)
    {
        this.WithMaxRequestsPerSecond(app.Setting.WebServer.MaxRequestsPerSecond);
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
