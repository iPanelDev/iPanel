using iPanelHost.WebSocket.Packets.DataBody;

namespace iPanelHost.WebSocket.Packets.Event
{
    internal class InvalidTargetPacket : SentPacket
    {
        public InvalidTargetPacket() : base("event", "invalid_target", new Reason("订阅目标无效"))
        { }
    }
}