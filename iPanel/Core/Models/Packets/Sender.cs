using iPanel.Core.Models.Client;

namespace iPanel.Core.Models.Packets;

public class Sender
{
    public string? Name { get; init; }

    public string Type { get; init; }

    public string? Address { get; init; }

    public InstanceMetadata? Metadata { get; init; }

    public string? InstanceID { get; init; }

    protected Sender()
    {
        Type = "unknown";
    }

    public Sender(string name, string type, string? address, InstanceMetadata metadata)
    {
        Name = name;
        Type = type;
        Address = address;
        Metadata = metadata;
    }

    public static Sender From(Instance instance) =>
        new()
        {
            Name = instance.CustomName,
            Type = "instance",
            Address = instance.Address,
            InstanceID = instance.InstanceID,
            Metadata = instance.Metadata
        };

    public static Sender FromUser() => new() { Type = "user" };
}
