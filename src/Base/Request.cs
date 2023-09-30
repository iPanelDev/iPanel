using Newtonsoft.Json.Linq;
using System;

namespace iPanelHost.Base;

public class Request
{
    public string CallerUUID;

    public string InstanceID;

    public DateTime StartTime = DateTime.Now;

    public JToken? Echo;

    public Request(string uuid, string instanceId)
    {
        CallerUUID = uuid;
        InstanceID = instanceId;
    }
}
