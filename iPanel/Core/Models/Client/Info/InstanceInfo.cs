using System;
using System.Text.Json.Serialization;

namespace iPanel.Core.Models.Client.Infos;

public class InstanceInfo
{
    public DateTime UpdateTime { get; } = DateTime.Now;

    [JsonRequired]
    public SystemInfo Sys { get; init; } = new();

    [JsonRequired]
    public ServerInfo Server { get; init; } = new();
}
