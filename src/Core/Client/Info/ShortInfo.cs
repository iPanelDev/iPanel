using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Client.Info
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct ShortInfo
    {
        public bool ServerStatus;

        public string? ServerFilename;

        public string? ServerTime;

        public string? OS;

        public ShortInfo(FullInfo fullInfo)
        {
            ServerStatus = fullInfo.Server.Status;
            ServerFilename = fullInfo.Server.Filename;
            ServerTime = fullInfo.Server.RunTime;
            OS = fullInfo.Sys.OS;
        }
    }
}
