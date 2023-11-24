using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Packets.Data;
using iPanel.Utils;
using iPanel.Utils.Json;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using WebSocket4Net;
using Xunit;

namespace iPanel.Tests;

[Collection("IPANEL")]
public class InstanceTests : IDisposable
{
    private readonly IHost _host;

    private const string _wsUrl = "ws://127.0.0.1:30000/ws/instance";
    private const string _password = "114514";

    public InstanceTests()
    {
        _host = new AppBuilder(new() { InstancePassword = _password }).Build();
        _host.StartAsync();
    }

    public void Dispose()
    {
        _host.StopAsync();
        _host.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ShouldBeAbleToConnect()
    {
        var ws = new WebSocket(_wsUrl);
        ws.Opened += (_, _) => Assert.True(true);
        ws.Opened += (_, _) => ws.Close();
        ws.Open();
    }

    [Fact]
    public void ShouldBeAbleToVerify()
    {
        var ws = new WebSocket(_wsUrl);
        var dateTime = DateTime.Now.ToString("o");
        ws.Opened += (_, _) =>
        {
            ws.Send(
                new WsSentPacket(
                    "request",
                    "verify",
                    new VerifyBody
                    {
                        Time = dateTime,
                        MD5 = Encryption.GetMD5($"{dateTime}.{_password}")
                    }
                )
            );
        };
        ws.MessageReceived += (_, e) =>
        {
            var packet = JsonSerializer.Deserialize<WsReceivedPacket>(
                e.Message,
                JsonSerializerOptionsFactory.CamelCase
            );

            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "verify_result");
            Assert.True(packet?.Data?.GetValueKind() == JsonValueKind.Object);
            Assert.True(packet?.Data?["success"]?.GetValueKind() == JsonValueKind.True);
            ws.Close();
        };
        ws.Open();
    }

    [Fact]
    public void ShouldBeNotAbleToVerifyWithoutMD5()
    {
        var ws = new WebSocket(_wsUrl);
        var dateTime = DateTime.Now.ToString("o");
        ws.Opened += (_, _) =>
        {
            ws.Send(new WsSentPacket("request", "verify", new VerifyBody { Time = dateTime, }));
        };
        ws.MessageReceived += (_, e) =>
        {
            var packet = JsonSerializer.Deserialize<WsReceivedPacket>(
                e.Message,
                JsonSerializerOptionsFactory.CamelCase
            );

            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "verify_result");
            Assert.True(packet?.Data?.GetValueKind() == JsonValueKind.Object);
            Assert.True(packet?.Data?["success"]?.GetValueKind() == JsonValueKind.False);
            ws.Close();
        };
        ws.Open();
    }

    [Fact]
    public void ShouldBeNotAbleToVerifyWithoutTime()
    {
        var ws = new WebSocket(_wsUrl);
        var dateTime = DateTime.Now.ToString("o");
        ws.Opened += (_, _) =>
        {
            ws.Send(
                new WsSentPacket(
                    "request",
                    "verify",
                    new VerifyBody { MD5 = Encryption.GetMD5($"{dateTime}.{_password}") }
                )
            );
        };
        ws.MessageReceived += (_, e) =>
        {
            var packet = JsonSerializer.Deserialize<WsReceivedPacket>(
                e.Message,
                JsonSerializerOptionsFactory.CamelCase
            );

            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "verify_result");
            Assert.True(packet?.Data?.GetValueKind() == JsonValueKind.Object);
            Assert.True(packet?.Data?["success"]?.GetValueKind() == JsonValueKind.False);
            ws.Close();
        };
        ws.Open();
    }

    [Fact(Timeout = 7500)]
    public void ShouldBeClosedDueToTimeout()
    {
        var ws = new WebSocket(_wsUrl);
        ws.MessageReceived += (_, e) =>
        {
            var packet = JsonSerializer.Deserialize<WsReceivedPacket>(
                e.Message,
                JsonSerializerOptionsFactory.CamelCase
            );

            Assert.True(packet?.Type == "event");
            Assert.True(packet?.SubType == "verify_result");
            Assert.True(packet?.Data?.GetValueKind() == JsonValueKind.Object);
            Assert.True(packet?.Data?["success"]?.GetValueKind() == JsonValueKind.False);
            ws.Close();
        };
        ws.Open();
    }
}
