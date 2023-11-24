using EmbedIO;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Users;
using iPanel.Utils;
using iPanel.Utils.Json;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iPanel.Core.Server.Api;

public static class ApiHelper
{
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
        httpContext.Session.TryGetValue(SessionKeyConstants.User, out object? value)
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
            !httpContext.Session.TryGetValue(SessionKeyConstants.User, out object? value)
            || value is not User user
            || user.Level < permissionLevel
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
            httpContext.Session.TryGetValue(SessionKeyConstants.User, out object? value)
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

    private static async Task SendJsonAsync(
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
            EncodingsMap.UTF8
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
        var httpStatusCode = (HttpStatusCode)exception.StatusCode;
        switch (exception.StatusCode)
        {
            case < 500
            and >= 400:
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

            case 500: // InternalServerError
                await context.SendJsonAsync(
                    $"{exception.DataObject?.GetType()?.ToString() ?? "null"}:{exception.Message}",
                    httpStatusCode
                );
                break;
        }
    }

    public static async Task HandleException(IHttpContext context, Exception e)
    {
        if (!context.IsLogined())
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
