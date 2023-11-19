using iPanel.Core.Models.Client.Infos;

namespace iPanel.Core.Models.Client;

public class Instance : Client
{
    public InstanceInfo? Info { get; set; } = new();

    public string? CustomName { get; set; }

    public string InstanceId { get; set; }

    public Metadata Metadata { get; set; }

    public Instance(string instanceId)
    {
        InstanceId = instanceId;
        Metadata ??= new();
    }
}
