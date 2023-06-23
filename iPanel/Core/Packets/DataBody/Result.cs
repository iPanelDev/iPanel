using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanel.Core.Packets.DataBody
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal struct Result
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success;

        /// <summary>
        /// 原因
        /// </summary>
        public string? Reason;

        public Result(bool success, string? reason)
        {
            Success = success;
            Reason = reason;
        }
    }
}