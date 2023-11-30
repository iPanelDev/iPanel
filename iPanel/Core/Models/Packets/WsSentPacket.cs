using System;

namespace iPanel.Core.Models.Packets;

public class WsSentPacket : WsPacket<object>
{
    public DateTime Time { get; } = DateTime.Now;

    private static readonly Sender _selfSender =
        new(
            $"iPanel",
            "host",
            null,
            new()
            {
                Version = Constant.Version,
                Environment = Environment.Version.ToString(),
                Name = "iPanel"
            }
        );

    public WsSentPacket() { }

    public WsSentPacket(string type, string sub_type, object? data = null)
        : this(type, sub_type, data, _selfSender) { }

    public WsSentPacket(string type, string sub_type, object? data, Sender sender)
    {
        Type = type;
        SubType = sub_type;
        Data = data;
        Sender = sender;
    }
}
