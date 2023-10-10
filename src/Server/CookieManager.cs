using EmbedIO;
using iPanelHost.Base;
using iPanelHost.Service;
using iPanelHost.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace iPanelHost.Server;

public class CookieManager : WebModuleBase
{
    public static readonly Dictionary<string, DateTime> Cookies = new();

    public CookieManager()
        : base("/api") { }

    public const string UserCookieKey = "user";

    public override bool IsFinalHandler => false;

    private static readonly Timer _timer = new(10000);

    static CookieManager()
    {
        _timer.Elapsed += (_, _) =>
        {
            lock (Cookies)
            {
                Cookies
                    .Where((kv) => kv.Value < DateTime.Now)
                    .ToList()
                    .ForEach((kv) => Cookies.Remove(kv.Key));
            }
        };
        _timer.Start();
    }

    protected override Task OnRequestAsync(IHttpContext httpContext)
    {
        string? userFlag = httpContext.Request.Cookies[UserCookieKey]?.Value;

        if (!string.IsNullOrEmpty(userFlag))
        {
            if (
                userFlag.Contains("_")
                && Cookies.TryGetValue(userFlag, out DateTime dateTime)
                && dateTime > DateTime.Now
            )
            {
                Update(userFlag);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新Cookies
    /// </summary>
    /// <param name="key">键</param>
    public static void Update(string key)
    {
        lock (Cookies)
        {
            if (Cookies.ContainsKey(key))
            {
                Cookies[key] = DateTime.Now.AddDays(7);
            }
        }
    }

    /// <summary>
    /// 查找用户Cookie
    /// </summary>
    /// <param name="httpContext">上下文</param>
    /// <returns>是否存在用户Cookie</returns>
    public static bool SearchUserCookie(IHttpContext httpContext)
    {
        string? userFlag = httpContext.Request.Cookies[UserCookieKey]?.Value;
        if (
            !string.IsNullOrEmpty(userFlag)
            && userFlag.Contains("_")
            && Cookies.TryGetValue(userFlag, out DateTime dateTime)
            && dateTime > DateTime.Now
        )
        {
            string? userName = userFlag.Split('_').FirstOrDefault();

            if (
                !string.IsNullOrEmpty(userName)
                && UserManager.Users.TryGetValue(userName, out User? user)
            )
            {
                httpContext.Session["user"] = user;
                httpContext.Session["uuid"] = userFlag.Split('_').LastOrDefault()!;
                Logger.Info($"[{httpContext.Id}] 已从Cookie中恢复登录状态");
                return true;
            }
        }

        return false;
    }
}
