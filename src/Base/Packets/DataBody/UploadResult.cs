using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class UploadResult
{
    public Dictionary<string, FileItemInfo>? Files;

    public string? Speed;

    public string? ID;
}
