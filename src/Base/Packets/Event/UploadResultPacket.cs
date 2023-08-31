using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.Event
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class UploadResultPacket : SentPacket
    {
        public UploadResultPacket(string id, Dictionary<string, FileInfo>? files, string speed)
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
            public Dictionary<string, FileInfo>? Files;

            public string? Speed;

            public string? ID;
        }

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public class FileInfo
        {
            public FileInfo(string md5, long length)
            {
                MD5 = md5;
                Length = length;
            }

            [JsonProperty("md5")]
            public string MD5;

            public long Length;
        }
    }
}