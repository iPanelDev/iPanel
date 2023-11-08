using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Security;

namespace iPanelHost.Server;

public class IPBannerModule : IPBanningModule
{
    public IPBannerModule()
        : base("/", Program.Setting.WebServer.WhiteList, Program.Setting.WebServer.BanMinutes)
    {
        this.WithMaxRequestsPerSecond(Program.Setting.WebServer.MaxRequestsPerSecond);
        OnHttpException = Handle403;
    }

    /// <summary>
    /// 处理403页面
    /// </summary>
    /// <param name="context">上下文</param>
    /// <param name="exception">异常对象</param>
    public static async Task Handle403(IHttpContext context, IHttpException exception)
    {
        if (exception.StatusCode == 403 && context.RequestedPath.StartsWith("/api"))
        {
            await ApiHelper.HandleHttpException(context, exception);
        }
    }
}
