using EmbedIO.WebSockets;
using iPanel.Core.Models.Users;
using iPanel.Core.Server;
using System.Text.Json.Serialization;

namespace iPanel.Core.Models.Client;

public class ConsoleListener : Client
{
    [JsonIgnore]
    public User User;

    public string? InstanceIdSubscribed { get; set; }

    [JsonIgnore]
    public string ConnectionId { get; set; }

    public ConsoleListener(User user, IWebSocketContext context, string connectionId)
    {
        User = user;
        Context = context;
        ConnectionId = connectionId;
    }
}
