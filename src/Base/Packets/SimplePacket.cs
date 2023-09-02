using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.Base.Packets;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class SimplePacket
{
    public int Code { get; init; } = 200;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? Success { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public object? Data { get; set; }

    public readonly long Time = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
}
