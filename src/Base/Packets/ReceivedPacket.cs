using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public sealed class ReceivedPacket : Packet
{
    public JToken? Data { init; get; }

    public JToken? Echo { init; get; }
}
