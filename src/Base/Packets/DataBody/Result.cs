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
    public readonly bool? Success;

    /// <summary>
    /// 代码
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly int? Code;

    /// <summary>
    /// 详细原因
    /// </summary>
    public readonly string? Reason;

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

    public const string DataAnomaly = "数据异常",
        DuplicateInstanceID = "实例ID重复",
        EmptyAccount = "帐号为空",
        ErrorWhenGettingPacketContent = "获取验证内容时异常",
        FailToVerify = "验证失败",
        IncorrectAccountOrPassword = "帐号或密码错误",
        IncorrectClientType = "客户端类型错误",
        IncorrectInstanceID = "实例ID错误",
        InternalDataError = "内部数据错误",
        InvalidTarget = "订阅目标无效",
        NotVerifyYet = "你还未通过验证",
        TimeoutInVerification = "验证超时";
}
