using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Client
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class Console : Client
    {
        [JsonIgnore]
        public new ClientType Type => ClientType.Console;

        public string? SubscribingTarget;
    }
}