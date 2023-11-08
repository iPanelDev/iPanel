using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Status
{
    public bool Logined;

    public TimeSpan SessionDuration;

    public object? User;

    [JsonProperty("uuid")]
    public object? UUID;
}
