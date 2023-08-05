using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets
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

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
