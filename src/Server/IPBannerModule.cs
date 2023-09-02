using EmbedIO.Security;

namespace iPanelHost.Server;

public class IPBannerModule : IPBanningModule
{
    public IPBannerModule()
        : base("/", Program.Setting.WebServer.WhiteList, Program.Setting.WebServer.BanMinutes)
    {
        this.WithMaxRequestsPerSecond(Program.Setting.WebServer.MaxRequestsPerSecond);
    }
}
