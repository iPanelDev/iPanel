using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Client;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class InstanceInfo
{
    public DateTime UpdateTime = DateTime.Now;

    public SysInfo Sys { get; init; } = new();

    public ServerInfo Server { get; init; } = new();

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SysInfo
    {
        public string? OS { get; init; }

        public string? CPUName { get; init; }

        public long TotalRAM { get; init; }

        public long FreeRAM { get; init; }

        public double RAMUsage => (1 - (double)FreeRAM / TotalRAM) * 100;

        public double CPUUsage { get; init; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ServerInfo
    {
        public string? Filename { get; init; }

        public bool Status { get; init; }

        public string? RunTime { get; init; }

        public double Usage { get; init; }
    }
}
