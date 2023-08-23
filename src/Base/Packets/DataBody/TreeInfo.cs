using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.Base.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class TreeInfo
    {
        public string[] Children;

        public string Location = string.Empty;

        public TreeInfo()
        {
            Children ??= Array.Empty<string>();
        }
    }
}