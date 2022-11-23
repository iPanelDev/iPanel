using Newtonsoft.Json;

namespace iPanel
{
    internal class Info
    {
        [JsonProperty(PropertyName = "guid")]
        public string GUID { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "server_status")]
        public bool ServerStatus { get; set; } = false;
        [JsonProperty(PropertyName = "server_file")]
        public string Filename { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "server_cpuperc")]
        public string ProcessCPUPercentage { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "server_time")]
        public string Time { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "os")]
        public string OS { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "cpu")]
        public string CPUName { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "ram_total")]
        public string TotalRAM { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "ram_used")]
        public string UsedRAM { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "ram_perc")]
        public string RAMPercentage { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "cpu_perc")]
        public string CPUPercentage { get; set; } = string.Empty;
    }
}
