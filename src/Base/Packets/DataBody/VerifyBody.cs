using iPanelHost.Base.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

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
    public Meta? MetaData { get; init; }

    /// <summary>
    /// 帐号
    /// </summary>
    public string? Account { get; init; }

    /// <summary>
    /// 客户端类型
    /// </summary>
    public string? ClientType { get; init; }
}
