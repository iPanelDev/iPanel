namespace iPanel.Core.Models.Client.Infos;

public class SystemInfo
{
    public string? OS { get; init; }

    public string? CPUName { get; init; }

    public long TotalRAM { get; init; }

    public long FreeRAM { get; init; }

    public double RAMUsage => TotalRAM == 0 ? 0 : (1 - (double)FreeRAM / TotalRAM) * 100;

    public double CPUUsage { get; init; }
}
