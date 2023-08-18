using iPanelHost.Base.Packets.DataBody;

namespace iPanelHost.Base.Packets.Event
{
    internal class OperationResultPacket : SentPacket
    {
        public OperationResultPacket(string? reason) : base("event", "operation_result", new Result(reason, string.IsNullOrEmpty(reason)))
        { }
    }
}