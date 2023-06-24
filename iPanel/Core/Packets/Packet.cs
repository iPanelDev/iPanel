using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Packets
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal abstract class Packet
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string Type = string.Empty;

        /// <summary>
        /// 子类型
        /// </summary>
        public string SubType = string.Empty;

        /// <summary>
        /// 发送者
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object? Sender;

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
