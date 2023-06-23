using Newtonsoft.Json;

namespace iPanel.Core.Client
{
    internal class Instance : Client
    {
        [JsonIgnore]
        public Info? Info;

        [JsonIgnore]
        public new ClientType Type => ClientType.Instance;
    }
}