using System;

namespace iPanel.Core.Models.Packets;

public class ApiPacket
{
    public int Code { get; init; } = 200;

    public string? ErrorMsg { get; init; }

    public object? Data { get; init; }

    public DateTime Time { get; } = DateTime.Now;
}
