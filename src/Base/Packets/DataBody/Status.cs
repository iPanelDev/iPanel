using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Status
{
    public bool IsVerified;

    public TimeSpan Duration;
}
