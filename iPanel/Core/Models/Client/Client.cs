using System.Text.Json.Serialization;
using System.Threading.Tasks;

using EmbedIO.WebSockets;

using iPanel.Utils.Extensions;

namespace iPanel.Core.Models.Client;

public abstract class Client
{
    [JsonIgnore]
    public IWebSocketContext Context { get; init; } = null!;

    public string? Address => Context.RemoteEndPoint.ToString();

    public async Task SendAsync(string text) => await Context.SendAsync(text);
}
