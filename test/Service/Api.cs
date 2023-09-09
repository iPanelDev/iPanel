using iPanelHost.Base;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Server;
using iPanelHost.Service;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebSocket4Net;
using Xunit;

namespace iPanelHost.Tests;

[Collection("Service")]
public class Api : IDisposable
{
    private readonly HttpClient _httpClient = new(new HttpClientHandler { UseCookies = true });

    private const string _password = "123456";

    private const string _baseRoot = "http://127.0.0.1:30000";

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
        HttpServer.Stop();
    }

    public Api()
    {
        _httpClient.Timeout = new(0, 0, 10);

        UserManager.Add("test", new() { Password = "114514" });
        Program.ReadSetting(new Setting { InstancePassword = _password });
        HttpServer.Start();
    }

    /// <summary>
    /// 应返回当前日期
    /// </summary>
    [Fact]
    public async Task ShouldBeAbleToReturnDateTime()
    {
        HttpResponseMessage responseMessage = await _httpClient.GetAsync($"{_baseRoot}/api/ping");
        DateTime dateTime = DateTime.Parse(await responseMessage.Content.ReadAsStringAsync());

        Assert.True((DateTime.Now - dateTime).TotalSeconds < 5);
        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
    }

    /// <summary>
    /// 在空的参数时应验证失败
    /// </summary>
    [Fact]
    public async Task ShouldBeNotAbleToVerifyWithEmptyInput()
    {
        HttpResponseMessage responseMessage = await _httpClient.GetAsync($"{_baseRoot}/api/verify");
        Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
    }

    /// <summary>
    /// 在输入错误的参数时应验证失败
    /// </summary>
    [Theory]
    [InlineData("uuid=114514")]
    [InlineData("user=114514")]
    [InlineData("token=114514")]
    [InlineData("uuid=114514&user=114514&token=114514")]
    public async Task ShouldBeNotAbleToVerifyWithWrongArgs(string arg)
    {
        HttpResponseMessage responseMessage = await _httpClient.GetAsync(
            $"{_baseRoot}/api/verify?{arg}"
        );
        Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
    }

    /// <summary>
    /// 验证
    /// </summary>
    [Fact]
    public void ShouldBeAbleToVerify()
    {
        WebSocket webSocket = Utils.CreateConsoleWebSocket("test", "114514");
        webSocket.MessageReceived += Handle;
        webSocket.Open();

        async void Handle(object? sender, MessageReceivedEventArgs e)
        {
            await Task.Delay(1000);
            ReceivedPacket? packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
            if (packet?.SubType == "verify_request")
            {
                VerifyRequest verifyRequest = packet.Data!.ToObject<VerifyRequest>()!;
                string token = General.GetMD5(verifyRequest.UUID + "test" + "114514");

                HttpResponseMessage responseMessage = await _httpClient.GetAsync(
                    $"{_baseRoot}/api/verify?token={token}&user=test&uuid={verifyRequest.UUID}"
                );

                string? cookie = responseMessage.Headers.GetValues("Set-Cookie").FirstOrDefault();
                Assert.False(string.IsNullOrEmpty(cookie));
                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
                _httpClient.DefaultRequestHeaders.Clear();

                responseMessage = await _httpClient.PostAsync(
                    $"{_baseRoot}/api/verify",
                    new StringContent(
                        new JObject()
                        {
                            { "token", token },
                            { "user", "test" },
                            { "uuid", verifyRequest.UUID },
                        }.ToString()
                    )
                    {
                        Headers = { ContentType = new("application/json") }
                    }
                );

                Assert.Equal(
                    cookie,
                    responseMessage.Headers.GetValues("Set-Cookie").FirstOrDefault()
                );
                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            }
            else
            {
                webSocket.Close();
            }
        }
    }

    /// <summary>
    /// 上传
    /// </summary>
    [Theory]
    [InlineData("upload")]
    [InlineData("uploadSimplly")]
    public void ShouldBeAbleToUpload(string endPoint)
    {
        WebSocket webSocket = Utils.CreateConsoleWebSocket("test", "114514");
        webSocket.MessageReceived += Handle;
        webSocket.Open();

        async void Handle(object? sender, MessageReceivedEventArgs e)
        {
            await Task.Delay(1000);
            ReceivedPacket? packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
            if (packet?.SubType == "verify_request")
            {
                VerifyRequest verifyRequest = packet.Data!.ToObject<VerifyRequest>()!;
                string token = General.GetMD5(verifyRequest.UUID + "test" + "114514");

                await _httpClient.GetAsync(
                    $"{_baseRoot}/api/verify?token={token}&user=test&uuid={verifyRequest.UUID}"
                );

                MultipartFormDataContent multipartFormDataContent =
                    new() { Headers = { ContentType = new("multipart/form-data") } };

                List<byte> bytes = new();
                Enumerable.Range(1, 114514).ToList().ForEach((_) => bytes.Add(0));
                multipartFormDataContent.Add(new ByteArrayContent(bytes.ToArray()), "test.test");
                HttpResponseMessage responseMessage = await _httpClient.PostAsync(
                    $"{_baseRoot}/api/{endPoint}",
                    multipartFormDataContent
                );

                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            }
            else
            {
                webSocket.Close();
            }
        }
    }
}
