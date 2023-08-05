using iPanelHost.WebSocket.Client.Info;
using Newtonsoft.Json;

namespace iPanelHost.WebSocket.Client
{
    internal class Instance : Client
    {
        [JsonIgnore]
        public FullInfo FullInfo;

        /// <summary>
        /// 短信息
        /// </summary>
        public ShortInfo ShortInfo => new(FullInfo);

        /// <summary>
        /// 自定义名称
        /// </summary>
        public string? CustomName;

        [JsonIgnore]
        public new ClientType Type => ClientType.Instance;

        public Instance(string? guid) : base(guid)
        { }
    }
}