using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class FileItemInfo
{
    public FileItemInfo(string md5, long length)
    {
        MD5 = md5;
        Length = length;
    }

    [JsonProperty("md5")]
    public string MD5 { get; init; }

    public long Length { get; init; }

    public readonly DateTime Expires = DateTime.Now.AddMinutes(30);

    public string? User { get; init; }
}
