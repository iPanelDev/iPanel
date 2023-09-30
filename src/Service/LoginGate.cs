using System;
using System.Collections.Generic;
using System.Linq;
using EmbedIO;
using iPanelHost.Base;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Server;
using iPanelHost.Utils;

namespace iPanelHost.Service;

public static class LoginGate
{
    public const string UserCookieKey = "user";

    /// <summary>
    /// 验证控制台
    /// </summary>
    /// <returns>验证结果</returns>
    public static void Verify(IHttpContext httpContext, string uuid, VerifyBody verifyBody)
    {
        string clientUrl = httpContext.RemoteEndPoint.ToString();

        if (string.IsNullOrEmpty(verifyBody.UserName))
        {
            throw HttpException.BadRequest();
        }

        if (
            !UserManager.Users.TryGetValue(verifyBody.UserName!, out User? user)
            || verifyBody.Token != General.GetMD5(uuid + verifyBody.UserName! + user.Password)
        )
        {
            Logger.Info($"<{clientUrl}> 验证失败");
            throw HttpException.Forbidden("验证失败");
        }

        user.LastLoginTime = DateTime.Now;

        user.IPAddresses.Insert(0, clientUrl);
        if (user.IPAddresses.Count > 10)
        {
            user.IPAddresses.RemoveRange(10, user.IPAddresses.Count - 10);
        }

        httpContext.Session["user"] = user;
        httpContext.Response.SetCookie(new(UserCookieKey, $"{verifyBody.UserName}_{uuid}"));
        CookieManager.Cookies.Add($"{verifyBody.UserName}_{uuid}", DateTime.Now);

        Logger.Info($"[{httpContext.Id}] 验证成功");
    }
}
