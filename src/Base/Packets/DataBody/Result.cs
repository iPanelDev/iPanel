using iPanelHost.Base.Packets.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.DataBody;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Result
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? Success { get; init; }

    /// <summary>
    /// 代码
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? Code { get; init; }

    /// <summary>
    /// 详细原因
    /// </summary>
    public string? Reason { get; init; }

    public Result(string? reason)
    {
        Reason = reason;
        Success = null;
    }

    public Result(string? reason, bool success)
    {
        Reason = reason;
        Success = success;
    }

    public Result(ResultTypes resultTypes)
    {
        Reason = resultTypes.ToString();
        Code = (int)resultTypes;
        Success = resultTypes == ResultTypes.Success;
    }
}
