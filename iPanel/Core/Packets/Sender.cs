using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Packets
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct Sender
    {
        public string? Name;

        public string Type;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Address;

        public Sender(string name, string type, string? address)
        {
            Name = name;
            Type = type;
            Address = address;
        }

        /// <summary>
        /// 作为发送者
        /// </summary>
        public static Sender From(Client.Client client)
            => new Sender()
            {
                Name = client.CustomName,
                Type = client.Type.ToString().ToLowerInvariant(),
                Address = client.Address
            };
    }
}