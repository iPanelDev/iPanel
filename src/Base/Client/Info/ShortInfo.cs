using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Client.Info;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ShortInfo
{
    public readonly bool ServerStatus;

    public readonly string? ServerFilename;

    public readonly string? ServerTime;

    public readonly string? OS;

    public ShortInfo(FullInfo? fullInfo)
    {
        if (fullInfo is null)
        {
            return;
        }
        ServerStatus = fullInfo.Server.Status;
        ServerFilename = fullInfo.Server.Filename;
        ServerTime = fullInfo.Server.RunTime;
        OS = fullInfo.Sys.OS;
    }
}
