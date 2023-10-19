using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using iPanelHost.Base;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Service;
using iPanelHost.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace iPanelHost.Server;

public partial class ApiMap
{
    /// <summary>
    /// 生成UUID
    /// </summary>
    [Route(HttpVerbs.Get, "/user/newUuid")]
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

        string? uuid = HttpContext.Session[ApiHelper.UUIDKEY]?.ToString();

        if (string.IsNullOrEmpty(uuid))
        {
            throw HttpException.InternalServerError();
        }
        if (string.IsNullOrEmpty(verifyBody.UserName))
        {
            throw HttpException.BadRequest();
        }
        if (
            !UserManager.Users.TryGetValue(verifyBody.UserName!, out User? user)
            || verifyBody.Token != General.GetMD5(uuid + verifyBody.UserName! + user.Password)
        )
        {
            throw HttpException.Forbidden("验证失败");
        }
        if (user.Level == PermissionLevel.Guest)
        {
            throw HttpException.Forbidden("无效用户");
        }

        user.LastLoginTime = DateTime.Now;
        user.IPAddresses.Insert(0, HttpContext.RemoteEndPoint.Address.ToString());
        if (user.IPAddresses.Count > 10)
        {
            user.IPAddresses.RemoveRange(10, user.IPAddresses.Count - 10);
        }

        HttpContext.Session["user"] = user;

        string token = UniqueIdGenerator.GetNext();
        HttpContext.Response.SetCookie(
            new(ApiHelper.USERKEY, $"{verifyBody.UserName}_{token}", "/")
        );
        CookieManager.Cookies.Add($"{verifyBody.UserName}_{token}", DateTime.Now.AddHours(24));

        Logger.Info($"[{HttpContext.Id}] 验证成功");

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
        HttpContext.Response.SetCookie(new("user", "", "/") { Expired = true });
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
    [Route(HttpVerbs.Get, "/user")]
    public async Task GetUser()
    {
        HttpContext.EnsureLogined();

        await HttpContext.SendJsonAsync(
            new SafeUser((HttpContext.Session[ApiHelper.USERKEY] as User)!)
        );
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    [Route(HttpVerbs.Get, "/user/{userName?}/info")]
    public async Task GetUserInfo(string userName)
    {
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
        if (newUser is null || string.IsNullOrEmpty(newUser.Password))
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
}
