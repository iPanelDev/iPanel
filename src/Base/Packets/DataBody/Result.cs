using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class Result
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success;

        /// <summary>
        /// 详细原因
        /// </summary>
        public string? Reason;

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

        public const string
           FailToVerify = "验证失败",
           NotVerifyYet = "你还未通过验证",
           InvalidTarget = "订阅目标无效",
           IncorrectAccountOrPassword = "帐号或密码错误",
           EmptyAccount = "帐号为空",
           InternalDataError = "内部数据错误",
           ErrorWhenGettingPacketContent = "获取验证内容时异常",
           DataAnomaly = "数据异常",
           IncorrectClientType = "客户端类型错误",
           TimeoutInVerification = "验证超时";
    }
}