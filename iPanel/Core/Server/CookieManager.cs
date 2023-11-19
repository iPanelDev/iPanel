using EmbedIO;
using iPanel.Core.Models.Users;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace iPanel.Core.Server;

#pragma warning disable CA1847

public class CookieManager : WebModuleBase
{
    private readonly App _app;
    public static readonly Dictionary<string, DateTime> Cookies = new();

    public CookieManager(App app)
        : base("/api")
    {
        _app = app;
    }

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

    public bool SearchUserCookie(IHttpContext httpContext)
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
                && _app.UserManager.Users.TryGetValue(userName, out User? user)
            )
            {
                httpContext.Session[SessionKeyConstants.User] = user!;
                httpContext.Session[SessionKeyConstants.UUID] = userFlag
                    .Split('_')
                    .LastOrDefault()!;
                Logger.Info($"[{httpContext.Id}] 已从Cookie中恢复登录状态");
                return true;
            }
        }

        return false;
    }
}
