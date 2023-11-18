using EmbedIO.WebSockets;
using iPanel.Core.Models.Users;
using iPanel.Core.Server;
using System.Text.Json.Serialization;

namespace iPanel.Core.Models.Client;

public class ConsoleListener : Client
{
    [JsonIgnore]
    public User User;

    public string? InstanceIdSubscribed =>
        Context?.Session[SessionKeyConstants.InstanceId]?.ToString();

    public ConsoleListener(User user, IWebSocketContext context)
    {
        User = user;
        Context = context;
    }
}
