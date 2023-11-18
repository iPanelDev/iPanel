using iPanel.Core.Models.Packets.Data;

namespace iPanel.Core.Models.Packets.Event;

public class InvalidParamPacket : SentPacket
{
    public InvalidParamPacket(string reason)
        : base("event", "invalid_param", new Result(reason)) { }
}
