using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct VerifyRequest
    {
        public int Timeout;

        public string RandomKey;

        public VerifyRequest(int timeout, string randomKey)
        {
            Timeout = timeout;
            RandomKey = randomKey;
        }
    }
}