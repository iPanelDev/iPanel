using Newtonsoft.Json;
using System;

namespace WebConsole
{
    internal class Packet
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "sub_type")]
        public string SubType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; } = null;

        [JsonProperty(PropertyName = "from", NullValueHandling = NullValueHandling.Ignore)]
        public string From { get; set; } = null;

        [JsonProperty(PropertyName = "target", NullValueHandling = NullValueHandling.Ignore)]
        public string Target { get; set; } = null;
        [JsonProperty(PropertyName = "time")]
        public long Time { get; set; } = 0;

        [JsonProperty(PropertyName = "info", NullValueHandling = NullValueHandling.Ignore)]
        public Info Info { get; set; } = null;

        public Packet(string type = "", string sub_type = "", object data = null, string from = null,string target = null)
        {
            Type = type;
            SubType = sub_type;
            Data = data;
            From = from;
            Target = target;
            Time = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
