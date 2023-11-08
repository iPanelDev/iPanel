using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Base.Packets;
using iPanelHost.Server.WebSocket;
using iPanelHost.Server.WebSocket.Handlers;
using iPanelHost.Utils;
using Newtonsoft.Json.Linq;

namespace iPanelHost.Server;

public partial class ApiMap
{
    /// <summary>
    /// 列出实例
    /// </summary>
    [Route(HttpVerbs.Get, "/instance/list")]
    public async Task ListInstances()
    {
        User user = HttpContext.EnsureLogined();
        await HttpContext.SendJsonAsync(
            JArray.FromObject(
                MainWsModule
                    .Instances
                    .Values
                    .Where(
                        (instance) =>
                            user.Level == PermissionLevel.Administrator
                            || (
                                user.Level == PermissionLevel.Assistant
                                || user.Level == PermissionLevel.ReadOnly
                            ) && user.Instances.Contains(instance.InstanceID)
                    )
                    .ToArray()
            )
        );
    }

    /// <summary>
    /// 获取实例信息
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/info")]
    public async Task GetInstanceInfo(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId, false);

        if (
            MainWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            await HttpContext.SendJsonAsync(instance);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 启动实例服务器
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/subscribe")]
    public async Task SubscribeInstance(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId, false);

        if (MainWsModule.Instances.ContainsKey(instanceId))
        {
            HttpContext.Session[ApiHelper.INSTANCEIDKEY] = instanceId;
            await HttpContext.SendJsonAsync(null, HttpStatusCode.OK);
        }
        else
        {
            HttpContext.Session[ApiHelper.INSTANCEIDKEY] = null!;
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 启动实例服务器
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/start")]
    public async Task CallInstanceStart(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            MainWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            instance?.Send(
                new SentPacket(
                    "request",
                    "server_start",
                    null,
                    sender: Sender.FromUser()
                ).ToString()
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 关闭实例服务器
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/stop")]
    public async Task CallInstanceStop(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            MainWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            instance?.Send(
                new SentPacket("request", "server_stop", null, sender: Sender.FromUser()).ToString()
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 强制结束实例服务器
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/kill")]
    public async Task CallInstanceKill(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            MainWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            instance?.Send(
                new SentPacket("request", "server_kill", null, sender: Sender.FromUser()).ToString()
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 向实例服务器输入
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Post, "/instance/{instanceId}/input")]
    public async Task CallInstanceInput(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        string[]? inputs = await HttpContext.ConvertRequestTo<string[]>();
        if (inputs is null || inputs.Length == 0)
        {
            throw HttpException.BadRequest("缺少输入数据");
        }

        if (
            MainWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            instance.Send(
                new SentPacket(
                    "request",
                    "server_input",
                    inputs,
                    sender: Sender.FromUser()
                ).ToString()
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// 向实例服务器输入
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    [Route(HttpVerbs.Get, "/instance/{instanceId}/dir")]
    public async Task GetInstanceDirInfo(string instanceId, [QueryField(true)] string path)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            !MainWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            || instance is null
        )
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
        else
        {
            try
            {
                await HttpContext.SendJsonAsync(
                    await RequestsFactory.Create<object>(instance, "get_dir_info", path),
                    HttpStatusCode.OK
                );
            }
            catch (TimeoutException)
            {
                await HttpContext.SendJsonAsync("Timeout", 421);
                Logger.Warn($"[{HttpContext.Id}] 实例({instanceId})回复超时");
            }
        }
    }
}
