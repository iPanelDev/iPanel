using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using iPanel.Core.Models.Packets.Data;
using iPanel.Core.Models.Users;
using iPanel.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace iPanel.Core.Server.Api;

public partial class ApiMap
{
    [Route(HttpVerbs.Get, "/user/@self/status")]
    public async Task Status()
    {
        if (HttpContext.IsLogined())
        {
            await HttpContext.SendJsonAsync(
                new Status
                {
                    Logined = true,
                    SessionDuration = HttpContext.Session.Duration,
                    User = new SafeUser((HttpContext.Session[SessionKeyConstants.User] as User)!),
                },
                HttpStatusCode.OK
            );
            return;
        }

        await HttpContext.SendJsonAsync(new Status { Logined = false });
    }

    [Route(HttpVerbs.Post, "/user/@self/login")]
    public async Task Login()
    {
        if (HttpContext.IsLogined())
        {
            await HttpContext.SendJsonAsync(
                new Status
                {
                    Logined = true,
                    SessionDuration = HttpContext.Session.Duration,
                    User = new SafeUser((HttpContext.Session[SessionKeyConstants.User] as User)!),
                }
            );
            return;
        }

        var verifyBody =
            await HttpContext.ConvertRequestTo<VerifyBody>() ?? throw HttpException.BadRequest();

        if (string.IsNullOrEmpty(verifyBody.Time))
            throw HttpException.BadRequest();

        if (string.IsNullOrEmpty(verifyBody.UserName))
            throw HttpException.BadRequest();

        if (!DateTime.TryParse(verifyBody.Time, out DateTime dateTime))
            throw HttpException.BadRequest("\"Time\"无效");

        var span = dateTime - DateTime.Now;
        if (span.TotalSeconds < -10 || span.TotalMinutes > 10)
            throw HttpException.Forbidden("\"Time\"已过期");

        if (
            !_app.UserManager.Users.TryGetValue(verifyBody.UserName!, out User? user)
            || user.Level == PermissionLevel.Guest
        )
            throw HttpException.Forbidden("无效用户");

        if (
            verifyBody.MD5
            != Encryption.GetMD5($"{verifyBody.Time}.{verifyBody.UserName}.{user.Password}")
        )
            throw HttpException.Forbidden("验证失败");

        user.LastLoginTime = DateTime.Now;

        var address = HttpContext.RemoteEndPoint.Address.ToString();
        user.IPAddresses.Remove(address);
        user.IPAddresses.Insert(0, address);
        if (user.IPAddresses.Count > 10)
            user.IPAddresses.RemoveRange(10, user.IPAddresses.Count - 10);

        HttpContext.Session[SessionKeyConstants.User] = user;

        var token = UniqueIdGenerator.GetNext();
        HttpContext.Response.SetCookie(
            new(SessionKeyConstants.User, $"{verifyBody.UserName}_{token}", "/")
        );
        CookieManager.Cookies.Add($"{verifyBody.UserName}_{token}", DateTime.Now.AddHours(24));

        Logger.Info($"[{HttpContext.Id}] 验证成功");

        await HttpContext.SendJsonAsync(
            new Status
            {
                Logined = true,
                SessionDuration = HttpContext.Session.Duration,
                User = new SafeUser((HttpContext.Session[SessionKeyConstants.User] as User)!),
            }
        );
    }

    [Route(HttpVerbs.Get, "/user/@self/logout")]
    public async Task Logout()
    {
        HttpContext.EnsureLogined();
        HttpContext.Session.Delete();
        HttpContext.Response.SetCookie(new("user", "", "/") { Expired = true });
        await HttpContext.SendJsonAsync(null);
    }

    [Route(HttpVerbs.Get, "/user")]
    public async Task ListUsers()
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        await HttpContext.SendJsonAsync(
            _app.UserManager.Users
                .Select((kv) => new KeyValuePair<string, SafeUser>(kv.Key, new SafeUser(kv.Value)))
                .ToDictionary((kv) => kv.Key, (kv) => kv.Value)
        );
    }

    [Route(HttpVerbs.Get, "/user/@self")]
    public async Task GetUser()
    {
        HttpContext.EnsureLogined();

        await HttpContext.SendJsonAsync(
            new SafeUser((HttpContext.Session[SessionKeyConstants.User] as User)!)
        );
    }

    [Route(HttpVerbs.Get, "/user/{userName}")]
    public async Task GetUserInfo(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (!_app.UserManager.Users.TryGetValue(userName, out User? user))
            await HttpContext.SendJsonAsync("用户不存在", HttpStatusCode.NotFound);
        else
            await HttpContext.SendJsonAsync(new SafeUser(user));
    }

    [Route(HttpVerbs.Delete, "/user/{userName}")]
    public async Task DeleteUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (userName == "@self")
            throw HttpException.Forbidden();

        if (_app.UserManager.Remove(userName))
        {
            _app.UserManager.Save();
            await HttpContext.SendJsonAsync(null);
        }
        else
            await HttpContext.SendJsonAsync("用户不存在", HttpStatusCode.NotFound);
    }

    [Route(HttpVerbs.Post, "/user/{userName}")]
    public async Task CreateUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (userName == "@self")
            throw HttpException.Forbidden();

        if (_app.UserManager.Users.ContainsKey(userName))
        {
            await HttpContext.SendJsonAsync("用户已存在", HttpStatusCode.Conflict);
            return;
        }

        User? user = await HttpContext.ConvertRequestTo<User>();
        if (user is null || string.IsNullOrEmpty(user.Password))
        {
            throw HttpException.BadRequest("用户对象不正确");
        }

        _app.UserManager.Add(userName, user);
        _app.UserManager.Save();

        await HttpContext.SendJsonAsync(null);
    }

    [Route(HttpVerbs.Put, "/user/{userName}")]
    public async Task EditUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (userName == "@self")
            throw HttpException.Forbidden();

        if (!_app.UserManager.Users.TryGetValue(userName, out User? user))
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
        _app.UserManager.Save();

        await HttpContext.SendJsonAsync(null);
    }
}
