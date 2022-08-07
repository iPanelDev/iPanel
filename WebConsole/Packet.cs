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

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "from")]
        public string From { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "time")]
        public long Time { get; set; } = 0;

        public Packet(string type = "", string sub_type = "", string data = "", string from = "",string target = "")
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
