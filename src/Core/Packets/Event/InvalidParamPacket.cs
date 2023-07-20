using iPanel.Core.Packets.DataBody;

namespace iPanel.Core.Packets.Event
{
    internal class InvalidParamPacket : SentPacket
    {
        public InvalidParamPacket(string reason) : base("event", "invalid_param", new Reason(reason))
        { }
    }
}