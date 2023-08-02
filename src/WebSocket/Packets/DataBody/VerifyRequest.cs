using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.WebSocket.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct VerifyRequest
    {
        /// <summary>
        /// 超时
        /// </summary>
        public int Timeout;

        /// <summary>
        /// 盐
        /// </summary>
        public string Salt;

        public VerifyRequest(int timeout, string salt)
        {
            Timeout = timeout;
            Salt = salt;
        }
    }
}