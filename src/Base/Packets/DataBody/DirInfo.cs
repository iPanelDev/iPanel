using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class DirInfo
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Item[]? Items;

    public bool IsExist;

    public string? Dir;

    private static readonly string[] _childrenTypes = { "file", "dir" };

    public DirInfo()
    {
        Items = Items?
            .Where((item) => !string.IsNullOrEmpty(item?.Type) && _childrenTypes.Contains(item?.Type ?? string.Empty))
            .ToArray();
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Item
    {
        public string? Type;

        public string? Path;

        public string? Name;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Size;
    }
}
