using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Service;
using iPanelHost.Utils;
using System;
using System.Text;
using System.Threading.Tasks;

namespace iPanelHost.Server;

public class Apis : WebApiController
{
    public static readonly Encoding UTF8 = new UTF8Encoding(false);

    /// <summary>
    /// 根目录
    /// </summary>
    [Route(HttpVerbs.Any, "/")]
    public void Root() => throw HttpException.Redirect("/");

    [Route(HttpVerbs.Any, "/ping")]
    public string Ping() => DateTime.Now.ToString("o");

    /// <summary>
    /// 上传文件
    /// </summary>
    [Route(HttpVerbs.Post, "/uploadSimplly")]
    public async Task SimpleUpload()
    {
        Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
        await FileTransferStation.SimpleUpload(HttpContext);
    }

    /// <summary>
    /// 上传文件流
    /// </summary>
    [Route(HttpVerbs.Post, "/upload")]
    public async Task StreamUpload()
    {
        Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
        await FileTransferStation.StreamUpload(HttpContext);
    }

    [Route(HttpVerbs.Any, "/generateCookie")]
    public async Task GenerateCookie()
    {
        HttpContext.Response.SetCookie(new("ipanel-token", Guid.NewGuid().ToString("N")));
        await HttpContext.SendStringAsync(new OperationResultPacket(null, ResultTypes.None).ToString(), "text/json", UTF8);
    }

    [Route(HttpVerbs.Any, "/login")]
    public string Login()
    {
        return HttpContext.Session.Id;
    }
}
