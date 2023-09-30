using EmbedIO;
using iPanelHost.Base;
using iPanelHost.Base.Packets;
using iPanelHost.Service;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iPanelHost.Server;

#pragma warning disable CA1822

public static class ApiHelper
{
    public static readonly Encoding UTF8 = new UTF8Encoding(false);

    /// <summary>
    /// 将请求内容转为对象
    /// </summary>
    /// <typeparam name="T">要转化的类型</typeparam>
    public static async Task<T?> ConvertRequsetTo<T>(this IHttpContext httpContext)
    {
        try
        {
            if (httpContext.Request.HttpVerb != HttpVerbs.Get)
            {
                return JsonConvert.DeserializeObject<T>(
                    await httpContext.GetRequestBodyAsStringAsync()
                );
            }

            JObject jObject = new();
            NameValueCollection queryData = httpContext.GetRequestQueryData();
            foreach (string? name in queryData.AllKeys)
            {
                if (string.IsNullOrEmpty(name) || jObject.ContainsKey(name))
                {
                    continue;
                }
                jObject.Add(name, queryData[name]);
            }

            return jObject.ToObject<T>();
        }
        catch (Exception e)
        {
            Logger.Warn($"[{httpContext.Id}] 尝试转化Body失败: {e.Message}");
            return default;
        }
    }

    /// <summary>
    /// 是否已经登录
    /// </summary>
    public static bool IsLogined(this IHttpContext httpContext)
    {
        if (
            httpContext.Session.TryGetValue("user", out object? value)
            && value is User user
            && user is not null
            && user.Level != PermissionLevel.Guest
        )
        {
            return true;
        }

        return CookieManager.SearchUserCookie(httpContext);
    }

    /// <summary>
    /// 确保已登录
    /// </summary>
    public static void EnsureLogined(this IHttpContext httpContext)
    {
        if (!httpContext.IsLogined())
        {
            throw HttpException.Unauthorized();
        }
    }

    /// <summary>
    /// 确保权限等级
    /// </summary>
    public static void EnsureLevel(this IHttpContext httpContext, PermissionLevel permissionLevel)
    {
        if (
            httpContext.Session.TryGetValue("user", out object? value)
            && value is User user
            && (int)user.Level >= (int)permissionLevel
        )
        {
            return;
        }
        throw HttpException.Forbidden("Permission_denied");
    }

    /// <summary>
    /// 确保权限等级
    /// </summary>
    public static void EnsureAccess(this IHttpContext httpContext, string instanceId)
    {
        if (
            httpContext.Session.TryGetValue("user", out object? value)
            && value is User user
            && (
                user.Level == PermissionLevel.Administrator
                || user.Level == PermissionLevel.Assistant && user.Instances.Contains(instanceId)
            )
        )
        {
            return;
        }
        throw HttpException.Forbidden("Permission_denied");
    }

    /// <summary>
    /// 发送Json
    /// </summary>
    /// <param name="data">数据字段</param>
    /// <param name="statusCode">状态码</param>
    public static async Task SendJsonAsync(
        this IHttpContext httpContext,
        object? data,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
    {
        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.SendStringAsync(
            JsonConvert.SerializeObject(new SimplePacket { Data = data, Code = (int)statusCode }),
            "text/json",
            UTF8
        );

        httpContext.SetHandled();
    }

    /// <summary>
    /// 处理Http异常
    /// </summary>
    /// <param name="context">上下文</param>
    /// <param name="exception">异常对象</param>
    public static async Task HandleHttpException(IHttpContext context, IHttpException exception)
    {
        HttpStatusCode httpStatusCode = (HttpStatusCode)exception.StatusCode;
        switch (httpStatusCode)
        {
            case HttpStatusCode.BadRequest: // 400
            case HttpStatusCode.Unauthorized: // 401
            case HttpStatusCode.Forbidden: // 403
            case HttpStatusCode.NotFound: // 404
            case HttpStatusCode.MethodNotAllowed: // 405
                await context.SendJsonAsync(
                    exception.Message
                        ?? Regex.Replace(
                            httpStatusCode.ToString(),
                            @"(?<=\w)[A-Z]",
                            (c) => "_" + c.Value.ToLower()
                        ),
                    httpStatusCode
                );
                break;

            case HttpStatusCode.InternalServerError: // 500
                await context.SendJsonAsync(
                    $"{exception.DataObject?.GetType()?.ToString() ?? "null"}:{exception.Message}",
                    httpStatusCode
                );
                break;
        }
    }

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="context">上下文</param>
    /// <param name="e">异常对象</param>
    public static async Task HandleException(IHttpContext context, Exception e)
    {
        if (context.IsLogined())
        {
            await context.SendJsonAsync(
                $"{e.GetType()}{e.Message}",
                HttpStatusCode.InternalServerError
            );
            return;
        }
        await context.SendJsonAsync($"{e.GetType()}", HttpStatusCode.InternalServerError);
    }
}
