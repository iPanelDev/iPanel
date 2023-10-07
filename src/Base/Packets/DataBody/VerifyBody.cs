using iPanelHost.Base.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class VerifyBody
{
    [JsonProperty("uuid")]
    public string? UUID { get; init; }

    public string? Token { get; init; }

    /// <summary>
    /// 自定义名称
    /// </summary>
    public string? CustomName { get; init; }

    /// <summary>
    /// 实例ID
    /// </summary>
    public string? InstanceID { get; init; }

    /// <summary>
    /// 元数据
    /// </summary>
    public InstanceMetadata? Metadata { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; init; }
}
