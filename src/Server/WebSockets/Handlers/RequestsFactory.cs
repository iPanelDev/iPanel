using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Base.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iPanelHost.Server.WebSocket.Handlers;

public static class RequestsFactory
{
    public static readonly Dictionary<string, Request> Requests = new();

    /// <summary>
    /// 创建请求
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="subType">数据包子类型</param>
    /// <param name="body">数据包主体</param>
    /// <returns>请求结果</returns>
    public static async Task<T?> Create<T>(Instance instance, string subType, object? body = null)
    {
        string id = Guid.NewGuid().ToString("N");
        Request request = new(instance.InstanceID);
        Requests.Add(id, request);
        instance.Send(new SentPacket("request", subType, body) { RequestId = id });

        for (int i = 0; i < 200; i++)
        {
            if (request.HasReceived)
            {
                return request.Data is null ? default : request.Data.ToObject<T>();
            }
            await Task.Delay(50);
        }
        throw new TimeoutException();
    }

    /// <summary>
    /// 标记为已接收
    /// </summary>
    /// <param name="id">请求ID</param>
    /// <param name="instanceId">实例ID</param>
    /// <param name="body">返回主体</param>
    public static void MarkAsReceived(string id, string instanceId, JToken? body = null)
    {
        if (!Requests.TryGetValue(id, out Request? request) || request.InstanceID != instanceId)
        {
            throw new ArgumentException("无法找到指定请求ID", nameof(id));
        }

        request.HasReceived = true;
        request.Data = body;
    }
}
