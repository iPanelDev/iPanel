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
        /// 盐
        /// </summary>
        public string Salt;

#pragma warning disable CA1822
        /// <summary>
        /// 当前版本
        /// </summary>
        public string Version => Constant.VERSION;

        /// <summary>
        /// 内部版本号
        /// </summary>
        public int InternalVersion => Constant.InternalVersion;
#pragma warning restore CA1822

        public VerifyRequest(int timeout, string salt)
        {
            Timeout = timeout;
            Salt = salt;
        }
    }
}