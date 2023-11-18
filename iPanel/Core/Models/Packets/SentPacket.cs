using System;

namespace iPanel.Core.Models.Packets;

public class SentPacket : WsPacket<object>
{
    public DateTime Time { get; } = DateTime.Now;

    private static readonly Sender _selfSender =
        new($"iPanel", "host", null, new() { Version = Constant.Version });

    public Sender? Sender { get; init; }

    public SentPacket() { }

    public SentPacket(string type, string sub_type, object? data = null)
        : this(type, sub_type, data, _selfSender) { }

    public SentPacket(string type, string sub_type, object? data, Sender sender)
    {
        Type = type;
        SubType = sub_type;
        Data = data;
        Sender = sender;
    }
}
