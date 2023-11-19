using iPanel.Core.Models.Packets.Data;

namespace iPanel.Core.Models.Packets.Event;

public class VerifyResultPacket : WsSentPacket
{
    public VerifyResultPacket(bool success, string? reason = null)
        : base("event", "verify_result", new Result(reason, success)) { }
}
