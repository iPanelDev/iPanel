using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Sessions;
using EmbedIO.Utilities;

using iPanel.Core.Models.Packets.Data;
using iPanel.Core.Models.Users;
using iPanel.Core.Service;
using iPanel.Utils;

using Microsoft.Extensions.Logging;

namespace iPanel.Core.Server.Api;

public partial class ApiMap
{
    [Route(HttpVerbs.Get, "/users/@self/status")]
    public async Task Status()
    {
        if (HttpContext.IsLogined())
        {
            await HttpContext.SendPacketAsync(
                new Status
                {
                    Logined = true,
                    SessionDuration = HttpContext.Session.Duration,
                    User = (HttpContext.Session[SessionKeyConstants.User] as User)!,
                }
            );
            return;
        }

        await HttpContext.SendPacketAsync(new Status { Logined = false });
    }

    [Route(HttpVerbs.Post, "/users/@self/login")]
    public async Task Login()
    {
        if (HttpContext.IsLogined())
        {
            await HttpContext.SendPacketAsync(
                new Status
                {
                    Logined = true,
                    SessionDuration = HttpContext.Session.Duration,
                    User = (HttpContext.Session[SessionKeyConstants.User] as User)!,
                }
            );
            return;
        }

        var verifyBody =
            await HttpContext.ConvertRequestTo<VerifyBody>() ?? throw HttpException.BadRequest();

        if (string.IsNullOrEmpty(verifyBody.UserName))
            throw HttpException.BadRequest("用户名为空");

        if (
            string.IsNullOrEmpty(verifyBody.Time)
            || !DateTime.TryParse(verifyBody.Time, out DateTime dateTime)
        )
            throw HttpException.BadRequest("\"time\"无效");

        var span = dateTime - DateTime.Now;
        if (span.TotalSeconds < -10 || span.TotalMinutes > 10)
            throw HttpException.BadRequest("\"time\"已过期");

        if (
            !UserManager.Users.TryGetValue(verifyBody.UserName!, out User? user)
            || verifyBody.MD5
                != Encryption.GetMD5($"{verifyBody.Time}.{verifyBody.UserName}.{user.Password}")
        )
            throw HttpException.Forbidden("用户名或密码错误");

        if (user.Level == PermissionLevel.Guest)
            throw HttpException.Forbidden("用户无效");

        user.LastLoginTime = DateTime.Now;

        var address = HttpContext.RemoteEndPoint.Address.ToString();
        user.IPAddresses.Remove(address);
        user.IPAddresses.Insert(0, address);
        if (user.IPAddresses.Count > 10)
            user.IPAddresses.RemoveRange(10, user.IPAddresses.Count - 10);

        HttpContext.Session[SessionKeyConstants.User] = user;
        HttpContext.Session[SessionKeyConstants.UserName] = verifyBody.UserName;

        Logger.LogInformation("[{}] 登录成功", HttpContext.Id);

        await HttpContext.SendPacketAsync(
            new Status
            {
                Logined = true,
                SessionDuration = HttpContext.Session.Duration,
                User = (HttpContext.Session[SessionKeyConstants.User] as User)!,
            }
        );
    }

    [Route(HttpVerbs.Get, "/users/@self/logout")]
    public async Task Logout()
    {
        HttpContext.EnsureLogined();
        HttpContext.Session.Delete();
        await HttpContext.SendPacketAsync();
        Logger.LogInformation("[{}] 退出成功", HttpContext.Id);
    }

    [Route(HttpVerbs.Get, "/users")]
    public async Task ListUsers()
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        await HttpContext.SendPacketAsync(
            UserManager.Users
                .Select((kv) => new KeyValuePair<string, UserWithoutPwd>(kv.Key, kv.Value))
                .ToDictionary((kv) => kv.Key, (kv) => kv.Value)
        );
    }

    [Route(HttpVerbs.Get, "/users/@self")]
    public async Task GetUser()
    {
        await HttpContext.SendPacketAsync(HttpContext.EnsureLogined());
    }

    [Route(HttpVerbs.Get, "/users/{userName}")]
    public async Task GetUserInfo(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (!UserManager.Users.TryGetValue(userName, out User? user))
            throw HttpException.NotFound("用户不存在");
        else
            await HttpContext.SendPacketAsync(user as UserWithoutPwd);
    }

    [Route(HttpVerbs.Delete, "/users/{userName}")]
    public async Task RemoveUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (
            userName == "@self"
            || userName == HttpContext.Session[SessionKeyConstants.UserName]?.ToString()
        )
            throw HttpException.Forbidden("不能删除自己");

        if (UserManager.Remove(userName))
        {
            UserManager.Save();
            await HttpContext.SendPacketAsync();
            Logger.LogInformation("[{}] 删除用户{}成功", HttpContext.Id, userName);
        }
        else
            throw HttpException.NotFound("用户不存在");
    }

    [Route(HttpVerbs.Post, "/users/{userName}")]
    public async Task CreateUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        if (
            userName == "@self"
            || userName == HttpContext.Session[SessionKeyConstants.UserName]?.ToString()
        )
            throw HttpException.Forbidden("不能创建自己");

        if (UserManager.Users.ContainsKey(userName))
            throw new HttpException(HttpStatusCode.Conflict, "用户已存在");

        var user =
            await HttpContext.ConvertRequestTo<User>() ?? throw HttpException.BadRequest("用户对象为空");

        if (
            !UserManager.ValidateUserName(userName, out string? message)
            || !UserManager.ValidatePassword(user.Password, false, out message)
        )
            throw HttpException.BadRequest(message);

        UserManager.Add(userName, user);
        UserManager.Save();

        await HttpContext.SendPacketAsync();
        Logger.LogInformation("[{}] 创建用户{}成功", HttpContext.Id, userName);
    }

    [Route(HttpVerbs.Put, "/users/{userName}")]
    public async Task EditUser(string userName)
    {
        HttpContext.EnsureLevel(PermissionLevel.Administrator);

        var newUser =
            await HttpContext.ConvertRequestTo<User>() ?? throw HttpException.BadRequest("用户对象不正确");

        User? user;
        string? message;

        if (
            userName == "@self"
            || userName == HttpContext.Session[SessionKeyConstants.UserName]?.ToString()
        )
            if (
                HttpContext.Session.TryGetValue(SessionKeyConstants.User, out user)
                && user is not null
            )
            {
                if (!UserManager.ValidatePassword(newUser.Password, false, out message))
                    throw HttpException.BadRequest(message);

                user.Password = newUser.Password;
                await HttpContext.SendPacketAsync();
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

        await HttpContext.SendPacketAsync();
        Logger.LogInformation("[{}] 更新用户{}成功", HttpContext.Id, userName);
    }
}
