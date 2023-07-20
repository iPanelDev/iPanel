using iPanel.Core.Packets.DataBody;

namespace iPanel.Core.Packets.Event
{
    internal class InvalidTargetPacket : SentPacket
    {
        public InvalidTargetPacket() : base("event", "invalid_target", new Reason("订阅目标无效"))
        { }
    }
}