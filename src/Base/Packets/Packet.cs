using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public abstract class Packet
{
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { init; get; } = string.Empty;

    /// <summary>
    /// 子类型
    /// </summary>
    public string SubType { init; get; } = string.Empty;

    public override string ToString() => JsonConvert.SerializeObject(this);
}
