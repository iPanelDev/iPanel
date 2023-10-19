using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Client;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class InstanceInfo
{
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime = DateTime.Now;

    public SysInfo Sys { get; init; } = new();

    public ServerInfo Server { get; init; } = new();

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SysInfo
    {
        /// <summary>
        /// 系统
        /// </summary>
        public string? OS { get; init; }

        /// <summary>
        /// CPU名称
        /// </summary>
        public string? CPUName { get; init; }

        /// <summary>
        /// 总内存
        /// </summary>
        public long TotalRAM { get; init; }

        /// <summary>
        /// 可用内存
        /// </summary>
        public long FreeRAM { get; init; }

        /// <summary>
        /// 内存使用率
        /// </summary>
        public double RAMUsage => TotalRAM == 0 ? 0 : (1 - (double)FreeRAM / TotalRAM) * 100;

        /// <summary>
        /// CPU占用率
        /// </summary>
        public double CPUUsage { get; init; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ServerInfo
    {
        /// <summary>
        /// 启动文件
        /// </summary>
        public string? Filename { get; init; }

        /// <summary>
        /// 服务器状态
        /// </summary>
        public bool Status { get; init; }

        /// <summary>
        /// 运行时间
        /// </summary>
        public string? RunTime { get; init; }

        /// <summary>
        /// 进程CPU占用率
        /// </summary>
        public double Usage { get; init; }
    }
}
