using iPanel.Core.Models.Client;

namespace iPanel.Core.Models.Packets;

public class Sender
{
    public string? Name { get; init; }

    public string Type { get; init; }

    public string? Address { get; init; }

    public Metadata? Metadata { get; init; }

    public string? InstanceId { get; init; }

    protected Sender()
    {
        Type = "unknown";
    }

    public Sender(string name, string type, string? address, Metadata metadata)
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
            InstanceId = instance.InstanceId,
            Metadata = instance.Metadata
        };

    public static Sender CreateUserSender(string? userName) =>
        new() { Type = "user", Name = userName };
}
