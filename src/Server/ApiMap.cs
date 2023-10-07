using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Service;
using iPanelHost.Server.WebSocket.Handlers;
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
    /// <summary>
    /// 根目录
    /// </summary>
    [Route(HttpVerbs.Get, "/")]
    public async Task Root()
    {
        await HttpContext.SendJsonAsync(null, HttpStatusCode.OK);
    }

    /// <summary>
    /// 根目录
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

    /// <summary>
    /// 生成UUID
    /// </summary>
    [Route(HttpVerbs.Get, "/generateUUID")]
    public async Task GenerateUUID()
    {
        string uuid =
            HttpContext.IsLogined()
            && HttpContext.Session.TryGetValue(ApiHelper.UUIDKEY, out object outobj)
                ? outobj.ToString() ?? Guid.NewGuid().ToString("N")
                : Guid.NewGuid().ToString("N");

        HttpContext.Session[ApiHelper.UUIDKEY] = uuid;
        await HttpContext.SendJsonAsync(uuid);
    }

    #region 用户

    /// <summary>
    /// 登录
    /// </summary>
    [Route(HttpVerbs.Post, "/user/login")]
    public async Task Login()
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
                }
            );
            return;
        }

        VerifyBody verifyBody =
            await HttpContext.ConvertRequestTo<VerifyBody>() ?? throw HttpException.BadRequest();

        LoginGate.Verify(
            HttpContext,
            HttpContext.Session[ApiHelper.UUIDKEY]?.ToString()!,
            verifyBody
        );

        await HttpContext.SendJsonAsync(
            new Status
            {
                Logined = true,
                SessionDuration = HttpContext.Session.Duration,
                User = new SafeUser((HttpContext.Session[ApiHelper.USERKEY] as User)!),
                UUID = HttpContext.Session[ApiHelper.UUIDKEY]
            }
        );
    }

    /// <summary>
    /// 退出
    /// </summary>
    [Route(HttpVerbs.Get, "/user/logout")]
    public async Task Logout()
    {
        HttpContext.EnsureLogined();
        HttpContext.Session.Delete();
        await HttpContext.SendJsonAsync(null);
    }

    /// <summary>
    /// 列出所有用户
    /// </summary>
    [Route(HttpVerbs.Get, "/user/list")]
    public async Task ListUsers()
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        await HttpContext.SendJsonAsync(
            UserManager.Users
                .Select((kv) => new KeyValuePair<string, SafeUser>(kv.Key, new SafeUser(kv.Value)))
                .ToDictionary((kv) => kv.Key, (kv) => kv.Value)
        );
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [Route(HttpVerbs.Get, "/user/{userName?}")]
    public async Task GetCurrentUser(string? userName)
    {
        HttpContext.EnsureLogined();

        if (string.IsNullOrEmpty(userName))
        {
            await HttpContext.SendJsonAsync(
                new SafeUser((HttpContext.Session[ApiHelper.USERKEY] as User)!)
            );
            return;
        }

        HttpContext.EnsureLevel(PermissionLevel.Administrator);
        if (!UserManager.Users.TryGetValue(userName, out User? user))
        {
            await HttpContext.SendJsonAsync("用户不存在", HttpStatusCode.NotFound);
            return;
        }
        await HttpContext.SendJsonAsync(new SafeUser(user));
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [Route(HttpVerbs.Get, "/user/{userName}/delete")]
    public async Task DeleteUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (UserManager.Users.Remove(userName))
        {
            UserManager.Save();
            await HttpContext.SendJsonAsync(null);
        }
        else
        {
            await HttpContext.SendJsonAsync("用户不存在", HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [Route(HttpVerbs.Post, "/user/{userName}/create")]
    public async Task CreateUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (UserManager.Users.ContainsKey(userName))
        {
            await HttpContext.SendJsonAsync("用户已存在", HttpStatusCode.Conflict);
            return;
        }

        User? user = await HttpContext.ConvertRequestTo<User>();
        if (user is null || string.IsNullOrEmpty(user.Password))
        {
            throw HttpException.BadRequest("用户对象不正确");
        }

        UserManager.Users.Add(userName, user);
        UserManager.Save();

        await HttpContext.SendJsonAsync(null);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [Route(HttpVerbs.Post, "/user/{userName}/edit")]
    public async Task EditUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (!UserManager.Users.TryGetValue(userName, out User? user))
        {
            await HttpContext.SendJsonAsync("用户不存在", HttpStatusCode.NotFound);
            return;
        }

        User? newUser = await HttpContext.ConvertRequestTo<User>();
        if (newUser is null || string.IsNullOrEmpty(newUser.Password) || newUser.Instances is null)
        {
            throw HttpException.BadRequest("用户对象不正确");
        }

        user.Level = newUser.Level;
        user.Instances = newUser.Instances;
        user.Password = newUser.Password;
        user.Description = newUser.Description;
        UserManager.Save();

        await HttpContext.SendJsonAsync(null);
    }

    #endregion


    #region 实例

    /// <summary>
    /// 列出实例
    /// </summary>
    [Route(HttpVerbs.Get, "/instance/list")]
    public async Task ListInstances()
    {
        HttpContext.EnsureLogined();

        await HttpContext.SendJsonAsync(JArray.FromObject(MainHandler.Instances.Values));
    }

    /// <summary>
    /// 获取实例信息
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}")]
    public async Task GetInstanceInfo(string instanceId)
    {
        Instance? instance = MainHandler.Instances.Values.FirstOrDefault(
            (value) => value.InstanceID == instanceId
        );
        if (instance is not null)
        {
            await HttpContext.SendJsonAsync(instance);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 启动实例服务器
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/subscribe")]
    public async Task SubscribeInstance(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (MainHandler.Instances.Any((kv) => kv.Value.InstanceID == instanceId))
        {
            HttpContext.Session[ApiHelper.INSTANCEIDKEY] = instanceId;
            await HttpContext.SendJsonAsync(null, HttpStatusCode.OK);
        }
        else
        {
            HttpContext.Session[ApiHelper.INSTANCEIDKEY] = null!;
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 启动实例服务器
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/start")]
    public async Task CallInstanceStart(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        Instance? instance = MainHandler.Instances.Values.FirstOrDefault(
            (value) => value.InstanceID == instanceId
        );
        if (instance is not null)
        {
            instance?.Send(
                new SentPacket(
                    "request",
                    "server_start",
                    null,
                    sender: Sender.FromUser()
                ).ToString()
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 关闭实例服务器
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/stop")]
    public async Task CallInstanceStop(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        Instance? instance = MainHandler.Instances.Values.FirstOrDefault(
            (value) => value.InstanceID == instanceId
        );
        if (instance is not null)
        {
            instance?.Send(
                new SentPacket("request", "server_stop", null, sender: Sender.FromUser()).ToString()
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 强制结束实例服务器
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/kill")]
    public async Task CallInstanceKill(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        Instance? instance = MainHandler.Instances.Values.FirstOrDefault(
            (value) => value.InstanceID == instanceId
        );
        if (instance is not null)
        {
            instance?.Send(
                new SentPacket("request", "server_kill", null, sender: Sender.FromUser()).ToString()
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 向实例服务器输入
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Post, "/instance/{instanceId}/input")]
    public async Task CallInstanceInput(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        string[]? inputs = await HttpContext.ConvertRequestTo<string[]>();
        if (inputs is null || inputs.Length == 0)
        {
            throw HttpException.BadRequest("缺少输入数据");
        }

        Instance? instance = MainHandler.Instances.Values.FirstOrDefault(
            (value) => value.InstanceID == instanceId
        );
        if (instance is not null)
        {
            instance.Send(
                new SentPacket(
                    "request",
                    "server_input",
                    inputs,
                    sender: Sender.FromUser()
                ).ToString()
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    #endregion
}
