using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanel.Core.Models.Client;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Users;
using iPanel.Core.Server.WebSocket.Handlers;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace iPanel.Core.Server.Api;

public partial class ApiMap
{
    [Route(HttpVerbs.Get, "/instance")]
    public async Task ListInstances()
    {
        var user = HttpContext.EnsureLogined();
        await HttpContext.SendJsonAsync(
            _app.HttpServer.InstanceWsModule.Instances.Values
                .Where(
                    (instance) =>
                        user.Level == PermissionLevel.Administrator
                        || (
                            user.Level == PermissionLevel.Assistant
                            || user.Level == PermissionLevel.ReadOnly
                        ) && user.Instances.Contains(instance.InstanceId)
                )
                .ToArray()
        );
    }

    [Route(HttpVerbs.Get, "/instance/{instanceId}")]
    public async Task GetInstanceInfo(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId, false);

        if (
            _app.HttpServer.InstanceWsModule.Instances.TryGetValue(
                instanceId,
                out Instance? instance
            ) && instance is not null
        )
            await HttpContext.SendJsonAsync(instance);
        else
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
    }

    [Route(HttpVerbs.Get, "/instance/{instanceId}/subscribe")]
    public async Task SubscribeInstance(string instanceId, [QueryField(true)] string connectionId)
    {
        HttpContext.EnsureAccess(instanceId, false);

        if (
            !_app.HttpServer.BroadcastWsModule.Listeners.TryGetValue(connectionId, out var listener)
        )
            throw HttpException.BadRequest("未连接到广播WebSocket服务器");

        if (_app.HttpServer.InstanceWsModule.Instances.ContainsKey(instanceId))
        {
            listener.InstanceIdSubscribed = instanceId;
            await HttpContext.SendJsonAsync(null, HttpStatusCode.OK);
        }
        else
        {
            HttpContext.Session[SessionKeyConstants.InstanceId] = null!;
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    [Route(HttpVerbs.Get, "/instance/{instanceId}/start")]
    public async Task CallInstanceStart(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            _app.HttpServer.InstanceWsModule.Instances.TryGetValue(
                instanceId,
                out Instance? instance
            ) && instance is not null
        )
        {
            instance?.SendAsync(
                new WsSentPacket("request", "server_start", null, sender: Sender.FromUser())
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    [Route(HttpVerbs.Get, "/instance/{instanceId}/stop")]
    public async Task CallInstanceStop(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            _app.HttpServer.InstanceWsModule.Instances.TryGetValue(
                instanceId,
                out Instance? instance
            ) && instance is not null
        )
        {
            instance?.SendAsync(
                new WsSentPacket("request", "server_stop", null, sender: Sender.FromUser())
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
    }

    [Route(HttpVerbs.Get, "/instance/{instanceId}/kill")]
    public async Task CallInstanceKill(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            _app.HttpServer.InstanceWsModule.Instances.TryGetValue(
                instanceId,
                out Instance? instance
            ) && instance is not null
        )
        {
            instance?.SendAsync(
                new WsSentPacket("request", "server_kill", null, sender: Sender.FromUser())
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
    }

    [Route(HttpVerbs.Post, "/instance/{instanceId}/input")]
    public async Task CallInstanceInput(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        var inputs = await HttpContext.ConvertRequestTo<string[]>();
        if (inputs is null || inputs.Length == 0)
            throw HttpException.BadRequest("缺少输入数据");

        if (
            _app.HttpServer.InstanceWsModule.Instances.TryGetValue(
                instanceId,
                out Instance? instance
            ) && instance is not null
        )
        {
            instance.SendAsync(
                new WsSentPacket("request", "server_input", inputs, sender: Sender.FromUser())
            );
            await HttpContext.SendJsonAsync(null, HttpStatusCode.Accepted);
        }
        else
        {
            await HttpContext.SendJsonAsync(null, HttpStatusCode.NotFound);
        }
    }

    [Route(HttpVerbs.Get, "/instance/{instanceId}/dir")]
    public async Task GetInstanceDirInfo(string instanceId, [QueryField(true)] string path)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            !_app.HttpServer.InstanceWsModule.Instances.TryGetValue(
                instanceId,
                out Instance? instance
            ) || instance is null
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
                await HttpContext.SendJsonAsync("等待超时", 421);
                Logger.Warn($"[{HttpContext.Id}] 实例({instanceId})回复超时");
            }
        }
    }
}
