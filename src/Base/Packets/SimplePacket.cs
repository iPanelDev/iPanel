using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.Base.Packets;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class SimplePacket
{
    public int Code { get; init; } = 200;

    public object? Data { get; init; }

    public long Time { get; init; } =
        (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
}
