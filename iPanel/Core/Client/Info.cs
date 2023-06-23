using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Client
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct Info
    {
        public bool ServerStatus = false;

        public string Filename = string.Empty;

        public string ProcessCPUUsage = string.Empty;

        public string Time = string.Empty;

        public string OS = string.Empty;

        public string CPUName = string.Empty;

        public string TotalRAM = string.Empty;

        public string UsedRAM = string.Empty;

        public string RAMUsage = string.Empty;

        public string CPUUsage = string.Empty;

        public Info() { }
    }
}
