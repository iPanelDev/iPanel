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

    [Route(HttpVerbs.Any, "/ping")]
    public string Ping() => DateTime.Now.ToString("o");

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
            await SendJson(HttpContext, ResultTypes.NotVerifyYet.ToString(), false, 401);
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
            await SendJson(HttpContext, ResultTypes.NotVerifyYet.ToString(), false, 401);
            return;
        }
        Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
        await FileTransferStation.StreamUpload(HttpContext);
    }

    [Route(HttpVerbs.Any, "/verify")]
    public async Task Verify()
    {
        if (
            HttpContext.Session.TryGetValue("user", out object? value)
            && value is User
            && value is not null
        )
        {
            await SendJson(HttpContext, null, true);
            return;
        }
        VerifyBody? verifyBody = await ConvertRequsetTo<VerifyBody>();
        if (string.IsNullOrEmpty(verifyBody?.Account))
        {
            await SendJson(HttpContext, ResultTypes.EmptyAccount.ToString(), false, 400);
            return;
        }
        if (string.IsNullOrEmpty(verifyBody.Token) || string.IsNullOrEmpty(verifyBody.SessionId))
        {
            await SendJson(HttpContext, ResultTypes.LostArgs.ToString(), false, 400);
            return;
        }
        if (
            !MainHandler.Consoles.ContainsKey(verifyBody.SessionId)
            || !UserManager.Users.TryGetValue(verifyBody.Account, out User? user)
            || General.GetMD5(verifyBody.SessionId + verifyBody.Account! + user.Password)
                != verifyBody.Token
        )
        {
            await SendJson(
                HttpContext,
                ResultTypes.IncorrectAccountOrPassword.ToString(),
                false,
                400
            );
            return;
        }

        HttpContext.Session["user"] = user;
        await SendJson(HttpContext, null, true);
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
        bool? success = null,
        int statusCode = 200
    )
    {
        httpContext.Response.StatusCode = statusCode;
        await httpContext.SendStringAsync(
            JsonConvert.SerializeObject(
                new SimplePacket()
                {
                    Data = data,
                    Success = success,
                    Code = statusCode,
                }
            ),
            "text/json",
            UTF8
        );
    }
}

#pragma warning restore CA1822
