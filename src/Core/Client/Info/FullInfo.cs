using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Client.Info
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct FullInfo
    {
        public SysInfo Sys;

        public ServerInfo Server;

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public struct SysInfo
        {
            public string? OS;

            public string? CPUName;

            public long TotalRAM;

            public long FreeRAM;

            public double RAMUsage => (1 - (double)FreeRAM / TotalRAM) * 100;

            public double CPUUsage;
        }

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public struct ServerInfo
        {
            public string? Filename;

            public bool Status;

            public string? RunTime;

            public double Usage;
        }
    }
}
