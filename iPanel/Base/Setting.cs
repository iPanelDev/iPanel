using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Base
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class Setting
    {
        public string WsAddr { get; set; } = "ws://0.0.0.0:30000";
        public string Password { get; set; } = string.Empty;
        public bool Debug { get; set; }
    }
}
