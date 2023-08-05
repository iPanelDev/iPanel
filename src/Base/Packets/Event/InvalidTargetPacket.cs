using iPanelHost.Base.Packets.DataBody;

namespace iPanelHost.Base.Packets.Event
{
    internal class InvalidTargetPacket : SentPacket
    {
        public InvalidTargetPacket() : base("event", "invalid_target", new Result(Result.InvalidTarget))
        { }
    }
}