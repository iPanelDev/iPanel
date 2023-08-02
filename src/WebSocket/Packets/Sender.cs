using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using iPanelHost.WebSocket.Client;

namespace iPanelHost.WebSocket.Packets
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct Sender
    {
        public string? Name;

        public string Type;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Address;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object>? Metadata;

        public Sender(string name, string type, string? address, Dictionary<string, object> metadata)
        {
            Name = name;
            Type = type;
            Address = address;
            Metadata = metadata;
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