using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.WebSocket.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct VerifyBody
    {
        public string? Token;

        /// <summary>
        /// 自定义名称
        /// </summary>
        public string? CustomName;

        /// <summary>
        /// 账号
        /// </summary>
        public string? Account;

        /// <summary>
        /// 客户端类型
        /// </summary>
        public string? ClientType;
    }
}