using iPanel.Core.Packets.DataBody;

namespace iPanel.Core.Packets.Event
{
    internal class InvalidDataPacket : SentPacket
    {
        public InvalidDataPacket(string reason) : base("event", "invalid_data", new Reason(reason))
        { }
    }
}