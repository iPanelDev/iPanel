using iPanel.Core.Packets.DataBody;

namespace iPanel.Core.Packets.Event
{
    internal class VerifyResultPacket : SentPacket
    {
        public VerifyResultPacket(bool success, string? reason = null) : base("event", "verify_result", new Result(success, reason))
        { }
    }
}