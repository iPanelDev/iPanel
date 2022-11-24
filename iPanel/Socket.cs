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

        [JsonProperty(PropertyName = "guid")]
        public string GUID;
        
        [JsonProperty(PropertyName = "custom_name", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomName = string.Empty;
    }

    internal class ConsoleSocket : Socket
    {
        public string SelectTarget;
    }

    internal class InstanceSocket : Socket
    {
        [JsonIgnore]
        public Info Info = null;
    }
}
