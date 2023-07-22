using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Base
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class Setting
    {
        public WSSetting WebSocket = new();
        public bool Debug;

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        internal struct WSSetting
        {
            public string Addr = "ws://0.0.0.0:30000";
            public string Password = string.Empty;

            public WSSetting() { }
        }
    }
}
