using System;
using System.Timers;
using iPanelHost.Base;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Server;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;
using Xunit;
using Xunit.Abstractions;

namespace iPanelHost.Tests;

[Collection("Service")]
public class WS : IDisposable
{
    private readonly WebSocket _webSocket;

    private readonly ITestOutputHelper _outputHelper;

    private const string _password = "123456";

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        HttpServer.Stop();
    }

    public WS(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;

        Program.ReadSetting(new Setting { InstancePassword = _password });
        HttpServer.Start();

        _webSocket = new("ws://127.0.0.1:30000");
        _webSocket.Opened += (_, _) => _outputHelper.WriteLine("Opened");
        _webSocket.Closed += (_, _) => _outputHelper.WriteLine("Closed");
        _webSocket.MessageReceived += (_, e) => _outputHelper.WriteLine("Received: " + e.Message);
        _webSocket.Error += (_, e) => throw new Exception(e.Exception.ToString());
    }

    /// <summary>
    /// 连接
    /// </summary>
    [Fact(Timeout = 20000)]
    public void ShouldBeAbleToConnect()
    {
        _webSocket.Opened += (_, _) =>
        {
            Assert.True(true);
            _webSocket.Close();
        };
        _webSocket.Open();
    }

    /// <summary>
    /// 接收验证数据包
    /// </summary>
    [Fact(Timeout = 20000)]
    public void ShouldReceiveVerificationPacket()
    {
        ReceivedPacket? packet = null;
        _webSocket.MessageReceived += (_, e) =>
        {
            packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
            _webSocket.Close();
        };
        _webSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "request");
            Assert.True(packet?.SubType == "verify_request");
            Assert.True(packet?.Data?["timeout"]?.Type == JTokenType.Integer);
            Assert.Matches(@"^\w{32}$", packet?.Data?["uuid"]?.ToString()!);
        };
        _webSocket.Open();
    }

    /// <summary>
    /// 因超时而关闭
    /// </summary>
    [Fact(Timeout = 20000)]
    public void ShouldBeClosedByHostDueToTimeout()
    {
        ReceivedPacket? packet = null;
        _webSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "disconnection");
            Assert.True(packet?.Data?.Type == JTokenType.Object);
            Assert.Equal(ResultTypes.TimeoutInVerification.ToString(), packet?.Data?.ToString());
        };
        _webSocket.Open();

        Timer timer = new(6000) { AutoReset = false };
        timer.Elapsed += (_, _) => Assert.True(_webSocket.State == WebSocketState.Closed);
        timer.Elapsed += (_, _) => timer.Dispose();
        timer.Start();
    }

    /// <summary>
    /// 因无效数据包而关闭
    /// </summary>
    [Fact(Timeout = 20000)]
    public void ShouldBeClosedBueToInvalidPacket()
    {
        ReceivedPacket? packet = null;
        _webSocket.MessageReceived += (_, e) =>
        {
            if (packet is null)
            {
                _webSocket.Send("666");
            }

            packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
        };
        _webSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "disconnection");
            Assert.True(packet?.Data?.Type == JTokenType.Object);
            Assert.Equal(ResultTypes.NotVerifyYet.ToString(), packet?.Data?.ToString());
        };

        _webSocket.Open();
    }

    /// <summary>
    /// 因验证失败而关闭
    /// </summary>
    [Fact(Timeout = 20000)]
    public void ShouldBeClosedDueToVerifyFailure()
    {
        ReceivedPacket? packet = null;
        _webSocket.MessageReceived += (_, e) =>
        {
            packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
            if (packet?.SubType == "verify_request")
            {
                _webSocket.Send(
                    new SentPacket
                    {
                        Type = "request",
                        SubType = "verify",
                        Data = new JObject { { "token", "114514" } }
                    }.ToString()
                );
            }
        };
        _webSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "disconnection");
            Assert.True(packet?.Data?.Type == JTokenType.Object);
            Assert.Equal(ResultTypes.FailToVerify.ToString(), packet?.Data?.ToString());
        };

        _webSocket.Open();
    }

    /// <summary>
    /// 因验证失败而关闭
    /// </summary>
    [Fact(Timeout = 20000)]
    public void ShouldBeAbleToVerifySuccessfully()
    {
        ReceivedPacket? packet = null;
        _webSocket.MessageReceived += (_, e) =>
        {
            packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
            if (packet?.SubType == "verify_request")
            {
                VerifyRequest verifyRequest = packet.Data!.ToObject<VerifyRequest>()!;
                _webSocket.Send(
                    new SentPacket
                    {
                        Type = "request",
                        SubType = "verify",
                        Data = new VerifyBody
                        {
                            InstanceID = Guid.NewGuid().ToString("N"),
                            Token = General.GetMD5(verifyRequest.UUID + _password)
                        }
                    }.ToString()
                );
            }
            else
            {
                _webSocket.Close();
            }
        };
        _webSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "verify_result");
            Assert.True(packet?.Data?.Type == JTokenType.Object);
            Assert.True(packet?.Data?["success"]?.Type == JTokenType.Boolean);
            Assert.True(((bool?)packet?.Data?["success"]) ?? false);
        };

        _webSocket.Open();
    }
}
