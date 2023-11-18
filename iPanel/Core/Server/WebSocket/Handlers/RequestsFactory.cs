using iPanel.Core.Models;
using iPanel.Core.Models.Client;
using iPanel.Core.Models.Packets;
using iPanel.Utils.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace iPanel.Core.Server.WebSocket.Handlers;

public static class RequestsFactory
{
    public static readonly Dictionary<string, Request> Requests = new();

    public static async Task<T?> Create<T>(Instance instance, string subType, object? body = null)
        where T : notnull
    {
        string id = Guid.NewGuid().ToString("N");
        Request request = new(instance.InstanceID);
        Requests.Add(id, request);

        await instance.SendAsync(new SentPacket("request", subType, body) { RequestId = id });

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

    public static void MarkAsReceived(string id, string instanceId, JsonNode? body = null)
    {
        if (!Requests.TryGetValue(id, out Request? request) || request.InstanceID != instanceId)
        {
            throw new ArgumentException("无法找到指定请求ID", nameof(id));
        }

        request.HasReceived = true;
        request.Data = body;
    }
}
