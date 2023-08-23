using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using iPanelHost.WebSocket.Client;

namespace iPanelHost.Base.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class VerifyBody
    {
        public string? Token;

        /// <summary>
        /// 自定义名称
        /// </summary>
        public string? CustomName;

        /// <summary>
        /// 实例ID
        /// </summary>
        public string? InstanceID;

        /// <summary>
        /// 元数据
        /// </summary>
        public Meta? MetaData;

        /// <summary>
        /// 帐号
        /// </summary>
        public string? Account;

        /// <summary>
        /// 客户端类型
        /// </summary>
        public string? ClientType;
    }
}