using Newtonsoft.Json;

namespace iPanelHost.WebSocket.Packets.DataBody
{
    internal struct Reason
    {
        /// <summary>
        /// 详细原因
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        public string DetailReason;

        public Reason(string reason)
        {
            DetailReason = reason;
        }
    }
}