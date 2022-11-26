using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace iPanel
{
    internal class Packet
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "sub_type")]
        public string SubType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; } = null;

        [JsonProperty(PropertyName = "sender", NullValueHandling = NullValueHandling.Ignore)]
        public object Sender { get; set; } = null;

        [JsonProperty(PropertyName = "custom_name", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomName { get; set; } = null;

        [JsonProperty(PropertyName = "time")]
        public long Time { get; set; } = 0;

        [JsonProperty(PropertyName = "info", NullValueHandling = NullValueHandling.Ignore)]
        public Info Info { get; set; } = null;

        public Packet(string type, string sub_type, object data = null, object sender = null)
        {
            Type = type;
            SubType = sub_type;
            Data = data;
            Sender = (sender ?? string.Empty).ToString() == "iPanel" ?
                new Dictionary<string, string>(){
                    {"guid",string.Empty },
                    {"type","iPanel" }} :
                    sender;
            Time = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
