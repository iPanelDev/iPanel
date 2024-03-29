using iPanel.Core.Models.Packets.Data;

namespace iPanel.Core.Models.Packets.Event;

public class InvalidDataPacket : WsSentPacket
{
    public InvalidDataPacket(string reason)
        : base("event", "invalid_data", new Result(reason)) { }
}
