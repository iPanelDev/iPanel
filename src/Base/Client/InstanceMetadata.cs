using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Client;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class InstanceMetadata
{
    /// <summary>
    /// 版本
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; init; }
}
