using iPanel.Core.Models.Packets.Data;

namespace iPanel.Core.Models.Packets.Event;

public class InvalidParamPacket : WsSentPacket
{
    public InvalidParamPacket(string reason)
        : base("event", "invalid_param", new Result(reason)) { }
}
