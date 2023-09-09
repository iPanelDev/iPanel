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

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { init; get; }

    public Console(string? uuid)
        : base(uuid) { }
}
