using EmbedIO.WebSockets;
using iPanelHost.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Client;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ConsoleListener : Client
{
    [JsonIgnore]
    public User User;

    /// <summary>
    /// 订阅的实例ID
    /// </summary>
    public string? InstanceIdSubscribed => Context?.Session[ApiHelper.INSTANCEIDKEY]?.ToString();

    public ConsoleListener(User user, IWebSocketContext context)
    {
        User = user;
        Context = context;
    }
}
