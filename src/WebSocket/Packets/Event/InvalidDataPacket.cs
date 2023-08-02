using iPanelHost.WebSocket.Packets.DataBody;

namespace iPanelHost.WebSocket.Packets.Event
{
    internal class InvalidDataPacket : SentPacket
    {
        public InvalidDataPacket(string reason) : base("event", "invalid_data", new Reason(reason))
        { }
    }
}