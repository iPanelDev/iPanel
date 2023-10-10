using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanelHost.Base;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Service;
using iPanelHost.Utils;
using System.Net;
using System.Threading.Tasks;

namespace iPanelHost.Server;

public partial class ApiMap : WebApiController
{
    /// <summary>
    /// 根目录
    /// </summary>
    [Route(HttpVerbs.Get, "/")]
    public async Task Root()
    {
        await HttpContext.SendJsonAsync(null, HttpStatusCode.OK);
    }

    /// <summary>
    /// 版本
    /// </summary>
    [Route(HttpVerbs.Get, "/version")]
    public async Task Version()
    {
        await HttpContext.SendJsonAsync(Constant.VERSION, HttpStatusCode.OK);
    }

    /// <summary>
    /// 当前状态
    /// </summary>
    [Route(HttpVerbs.Get, "/status")]
    public async Task Status()
    {
        if (HttpContext.IsLogined())
        {
            await HttpContext.SendJsonAsync(
                new Status
                {
                    Logined = true,
                    SessionDuration = HttpContext.Session.Duration,
                    User = new SafeUser((HttpContext.Session[ApiHelper.USERKEY] as User)!),
                    UUID = HttpContext.Session[ApiHelper.UUIDKEY]
                },
                HttpStatusCode.OK
            );
            return;
        }

        await HttpContext.SendJsonAsync(
            new Status { Logined = false, UUID = HttpContext.Session[ApiHelper.UUIDKEY] }
        );
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    [Route(HttpVerbs.Post, "/uploadSimplly")]
    public async Task SimpleUpload()
    {
        HttpContext.EnsureLogined();

        Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
        await FileTransferStation.SimpleUpload(HttpContext);
    }

    /// <summary>
    /// 上传文件流
    /// </summary>
    [Route(HttpVerbs.Post, "/upload")]
    public async Task StreamUpload()
    {
        HttpContext.EnsureLogined();

        Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
        await FileTransferStation.StreamUpload(HttpContext);
    }
}
