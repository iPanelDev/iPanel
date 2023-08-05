using EmbedIO.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.WebSocket.Client
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal abstract class Client
    {
        /// <summary>
        /// 连接对象
        /// </summary>
        [JsonIgnore]
        public IWebSocketContext? Context;

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address => Context?.RemoteEndPoint.ToString();

        [JsonIgnore]
        public DateTime LastTime;

        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string GUID { init; get; }

        /// <summary>
        /// 类型
        /// </summary>
        [JsonIgnore]
        public ClientType Type;

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close() => Context?.Close();

        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="text">发送内容</param>
        public void Send(string text)
        {
            if (Context is not null)
            {
                Context.Send(text);
            }
        }

        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="text">发送内容</param>
        public void Send(object obj)
        {
            Send(JsonConvert.SerializeObject(obj));
        }

        internal enum ClientType
        {
            Unknown,
            Instance,
            Console
        }

        protected Client()
        {
            if (GUID is null)
            {
                throw new InvalidOperationException();
            }
        }

        protected Client(string? guid)
        {
            GUID = guid ?? throw new ArgumentNullException(nameof(guid));
        }
    }
}
