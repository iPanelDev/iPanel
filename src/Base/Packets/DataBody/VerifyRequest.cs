using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class VerifyRequest
    {
        /// <summary>
        /// 超时
        /// </summary>
        public int Timeout;

        /// <summary>
        /// 唯一标识ID
        /// </summary>
        public string UUID;

        /// <summary>
        /// 当前版本
        /// </summary>
        public string Version => Constant.VERSION;

        /// <summary>
        /// 内部版本号
        /// </summary>
        public int InternalVersion => Constant.InternalVersion;

        public VerifyRequest(int timeout, string uuid)
        {
            Timeout = timeout;
            UUID = uuid;
        }
    }
}