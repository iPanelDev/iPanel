using System.Text.Json.Serialization;

namespace iPanel.Core.Models.Client.Infos;

public class ServerInfo
{
    public string? Filename { get; init; }

    [JsonRequired]
    public bool Status { get; init; }

    public string? RunTime { get; init; }

    public double Usage { get; init; }

    public int Capacity { get; set; }

    public int OnlinePlayers { get; set; }

    public string? Version { get; set; }
}
