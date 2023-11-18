using System.Text.Json;
using System.Text.Json.Serialization;
using iPanel.Utils.Json;

namespace iPanel.Core.Models.Packets;

public abstract class WsPacket<T>
{
    [JsonRequired]
    public string Type { get; init; } = string.Empty;

    [JsonRequired]
    public string SubType { get; init; } = string.Empty;

    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RequestId { get; init; }

    public override string ToString() =>
        JsonSerializer.Serialize(this, JsonSerializerOptionsFactory.CamelCase);

    public static implicit operator string(WsPacket<T> packet) => packet.ToString();
}
