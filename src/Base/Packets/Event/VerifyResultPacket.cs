using iPanelHost.Base.Packets.DataBody;

namespace iPanelHost.Base.Packets.Event;

public class VerifyResultPacket : SentPacket
{
    public VerifyResultPacket(bool success, string? reason = null)
        : base("event", "verify_result", new Result(reason, success)) { }
}
