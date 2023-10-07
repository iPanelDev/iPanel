using System;

namespace iPanelHost.Base.Client;

public class Instance : Client
{
    /// <summary>
    /// 完整信息
    /// </summary>
    public InstanceInfo? Info = new();

    /// <summary>
    /// 自定义名称
    /// </summary>
    public string? CustomName;

    /// <summary>
    /// 实例ID
    /// </summary>
    public string InstanceID;

    /// <summary>
    /// 唯一标识符
    /// </summary>
    public string UUID { get; init; }

    /// <summary>
    /// 实例元数据
    /// </summary>
    public InstanceMetadata? Metadata;

    public Instance(string instanceId, string? uuid)
    {
        InstanceID = instanceId;
        UUID = uuid ?? throw new ArgumentNullException(nameof(uuid));
    }
}
