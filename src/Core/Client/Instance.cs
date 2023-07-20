using iPanel.Core.Client.Info;
using Newtonsoft.Json;

namespace iPanel.Core.Client
{
    internal class Instance : Client
    {
        [JsonIgnore]
        public FullInfo FullInfo;

        public ShortInfo ShortInfo => new(FullInfo);

        [JsonIgnore]
        public new ClientType Type => ClientType.Instance;
    }
}