using iPanel.Core.Models.Client.Infos;

namespace iPanel.Core.Models.Client;

public class Instance : Client
{
    public InstanceInfo? Info { get; set; } = new();

    public string? CustomName { get; set; }

    public string InstanceID { get; set; }

    public InstanceMetadata Metadata { get; set; }

    public Instance(string instanceId)
    {
        InstanceID = instanceId;
        Metadata ??= new();
    }
}
