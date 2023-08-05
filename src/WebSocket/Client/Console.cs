using iPanelHost.Permissons;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.WebSocket.Client
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class Console : Client
    {
        [JsonIgnore]
        public new ClientType Type => ClientType.Console;

        public string? SubscribingTarget;

        public User? User;

        public Console(string? guid) : base(guid)
        { }
    }
}