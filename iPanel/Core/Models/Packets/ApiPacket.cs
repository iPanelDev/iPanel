using System;

namespace iPanel.Core.Models.Packets;

public class ApiPacket<T>
    where T : notnull
{
    public int Code { get; init; } = 200;

    public string? ErrorMsg { get; init; }

    public T? Data { get; init; }

    public DateTime Time { get; } = DateTime.Now;
}

public class ApiPacket : ApiPacket<object> { }
