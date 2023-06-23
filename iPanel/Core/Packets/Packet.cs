using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Packets
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal abstract class Packet
    {
        public string Type = string.Empty;

        public string SubType = string.Empty;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object? Sender;

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
