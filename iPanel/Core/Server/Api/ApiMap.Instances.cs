using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanel.Core.Models.Client;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Users;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace iPanel.Core.Server.Api;

public partial class ApiMap
{
    [Route(HttpVerbs.Get, "/instances")]
    public async Task GetAllInstances()
    {
        var user = HttpContext.EnsureLogined();
        await HttpContext.SendPacketAsync(
            InstanceWsModule.Instances.Values
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

    [Route(HttpVerbs.Get, "/instances/{instanceId}")]
    public async Task GetInstance(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId, false);

        if (
            InstanceWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
            await HttpContext.SendPacketAsync(instance);
        else
            throw HttpException.NotFound("实例不存在");
    }

    [Route(HttpVerbs.Get, "/instances/{instanceId}/subscribe")]
    public async Task SubscribeInstance(string instanceId, [QueryField(true)] string connectionId)
    {
        HttpContext.EnsureAccess(instanceId, false);

        if (!BroadcastWsModule.Listeners.TryGetValue(connectionId, out var listener))
            throw HttpException.BadRequest("未连接到广播WebSocket服务器");

        if (InstanceWsModule.Instances.ContainsKey(instanceId))
        {
            listener.InstanceIdSubscribed = instanceId;
            await HttpContext.SendPacketAsync();
        }
        else
            throw HttpException.NotFound("实例不存在");
    }

    [Route(HttpVerbs.Get, "/instances/{instanceId}/start")]
    public async Task CallInstanceStart(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            InstanceWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            await instance.SendAsync(
                new WsSentPacket(
                    "request",
                    "server_start",
                    null,
                    Sender.CreateUserSender(
                        HttpContext.Session[SessionKeyConstants.UserName]?.ToString()
                    )
                )
            );
            await HttpContext.SendPacketAsync(HttpStatusCode.Accepted);
        }
        else
            throw HttpException.NotFound("实例不存在");
    }

    [Route(HttpVerbs.Get, "/instances/{instanceId}/stop")]
    public async Task CallInstanceStop(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            InstanceWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            await instance.SendAsync(
                new WsSentPacket(
                    "request",
                    "server_stop",
                    null,
                    Sender.CreateUserSender(
                        HttpContext.Session[SessionKeyConstants.UserName]?.ToString()
                    )
                )
            );
            await HttpContext.SendPacketAsync(HttpStatusCode.Accepted);
        }
        else
            throw HttpException.NotFound("实例不存在");
    }

    [Route(HttpVerbs.Get, "/instances/{instanceId}/kill")]
    public async Task CallInstanceKill(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        if (
            InstanceWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            await instance.SendAsync(
                new WsSentPacket(
                    "request",
                    "server_kill",
                    null,
                    Sender.CreateUserSender(
                        HttpContext.Session[SessionKeyConstants.UserName]?.ToString()
                    )
                )
            );
            await HttpContext.SendPacketAsync(HttpStatusCode.Accepted);
        }
        else
            throw HttpException.NotFound("实例不存在");
    }

    [Route(HttpVerbs.Post, "/instances/{instanceId}/input")]
    public async Task CallInstanceInput(string instanceId)
    {
        HttpContext.EnsureAccess(instanceId);

        var inputs = await HttpContext.ConvertRequestTo<string[]>();
        if (inputs is null || inputs.Length == 0)
            throw HttpException.BadRequest("缺少输入数据");

        if (
            InstanceWsModule.Instances.TryGetValue(instanceId, out Instance? instance)
            && instance is not null
        )
        {
            await instance.SendAsync(
                new WsSentPacket(
                    "request",
                    "server_input",
                    inputs,
                    Sender.CreateUserSender(
                        HttpContext.Session[SessionKeyConstants.UserName]?.ToString()
                    )
                )
            );
            await HttpContext.SendPacketAsync(HttpStatusCode.Accepted);
        }
        else
            throw HttpException.NotFound("实例不存在");
    }
}
