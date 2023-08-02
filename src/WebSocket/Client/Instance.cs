using iPanelHost.WebSocket.Client.Info;
using Newtonsoft.Json;

namespace iPanelHost.WebSocket.Client
{
    internal class Instance : Client
    {
        [JsonIgnore]
        public FullInfo FullInfo;

        public ShortInfo ShortInfo => new(FullInfo);

        [JsonIgnore]
        public new ClientType Type => ClientType.Instance;

        public Instance(string? guid) : base(guid)
        { }
    }
}