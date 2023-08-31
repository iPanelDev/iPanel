using iPanelHost.Base;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;
using WebSocket4Net;
using Xunit;
using Xunit.Abstractions;

namespace iPanelHost.Tests;

public class WS : IDisposable
{
    public WebSocket4Net.WebSocket WebSocket;

    private readonly ITestOutputHelper _outputHelper;

    private const string _password = "123456";

    public void Dispose()
    {
        HttpServer.Stop();
    }

    public WS(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;

        File.WriteAllText("setting.json", JsonConvert.SerializeObject(new Setting { InstancePassword = _password }));
        Program.ReadSetting();
        HttpServer.Start();

        WebSocket = new("ws://127.0.0.1:30000");
        WebSocket.Opened += (_, _) => _outputHelper.WriteLine("Opened");
        WebSocket.Closed += (_, _) => _outputHelper.WriteLine("Closed");
        WebSocket.MessageReceived += (_, e) => _outputHelper.WriteLine("Received: " + e.Message);
        WebSocket.Error += (_, e) => throw new Exception(e.Exception.ToString());
    }

    /// <summary>
    /// 连接
    /// </summary>
    [Fact]
    public void ShouldBeAbleToConnect()
    {
        WebSocket.Opened += (_, _) =>
        {
            Assert.True(true);
            WebSocket.Close();
        };
        WebSocket.Open();
    }

    /// <summary>
    /// 接收验证数据包
    /// </summary>
    [Fact(Timeout = 5000)]
    public void ShouldReceiveVerificationPacket()
    {
        ReceivedPacket? packet = null;
        WebSocket.MessageReceived += (_, e) =>
        {
            packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
            WebSocket.Close();
        };
        WebSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "request");
            Assert.True(packet?.SubType == "verify_request");
            Assert.True(packet?.Data?["timeout"]?.Type == JTokenType.Integer);
            Assert.Matches(@"^\w{32}$", packet?.Data?["uuid"]?.ToString()!);
        };
        WebSocket.Open();
    }

    /// <summary>
    /// 因超时而关闭
    /// </summary>
    [Fact(Timeout = 10000)]
    public void ShouldBeClosedByHostDueToTimeout()
    {
        ReceivedPacket? packet = null;
        WebSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "disconnection");
            Assert.True(packet?.Data?.Type == JTokenType.Object);
            Assert.True(packet?.Data?.ToString()?.Contains(Result.TimeoutInVerification));
        };
        WebSocket.Open();

        Timer timer = new(6000) { AutoReset = false };
        timer.Elapsed += (_, _) => Assert.True(WebSocket.State == WebSocketState.Closed);
        timer.Elapsed += (_, _) => timer.Dispose();
        timer.Start();
    }

    /// <summary>
    /// 因无效数据包而关闭
    /// </summary>
    [Fact(Timeout = 10000)]
    public void ShouldBeClosedBueToInvalidPacket()
    {
        ReceivedPacket? packet = null;
        WebSocket.MessageReceived += (_, e) =>
        {
            if (packet is null)
            {
                WebSocket.Send("666");
            }

            packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
        };
        WebSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "disconnection");
            Assert.True(packet?.Data?.Type == JTokenType.Object);
            Assert.True(packet?.Data?.ToString()?.Contains(Result.NotVerifyYet));
        };

        WebSocket.Open();

    }

    /// <summary>
    /// 因验证失败而关闭
    /// </summary>
    [Fact(Timeout = 10000)]
    public void ShouldBeClosedDueToVerifyFailure()
    {
        ReceivedPacket? packet = null;
        WebSocket.MessageReceived += (_, e) =>
        {
            packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
            if (packet?.SubType == "verify_request")
            {
                WebSocket.Send(
                    new SentPacket()
                    {
                        Type = "request",
                        SubType = "verify",
                        Data = new JObject
                        {
                            { "token", "114514" }
                        }
                    }.ToString()
                );
            }
        };
        WebSocket.Closed += (_, _) =>
        {
            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "disconnection");
            Assert.True(packet?.Data?.Type == JTokenType.Object);
            Assert.True(packet?.Data?.ToString()?.Contains(Result.FailToVerify));
        };

        WebSocket.Open();
    }
}