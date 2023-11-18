using System;
using System.Text.Json.Serialization;

namespace iPanel.Core.Models.Packets.Data;

public class Status
{
    public bool Logined { get; init; }

    public TimeSpan SessionDuration { get; init; }

    public object? User { get; init; }
}
