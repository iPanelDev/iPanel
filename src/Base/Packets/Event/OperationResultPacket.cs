using iPanelHost.Base.Packets.DataBody;
using Newtonsoft.Json.Linq;

namespace iPanelHost.Base.Packets.Event
{
    public class OperationResultPacket : SentPacket
    {
        public OperationResultPacket(JToken? echo, string? reason)
            : base("event", "operation_result", new Result(reason, string.IsNullOrEmpty(reason)))
        {
            Echo = echo;
        }

        public OperationResultPacket(JToken? echo, ResultTypes result)
            : base("event", "operation_result", new Result(result))
        {
            Echo = echo;
        }
    }
}