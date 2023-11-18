using EmbedIO;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Users;
using iPanel.Utils.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iPanel.Core.Server.Api;

#pragma warning disable CA1822

public static class ApiHelper
{
    public static readonly Encoding UTF8 = new UTF8Encoding(false);

    public static async Task<T?> ConvertRequestTo<T>(this IHttpContext httpContext)
        where T : notnull
    {
        if (httpContext.Request.HttpVerb != HttpVerbs.Get)
        {
            if (httpContext.Request.ContentType != "application/json")
                throw HttpException.BadRequest("不支持的\"ContentType\"");

            return JsonSerializer.Deserialize<T>(
                await httpContext.GetRequestBodyAsStringAsync(),
                JsonSerializerOptionsFactory.CamelCase
            );
        }

        var jsonObject = new JsonObject();
        var queryData = httpContext.GetRequestQueryData();
        foreach (string? name in queryData.AllKeys)
        {
            if (string.IsNullOrEmpty(name) || jsonObject.ContainsKey(name))
            {
                continue;
            }
            jsonObject.Add(name, queryData[name]);
        }

        return JsonSerializer.Deserialize<T>(jsonObject, JsonSerializerOptionsFactory.CamelCase);
    }

    public static bool IsLogined(this IHttpContext httpContext) =>
        httpContext.Session.TryGetValue("user", out object? value)
        && value is User user
        && user is not null
        && user.Level != PermissionLevel.Guest;

    public static User EnsureLogined(this IHttpContext httpContext)
    {
        if (!httpContext.IsLogined())
            throw HttpException.Unauthorized();

        return (httpContext.Session[SessionKeyConstants.User] as User)!;
    }

    public static void EnsureLevel(this IHttpContext httpContext, PermissionLevel permissionLevel)
    {
        if (
            !httpContext.Session.TryGetValue("user", out object? value)
            || value is not User user
            || (int)user.Level < (int)permissionLevel
        )
            throw HttpException.Forbidden("权限不足");
    }

    public static void EnsureAccess(
        this IHttpContext httpContext,
        string instanceId,
        bool strict = true
    )
    {
        if (
            httpContext.Session.TryGetValue("user", out object? value)
            && value is User user
            && (
                user.Level == PermissionLevel.Administrator
                || user.Instances.Contains(instanceId) && user.Level == PermissionLevel.Assistant
                || user.Instances.Contains(instanceId)
                    && user.Level == PermissionLevel.ReadOnly
                    && !strict
            )
        )
        {
            return;
        }
        throw HttpException.Forbidden("权限不足");
    }

    public static async Task SendJsonAsync(
        this IHttpContext httpContext,
        object? data,
        int statusCode
    )
    {
        httpContext.Response.StatusCode = statusCode;
        await httpContext.SendStringAsync(
            JsonSerializer.Serialize(
                new ApiPacket { Data = data, Code = statusCode },
                JsonSerializerOptionsFactory.CamelCase
            ),
            "text/json",
            UTF8
        );

        httpContext.SetHandled();
    }

    public static async Task SendJsonAsync(
        this IHttpContext httpContext,
        object? data,
        HttpStatusCode statusCode = HttpStatusCode.OK
    ) => await SendJsonAsync(httpContext, data, (int)statusCode);

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
                            @"^[A-Z]",
                            (c) => c.Value.ToLower()
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
