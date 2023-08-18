using iPanelHost.Permissons;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.WebSocket.Client
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class Console : Client
    {
        public string? SubscribingTarget;

        [JsonIgnore]
        public User? User;

        public Console(string? uuid) : base(uuid)
        { }
    }
}