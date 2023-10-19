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
    /// 实例元数据
    /// </summary>
    public InstanceMetadata Metadata;

    public Instance(string instanceId)
    {
        InstanceID = instanceId;
        Metadata ??= new();
    }
}
