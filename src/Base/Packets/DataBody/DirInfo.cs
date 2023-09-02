using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class DirInfo
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Item[]? Items { get; init; }

    public bool IsExist { get; init; }

    public string? Dir { get; init; }

    private static readonly string[] _childrenTypes = { "file", "dir" };

    public DirInfo()
    {
        Items = Items
            ?.Where(
                (item) =>
                    !string.IsNullOrEmpty(item?.Type)
                    && _childrenTypes.Contains(item?.Type ?? string.Empty)
            )
            .ToArray();
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Item
    {
        public string? Type { get; init; }

        public string? Path { get; init; }

        public string? Name { get; init; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; init; }
    }
}
