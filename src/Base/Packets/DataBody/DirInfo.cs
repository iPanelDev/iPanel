using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class DirInfo
{
    /// <summary>
    /// 子文件夹
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public FileItem[] Items { get; init; }

    /// <summary>
    /// 是否存在
    /// </summary>
    public bool IsExist { get; init; }

    /// <summary>
    /// 当前路径
    /// </summary>
    public string? Path { get; init; }

    private static readonly string[] _childrenTypes = { "file", "dir" };

    public DirInfo()
    {
        Items =
            Items
                ?.Where(
                    (item) =>
                        !string.IsNullOrEmpty(item?.Type)
                        && _childrenTypes.Contains(item?.Type ?? string.Empty)
                )
                .ToArray() ?? Array.Empty<FileItem>();
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class FileItem
    {
        public string? Type { get; init; }

        public string? Path { get; init; }

        public string? Name { get; init; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; init; }
    }
}
