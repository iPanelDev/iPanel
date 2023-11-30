using System.Text.Json.Serialization;

namespace iPanel.Core.Models.Client.Infos;

public class SystemInfo
{
    public string? OS { get; init; }

    public string? CPUName { get; init; }

    [JsonPropertyName("totalRam")]
    public double TotalRAM { get; init; }

    [JsonPropertyName("freeRam")]
    public double FreeRAM { get; init; }

    public double RAMUsage => TotalRAM == 0 ? 0 : (1 - FreeRAM / TotalRAM) * 100;

    public double CPUUsage { get; init; }
}
