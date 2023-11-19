using System;
using System.Text.Json.Nodes;

namespace iPanel.Core.Models;

public class Request
{
    public string InstanceId;

    public DateTime StartTime = DateTime.Now;

    public JsonNode? Data;

    public bool HasReceived;

    public Request(string instanceId)
    {
        InstanceId = instanceId;
    }
}
