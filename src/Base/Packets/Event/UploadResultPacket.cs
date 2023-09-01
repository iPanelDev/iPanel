using iPanelHost.Base.Packets.DataBody;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace iPanelHost.Base.Packets.Event;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class UploadResultPacket : SentPacket
{
    public UploadResultPacket(string id, Dictionary<string, FileItemInfo>? files, string speed)
    {
        Data = new UploadResult
        {
            Files = files,
            Speed = speed,
            ID = id
        };
        Type = "event";
        SubType = "upload_result";
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    private class UploadResult
    {
        public Dictionary<string, FileItemInfo>? Files;

        public string? Speed;

        public string? ID;
    }
}
