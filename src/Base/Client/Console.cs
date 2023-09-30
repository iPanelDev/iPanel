using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Client;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Console : Client
{
    /// <summary>
    /// 订阅的实例ID
    /// </summary>
    public string? InstanceIdSubscribed;

    [JsonIgnore]
    public User? User;

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { init; get; }

    public Console(string userName, string uuid)
        : base(uuid)
    {
        UserName = userName;
    }
}
