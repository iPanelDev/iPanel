using EmbedIO;
using EmbedIO.Sessions;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Users;
using iPanel.Utils;
using iPanel.Utils.Json;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iPanel.Core.Server.Api;

public static class ApiHelper
{
    public static async Task<T?> ConvertRequestTo<T>(this IHttpContext httpContext)
        where T : notnull
    {
        if (httpContext.Request.HttpVerb is not HttpVerbs.Get or HttpVerbs.Head)
        {
            if (httpContext.Request.ContentType != "application/json")
                throw HttpException.BadRequest("不支持的\"ContentType\"");

            try
            {
                return JsonSerializer.Deserialize<T>(
                    await httpContext.GetRequestBodyAsStringAsync(),
                    JsonSerializerOptionsFactory.CamelCase
                );
            }
            catch (Exception e)
            {
                throw HttpException.BadRequest(e.Message);
            }
        }

        throw HttpException.MethodNotAllowed();
    }

    public static bool IsLogined(this IHttpContext httpContext) =>
        httpContext.Session.TryGetValue(SessionKeyConstants.User, out object? value)
        && value is User user
        && user is not null
        && user.Level != PermissionLevel.Guest;

    public static UserWithoutPwd EnsureLogined(this IHttpContext httpContext)
    {
        if (!httpContext.IsLogined())
            throw HttpException.Unauthorized();

        return (httpContext.Session[SessionKeyConstants.User] as User)!;
    }

    public static void EnsureLevel(this IHttpContext httpContext, PermissionLevel permissionLevel)
    {
        if (
            !httpContext.Session.TryGetValue(SessionKeyConstants.User, out User? user)
            || user is null
            || user.Level == PermissionLevel.Guest
        )
            throw HttpException.Unauthorized();

        if (user.Level < permissionLevel)
            throw HttpException.Forbidden("权限不足");
    }

    public static void EnsureAccess(
        this IHttpContext httpContext,
        string instanceId,
        bool strict = true
    )
    {
        if (
            !httpContext.Session.TryGetValue(SessionKeyConstants.User, out User? user)
            || user is null
            || user.Level == PermissionLevel.Guest
        )
            throw HttpException.Unauthorized();

        if (
            user.Level != PermissionLevel.Administrator
            && (!user.Instances.Contains(instanceId) || user.Level != PermissionLevel.Assistant)
            && (
                !user.Instances.Contains(instanceId)
                || user.Level != PermissionLevel.ReadOnly
                || strict
            )
        )
            throw HttpException.Forbidden("权限不足");
    }

    private static async Task SendJsonAsync<T>(this IHttpContext httpContext, ApiPacket<T> packet)
        where T : notnull
    {
        httpContext.Response.StatusCode = packet.Code;
        await httpContext.SendStringAsync(
            JsonSerializer.Serialize(packet, JsonSerializerOptionsFactory.CamelCase),
            "text/json",
            EncodingsMap.UTF8
        );

        httpContext.SetHandled();
    }

    private static async Task SendPacketAsync(this IHttpContext httpContext, ApiPacket packet) =>
        await SendJsonAsync(httpContext, packet);

    public static async Task SendPacketAsync<T>(
        this IHttpContext httpContext,
        T? data = default,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
        where T : notnull =>
        await SendJsonAsync(
            httpContext,
            new ApiPacket<T>() { Data = data, Code = (int)statusCode }
        );

    public static async Task SendPacketAsync(
        this IHttpContext httpContext,
        HttpStatusCode statusCode = HttpStatusCode.OK
    ) => await SendPacketAsync(httpContext, new ApiPacket() { Code = (int)statusCode });

    public static async Task HandleHttpException(IHttpContext context, IHttpException exception)
    {
        var httpStatusCode = (HttpStatusCode)exception.StatusCode;
        await context.SendPacketAsync(
            new ApiPacket
            {
                ErrorMsg =
                    exception.Message
                    ?? Regex.Replace(
                        httpStatusCode.ToString(),
                        @"^[A-Z]",
                        (c) => c.Value.ToLower()
                    ),
                Code = exception.StatusCode
            }
        );
    }

    public static async Task HandleException(IHttpContext context, Exception e)
    {
        if (context.IsLogined())
            await context.SendPacketAsync(new ApiPacket { ErrorMsg = $"{e.GetType()}", Code = 500 });
        else
            await context.SendPacketAsync(
                new ApiPacket { ErrorMsg = $"{e.GetType()}:{e.Message}", Code = 500 }
            );
    }
}
