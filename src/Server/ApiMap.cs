using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Service;
using iPanelHost.Service.Handlers;
using iPanelHost.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace iPanelHost.Server;

public class ApiMap : WebApiController
{
    private const string UUIDKEY = "uuid";

    /// <summary>
    /// 根目录
    /// </summary>
    [Route(HttpVerbs.Get, "/")]
    public async Task Root()
    {
        await HttpContext.SendJsonAsync(null, HttpStatusCode.OK);
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
                    User = new SafeUser((HttpContext.Session["user"] as User)!),
                    UUID = HttpContext.Session[UUIDKEY]
                },
                HttpStatusCode.OK
            );
            return;
        }

        await HttpContext.SendJsonAsync(
            new Status { Logined = false, UUID = HttpContext.Session[UUIDKEY] }
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

    /// <summary>
    /// 生成UUID
    /// </summary>
    [Route(HttpVerbs.Get, "/generateUUID")]
    public async Task GenerateUUID()
    {
        string uuid =
            HttpContext.IsLogined() && HttpContext.Session.TryGetValue(UUIDKEY, out object outobj)
                ? outobj.ToString() ?? Guid.NewGuid().ToString("N")
                : Guid.NewGuid().ToString("N");

        HttpContext.Session[UUIDKEY] = uuid;
        await HttpContext.SendJsonAsync(uuid);
    }

    /// <summary>
    /// 登录
    /// </summary>
    [Route(HttpVerbs.Get, "/login")]
    [Route(HttpVerbs.Post, "/login")]
    public async Task Login()
    {
        if (HttpContext.IsLogined())
        {
            await HttpContext.SendJsonAsync(
                new Status
                {
                    Logined = true,
                    SessionDuration = HttpContext.Session.Duration,
                    User = new SafeUser((HttpContext.Session["user"] as User)!),
                    UUID = HttpContext.Session[UUIDKEY]
                }
            );
            return;
        }

        VerifyBody verifyBody =
            await HttpContext.ConvertRequsetTo<VerifyBody>() ?? throw HttpException.BadRequest();

        LoginGate.Verify(HttpContext, HttpContext.Session[UUIDKEY]?.ToString()!, verifyBody);

        await HttpContext.SendJsonAsync(
            new Status
            {
                Logined = true,
                SessionDuration = HttpContext.Session.Duration,
                User = new SafeUser((HttpContext.Session["user"] as User)!),
                UUID = HttpContext.Session[UUIDKEY]
            }
        );
    }

    #region 用户

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [Route(HttpVerbs.Get, "/user/current")]
    public async Task GetCurrentUser()
    {
        HttpContext.EnsureLogined();
        await HttpContext.SendJsonAsync(new SafeUser((HttpContext.Session["user"] as User)!));
    }

    /// <summary>
    /// 获取当前用户的UUID
    /// </summary>
    [Route(HttpVerbs.Get, "/user/current/uuid")]
    public async Task GetCurrentUserUUID()
    {
        HttpContext.EnsureLogined();
        await HttpContext.SendJsonAsync(HttpContext.Session[UUIDKEY]);
    }

    /// <summary>
    /// 列出所有用户
    /// </summary>
    [Route(HttpVerbs.Get, "/user/all")]
    public async Task ListUsers()
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        await HttpContext.SendJsonAsync(
            UserManager.Users
                .Select((kv) => new KeyValuePair<string, SafeUser>(kv.Key, new SafeUser(kv.Value)))
                .ToDictionary((kv) => kv.Key, (kv) => kv.Value)
        );
    }

    #endregion


    #region 实例

    [Route(HttpVerbs.Get, "/instance/all")]
    public async Task ListInstances()
    {
        HttpContext.EnsureLogined();

        await HttpContext.SendJsonAsync(JArray.FromObject(MainHandler.Instances.Values));
    }

    [Route(HttpVerbs.Get, "/instance/{instanceId}")]
    public async Task GetInstanceInfo(string instanceId)
    {
        Instance? instance = MainHandler.Instances.Values.FirstOrDefault(
            (value) => value.InstanceID == instanceId
        );
        if (instance is not null)
        {
            await HttpContext.SendJsonAsync(instance.FullInfo);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    [Route(HttpVerbs.Get, "/instance/{instanceId}/start")]
    public async Task CallInstanceStart(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
    }

    #endregion
}
