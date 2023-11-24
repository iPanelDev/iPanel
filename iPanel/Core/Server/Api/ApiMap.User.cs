using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Sessions;
using EmbedIO.Utilities;
using iPanel.Core.Models.Packets.Data;
using iPanel.Core.Models.Users;
using iPanel.Core.Service;
using iPanel.Utils;
using Microsoft.Extensions.Logging;
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
            !UserManager.Users.TryGetValue(verifyBody.UserName!, out User? user)
            || user.Level == PermissionLevel.Guest
        )
            throw HttpException.Forbidden("无效用户");

        if (
            verifyBody.MD5
            != Encryption.GetMD5($"{verifyBody.Time}.{verifyBody.UserName}.{user.Password}")
        )
            throw HttpException.Forbidden("用户名或密码错误");

        user.LastLoginTime = DateTime.Now;

        var address = HttpContext.RemoteEndPoint.Address.ToString();
        user.IPAddresses.Remove(address);
        user.IPAddresses.Insert(0, address);
        if (user.IPAddresses.Count > 10)
            user.IPAddresses.RemoveRange(10, user.IPAddresses.Count - 10);

        HttpContext.Session[SessionKeyConstants.User] = user;

        Logger.LogInformation("[{}] 登录成功", HttpContext.Id);

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
        Logger.LogInformation("[{}] 退出成功", HttpContext.Id);
    }

    [Route(HttpVerbs.Get, "/user")]
    public async Task ListUsers()
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        await HttpContext.SendJsonAsync(
            UserManager.Users
                .Select((kv) => new KeyValuePair<string, SafeUser>(kv.Key, new SafeUser(kv.Value)))
                .ToDictionary((kv) => kv.Key, (kv) => kv.Value)
        );
    }

    [Route(HttpVerbs.Get, "/user/@self")]
    public async Task GetUser()
    {
        await HttpContext.SendJsonAsync(new SafeUser(HttpContext.EnsureLogined()));
    }

    [Route(HttpVerbs.Get, "/user/{userName}")]
    public async Task GetUserInfo(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (!UserManager.Users.TryGetValue(userName, out User? user))
            await HttpContext.SendJsonAsync("用户不存在", HttpStatusCode.NotFound);
        else
            await HttpContext.SendJsonAsync(new SafeUser(user));
    }

    [Route(HttpVerbs.Delete, "/user/{userName}")]
    public async Task RemoveUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (userName == "@self")
            throw HttpException.Forbidden();

        if (UserManager.Remove(userName))
        {
            UserManager.Save();
            await HttpContext.SendJsonAsync(null);
            Logger.LogInformation("[{}] 删除用户{}成功", HttpContext.Id, userName);
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

        if (UserManager.Users.ContainsKey(userName))
        {
            await HttpContext.SendJsonAsync("用户已存在", HttpStatusCode.Conflict);
            return;
        }

        var user =
            await HttpContext.ConvertRequestTo<User>() ?? throw HttpException.BadRequest("用户对象为空");

        if (
            !UserManager.ValidateUserName(userName, out string? message)
            || !UserManager.ValidatePassword(user.Password, false, out message)
        )
            throw HttpException.BadRequest(message);

        UserManager.Add(userName, user);
        UserManager.Save();

        await HttpContext.SendJsonAsync(null);
        Logger.LogInformation("[{}] 创建用户{}成功", HttpContext.Id, userName);
    }

    [Route(HttpVerbs.Put, "/user/{userName}")]
    public async Task EditUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        var newUser =
            await HttpContext.ConvertRequestTo<User>() ?? throw HttpException.BadRequest("用户对象不正确");

        User? user;
        string? message;

        if (userName == "@self")
            if (
                HttpContext.Session.TryGetValue(SessionKeyConstants.User, out user)
                && user is not null
            )
            {
                if (!UserManager.ValidatePassword(newUser.Password, false, out message))
                    throw HttpException.BadRequest(message);

                user.Password = newUser.Password;
                await HttpContext.SendJsonAsync(null);
                UserManager.Save();
                Logger.LogInformation("[{}] 更新用户{}成功", HttpContext.Id, userName);
                return;
            }
            else
                throw new InvalidOperationException();

        if (!UserManager.Users.TryGetValue(userName, out user))
            throw HttpException.NotFound("用户不存在");

        user.Level = newUser.Level;
        user.Instances = newUser.Instances ?? user.Instances;

        if (!UserManager.ValidatePassword(newUser.Password, true, out message))
            throw HttpException.BadRequest(message);

        user.Password = newUser.Password ?? user.Password;
        user.Description = newUser.Description ?? user.Description;
        UserManager.Save();

        await HttpContext.SendJsonAsync(null);
        Logger.LogInformation("[{}] 更新用户{}成功", HttpContext.Id, userName);
    }
}
