using Newtonsoft.Json;

namespace iPanel.Core.Packets.DataBody
{
    internal struct Reason
    {
        [JsonProperty(PropertyName = "reason")]
        public string DetailReason;

        public Reason(string reason)
        {
            DetailReason = reason;
        }
    }
}