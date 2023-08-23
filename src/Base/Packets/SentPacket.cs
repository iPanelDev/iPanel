using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.Base.Packets
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class SentPacket : Packet
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public object? Data { init; get; }

        public long Time { init; get; }

        private static readonly Sender _selfSender = new(
            $"iPanel Host",
            "host",
            null,
            new()
            {
                Version = Constant.VERSION
            }
            );

        /// <summary>
        /// 发送者
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Sender? Sender { init; get; }

        protected SentPacket() { }

        public SentPacket(string type, string sub_type, object? data = null) : this(type, sub_type, data, _selfSender)
        { }

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