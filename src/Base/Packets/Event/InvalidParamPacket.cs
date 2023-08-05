using iPanelHost.Base.Packets.DataBody;

namespace iPanelHost.Base.Packets.Event
{
    internal class InvalidParamPacket : SentPacket
    {
        public InvalidParamPacket(string reason) : base("event", "invalid_param", new Result(reason))
        { }
    }
}