using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct VerifyBody
    {
        public string MD5;

        public string CustomName;

        public string ClientType;
    }
}