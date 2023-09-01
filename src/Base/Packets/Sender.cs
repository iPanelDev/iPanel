using iPanelHost.Base.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Sender
{
    public string? Name { init; get; }

    public string Type { init; get; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Address { init; get; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Meta? Metadata { init; get; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? UUID { init; get; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? InstanceID { init; get; }

    protected Sender()
    {
        Type = "unknown";
    }

    public Sender(string name, string type, string? address, Meta metadata)
    {
        Name = name;
        Type = type;
        Address = address;
        Metadata = metadata;
    }

    /// <summary>
    /// 作为发送者
    /// </summary>
    public static Sender From(Instance instance)
        => new()
        {
            Name = instance.CustomName,
            Type = "instance",
            Address = instance.Address,
            InstanceID = instance.InstanceID,
            UUID = instance.UUID,
            Metadata = instance.Metadata
        };

    /// <summary>
    /// 作为发送者
    /// </summary>
    public static Sender From(Console console)
        => new()
        {
            Type = "console",
            Address = console.Address,
            UUID = console.UUID,
        };
}
