using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct VerifyRequest
    {
        /// <summary>
        /// 超时
        /// </summary>
        public int Timeout;

        /// <summary>
        /// 随机键值
        /// </summary>
        public string RandomKey;

        public VerifyRequest(int timeout, string randomKey)
        {
            Timeout = timeout;
            RandomKey = randomKey;
        }
    }
}