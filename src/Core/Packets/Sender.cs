using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using iPanel.Core.Client;

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
        public static Sender From(Instance instance)
            => new Sender()
            {
                Name = instance.CustomName,
                Type = instance.Type.ToString().ToLowerInvariant(),
                Address = instance.Address
            };

        /// <summary>
        /// 作为发送者
        /// </summary>
        public static Sender From(Console console)
            => new Sender()
            {
                Name = console.CustomName,
                Type = console.Type.ToString().ToLowerInvariant(),
                Address = console.Address
            };
    }
}