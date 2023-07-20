using Fleck;
using iPanel.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace iPanel.Core.Client
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal abstract class Client
    {
        /// <summary>
        /// 连接对象
        /// </summary>
        [JsonIgnore]
        public IWebSocketConnection? WebSocketConnection;

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address => WebSocketConnection?.GetFullAddr();

        [JsonIgnore]
        public DateTime LastTime;

        /// <summary>
        /// 自定义名称
        /// </summary>
        public string? CustomName;

        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string GUID { init; get; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 类型
        /// </summary>
        [JsonIgnore]
        public ClientType Type;

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close() => WebSocketConnection?.Close();

        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="text">发送内容</param>
        public async Task Send(string text)
        {
            if (WebSocketConnection is not null)
            {
                await WebSocketConnection.Send(text);
            }
        }

        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="text">发送内容</param>
        public async Task Send(object obj)
        {
            await Send(JsonConvert.SerializeObject(obj));
        }

        internal enum ClientType
        {
            Unknown,
            Instance,
            Console
        }
    }
}
