using Newtonsoft.Json.Linq;
using System;

namespace iPanelHost.Base;

public class Request
{
    public string InstanceID;

    public DateTime StartTime = DateTime.Now;

    public JToken? Data;

    public bool HasReceived;

    public Request(string instanceId)
    {
        InstanceID = instanceId;
    }
}
