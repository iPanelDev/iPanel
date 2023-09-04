using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanelHost.Base;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Service;
using iPanelHost.Service.Handlers;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iPanelHost.Server;

#pragma warning disable CA1822

public class Apis : WebApiController
{
    private static readonly Encoding UTF8 = new UTF8Encoding(false);

    /// <summary>
    /// 根目录
    /// </summary>
    [Route(HttpVerbs.Any, "/")]
    public void Root() => throw HttpException.Redirect("/");

    [Route(HttpVerbs.Get, "/ping")]
    public async Task Ping()
    {
        await HttpContext.SendStringAsync(DateTime.Now.ToString("o"), "text/plain", UTF8);
    }

    [Route(HttpVerbs.Get, "/status")]
    public async Task Status()
    {
        if (
            HttpContext.Session.TryGetValue("user", out object? value)
            && value is User
            && value is not null
        )
        {
            await SendJson(
                HttpContext,
                new Status { IsVerified = true, Duration = HttpContext.Session.Duration }
            );
        }
        else
        {
            await SendJson(
                HttpContext,
                new Status { IsVerified = false },
                HttpStatusCode.Unauthorized
            );
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    [Route(HttpVerbs.Post, "/uploadSimplly")]
    public async Task SimpleUpload()
    {
        if (
            !(
                HttpContext.Session.TryGetValue("user", out object? value)
                && value is User
                && value is not null
            )
        )
        {
            await SendJson(
                HttpContext,
                ResultTypes.NotVerifyYet.ToString(),
                false,
                HttpStatusCode.Unauthorized
            );
            return;
        }
        Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
        await FileTransferStation.SimpleUpload(HttpContext);
    }

    /// <summary>
    /// 上传文件流
    /// </summary>
    [Route(HttpVerbs.Post, "/upload")]
    public async Task StreamUpload()
    {
        if (
            !(
                HttpContext.Session.TryGetValue("user", out object? value)
                && value is User
                && value is not null
            )
        )
        {
            await SendJson(
                HttpContext,
                ResultTypes.NotVerifyYet.ToString(),
                false,
                HttpStatusCode.Unauthorized
            );
            return;
        }
        Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
        await FileTransferStation.StreamUpload(HttpContext);
    }

    [Route(HttpVerbs.Get, "/verify")]
    [Route(HttpVerbs.Post, "/verify")]
    public async Task Verify()
    {
        if (
            HttpContext.Session.TryGetValue("user", out object? value)
            && value is User
            && value is not null
        )
        {
            await SendJson(HttpContext, true);
            return;
        }
        VerifyBody? verifyBody = await ConvertRequsetTo<VerifyBody>();
        if (
            verifyBody is null
            || string.IsNullOrEmpty(verifyBody?.Token)
            || string.IsNullOrEmpty(verifyBody?.UUID)
        )
        {
            await SendJson(
                HttpContext,
                ResultTypes.LostArgs.ToString(),
                false,
                HttpStatusCode.BadRequest
            );
            return;
        }
        if (string.IsNullOrEmpty(verifyBody?.Account))
        {
            await SendJson(
                HttpContext,
                ResultTypes.EmptyAccount.ToString(),
                false,
                HttpStatusCode.BadRequest
            );
            return;
        }
        if (
            !MainHandler.Consoles.ContainsKey(verifyBody.UUID!)
            || !UserManager.Users.TryGetValue(verifyBody.Account!, out User? user)
            || General.GetMD5(verifyBody.UUID + verifyBody.Account! + user.Password)
                != verifyBody.Token
        )
        {
            await SendJson(
                HttpContext,
                ResultTypes.IncorrectAccountOrPassword.ToString(),
                false,
                HttpStatusCode.BadRequest
            );
            return;
        }

        HttpContext.Session["user"] = user;
        await SendJson(HttpContext, true);
    }

    /// <summary>
    /// 将请求内容转为对象
    /// </summary>
    /// <typeparam name="T">要转化的类型</typeparam>
    private async Task<T?> ConvertRequsetTo<T>()
    {
        try
        {
            string? body = await HttpContext.GetRequestBodyAsStringAsync();
            if (!string.IsNullOrEmpty(body))
            {
                return JsonConvert.DeserializeObject<T>(body);
            }

            JObject jObject = new();
            NameValueCollection queryData = HttpContext.GetRequestQueryData();
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
            Logger.Warn($"[{HttpContext.Id}] 尝试转化Body失败: {e.Message}");
            return default;
        }
    }

    /// <summary>
    /// 发送json
    /// </summary>
    /// <param name="data">数据字段</param>
    /// <param name="statusCode">状态码</param>
    public static async Task SendJson(
        IHttpContext httpContext,
        object? data,
        HttpStatusCode statusCode = HttpStatusCode.OK
    ) => await SendJson(httpContext, data, null, statusCode);

    /// <summary>
    /// 发送json
    /// </summary>
    /// <param name="data">数据字段</param>
    /// <param name="statusCode">状态码</param>
    public static async Task SendJson(
        IHttpContext httpContext,
        object? data,
        bool? success,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
    {
        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.SendStringAsync(
            JsonConvert.SerializeObject(
                new SimplePacket()
                {
                    Data = data,
                    Success = success,
                    Code = (int)statusCode,
                }
            ),
            "text/json",
            UTF8
        );
    }
}

#pragma warning restore CA1822
