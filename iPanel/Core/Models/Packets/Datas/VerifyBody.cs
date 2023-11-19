using iPanel.Core.Models.Client;
using System.Text.Json.Serialization;

namespace iPanel.Core.Models.Packets.Data;

public class VerifyBody
{
    [JsonPropertyName("md5")]
    [JsonRequired]
    public string MD5 { get; init; } = string.Empty;

    [JsonRequired]
    public string Time { get; init; } = string.Empty;

    public string? CustomName { get; init; }

    public string? InstanceId { get; init; }

    public Metadata? Metadata { get; init; }

    public string? UserName { get; init; }
}
