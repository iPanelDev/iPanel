using iPanelHost.Base.Client.Info;
using Newtonsoft.Json;

namespace iPanelHost.Base.Client;

public class Instance : Client
{
    /// <summary>
    /// 完整信息
    /// </summary>
    [JsonIgnore]
    public FullInfo? FullInfo;

    /// <summary>
    /// 短信息
    /// </summary>
    public ShortInfo ShortInfo => new(FullInfo);

    /// <summary>
    /// 自定义名称
    /// </summary>
    public string? CustomName;

    /// <summary>
    /// 实例ID
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? InstanceID;

    /// <summary>
    /// 实例元数据
    /// </summary>
    public Meta? Metadata;

    public Instance(string? uuid)
        : base(uuid) { }
}
