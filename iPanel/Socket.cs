using Fleck;
using Newtonsoft.Json;
using System;

namespace iPanel
{
    internal class Socket
    {
        [JsonIgnore]
        public IWebSocketConnection WebSocketConnection;

        [JsonIgnore]
        public DateTime LastTime = DateTime.Now;

        [JsonProperty(PropertyName = "custom_name", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomName = string.Empty;

        [JsonProperty(PropertyName = "guid")]
        public string GUID;

        [JsonProperty(PropertyName = "select_target")]
        public string SelectTarget;

        [JsonProperty(PropertyName = "type")]
        public string Type = null;
    }

    internal class ConsoleSocket : Socket
    {
        [JsonProperty(PropertyName = "type")]
        public readonly new string Type = "console";
    }

    internal class InstanceSocket : Socket
    {
        [JsonIgnore]
        public Info Info = null;

        [JsonProperty(PropertyName = "type")]
        public readonly new string Type = "instance";
    }
}
