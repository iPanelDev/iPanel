using iPanelHost.WebSocket.Packets.DataBody;

namespace iPanelHost.WebSocket.Packets.Event
{
    internal class InvalidParamPacket : SentPacket
    {
        public InvalidParamPacket(string reason) : base("event", "invalid_param", new Reason(reason))
        { }
    }
}