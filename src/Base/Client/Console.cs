using iPanelHost.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Client;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Console : Client
{
    public string? SubscribingTarget;

    [JsonIgnore]
    public User? User;

    public Console(string? uuid)
        : base(uuid) { }
}
