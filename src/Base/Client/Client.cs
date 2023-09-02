using EmbedIO.WebSockets;
using iPanelHost.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.Base.Client;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public abstract class Client
{
    /// <summary>
    /// 连接对象
    /// </summary>
    [JsonIgnore]
    public IWebSocketContext? Context { get; init; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address => Context?.RemoteEndPoint.ToString();

    /// <summary>
    /// 唯一标识符
    /// </summary>
    public string UUID { init; get; }

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
        Context?.Send(text);
    }

    /// <summary>
    /// 发送文本
    /// </summary>
    /// <param name="text">发送内容</param>
    public void Send(object obj)
    {
        Send(JsonConvert.SerializeObject(obj));
    }

    protected Client()
    {
        if (UUID is null)
        {
            throw new InvalidOperationException();
        }
    }

    protected Client(string? uuid)
    {
        UUID = uuid ?? throw new ArgumentNullException(nameof(uuid));
    }
}
