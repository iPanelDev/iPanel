using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanel.Core.Packets
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal sealed class SentPacket : Packet
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public readonly object? Data;

        public long Time;

        private static readonly Sender SelfSender = new($"iPanel_{Program.VERSION}", "host", null);

        public SentPacket(string type, string sub_type, object? data = null)
        {
            Type = type;
            SubType = sub_type;
            Data = data;
            Sender = SelfSender;
            Time = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public SentPacket(string type, string sub_type, object? data, Sender sender)
        {
            Type = type;
            SubType = sub_type;
            Data = data;
            Sender = sender;
            Time = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}