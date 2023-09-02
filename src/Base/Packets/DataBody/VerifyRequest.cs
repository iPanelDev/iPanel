using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class VerifyRequest
{
    /// <summary>
    /// 超时
    /// </summary>
    public readonly int Timeout;

    /// <summary>
    /// 唯一标识ID
    /// </summary>
    public readonly string UUID;

    /// <summary>
    /// 当前版本
    /// </summary>
    [JsonProperty]
    public static string Version => Constant.VERSION;

    /// <summary>
    /// 内部版本号
    /// </summary>
    [JsonProperty]
    public static int InternalVersion => Constant.InternalVersion;

    public VerifyRequest(int timeout, string uuid)
    {
        Timeout = timeout;
        UUID = uuid;
    }
}
