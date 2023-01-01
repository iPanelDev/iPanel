using Newtonsoft.Json;

namespace iPanel
{
    internal class Info
    {
        [JsonProperty(PropertyName = "server_status")]
        public bool ServerStatus { get; set; } = false;

        [JsonProperty(PropertyName = "server_file")]
        public string Filename { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "server_cpuusage")]
        public string ProcessCPUUsage { get; set; } = string.Empty;

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

        [JsonProperty(PropertyName = "ram_usage")]
        public string RAMUsage { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "cpu_usage")]
        public string CPUUsage { get; set; } = string.Empty;
    }
}
