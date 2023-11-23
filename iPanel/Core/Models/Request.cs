using System;
using System.Text.Json.Nodes;

namespace iPanel.Core.Models;

public class Request
{
    public string InstanceId { get; set; }

    public DateTime StartTime { get; } = DateTime.Now;

    public JsonNode? Data { get; set; }

    public bool HasReceived { get; set; }

    public Request(string instanceId)
    {
        InstanceId = instanceId;
    }
}
