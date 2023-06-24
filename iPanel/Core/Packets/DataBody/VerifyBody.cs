using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct VerifyBody
    {
        public string Token;

        /// <summary>
        /// 自定义名称
        /// </summary>
        public string CustomName;

        /// <summary>
        /// 客户端类型
        /// </summary>
        public string ClientType;
    }
}