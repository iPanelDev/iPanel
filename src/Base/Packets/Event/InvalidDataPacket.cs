using iPanelHost.Base.Packets.DataBody;

namespace iPanelHost.Base.Packets.Event
{
    public class InvalidDataPacket : SentPacket
    {
        public InvalidDataPacket(string reason) : base("event", "invalid_data", new Result(reason))
        { }
    }
}