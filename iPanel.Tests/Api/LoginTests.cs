using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Packets.Data;
using iPanel.Core.Models.Users;
using iPanel.Core.Service;
using iPanel.Utils;
using iPanel.Utils.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace iPanel.Tests.Api;

[Collection("IPANEL")]
public class LoginTests : IDisposable
{
    private const string _base = "http://127.0.0.1:30000/";
    private readonly IHost _host;
    private IServiceProvider Services => _host.Services;
    private UserManager UserManager => Services.GetRequiredService<UserManager>();

    public LoginTests()
    {
        _host = new AppBuilder(new() { InstancePassword = "🥵" }).Build();
        _host.Start();
        UserManager.Clear();
        UserManager.Add(
            "Administrator",
            new() { Password = "123456", Level = PermissionLevel.Administrator }
        );
        UserManager.Add("Guest", new() { Password = "123456", Level = PermissionLevel.Guest });
    }

    public void Dispose()
    {
        _host.StopAsync();
        _host.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ShouldBeAbleToLogin()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };

        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.Administrator.123456"),
                        Time = dateTime.ToString(),
                        UserName = "Administrator"
                    },
                    "application/json"
                )
            }
        );

        var body = await response.Content.ReadFromJsonAsync<ApiPacket<Status>>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.ErrorMsg);
        Assert.True(body?.Data?.Logined);
        Assert.NotNull(body?.Data?.User);
        Assert.Equal(PermissionLevel.Administrator, body?.Data?.User?.Level);
    }

    [Fact]
    public async Task ShouldNotBeAbleToLoginWithWrongPwd()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };

        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.Administrator.114514"),
                        Time = dateTime.ToString(),
                        UserName = "Administrator"
                    },
                    "application/json"
                )
            }
        );

        var body = await response.Content.ReadFromJsonAsync<ApiPacket<JsonNode>>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("用户名或密码错误", body?.ErrorMsg);
        Assert.Null(body?.Data);
    }

    [Fact]
    public async Task ShouldNotBeAbleToLoginAsInvalidUser()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };

        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.abc.114514"),
                        Time = dateTime.ToString(),
                        UserName = "abc"
                    },
                    "application/json"
                )
            }
        );

        var body = await response.Content.ReadFromJsonAsync<ApiPacket<JsonNode>>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("用户名或密码错误", body?.ErrorMsg);
        Assert.Null(body?.Data);
    }

    [Theory]
    [InlineData("Get")]
    [InlineData("Delete")]
    [InlineData("Head")]
    [InlineData("Put")]
    [InlineData("Patch")]
    public async Task ShouldNotBeAbleToLoginWithOtherMethods(string method)
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        var httpMethod = method switch
        {
            "Get" => HttpMethod.Get,
            "Delete" => HttpMethod.Delete,
            "Head" => HttpMethod.Head,
            "Put" => HttpMethod.Put,
            "Patch" => HttpMethod.Patch,
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(httpMethod, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.Administrator.123456"),
                        Time = dateTime.ToString(),
                        UserName = "Administrator"
                    },
                    "application/json"
                )
            }
        );

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotBeAbleToLoginWithoutContentType()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };

        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.Administrator.123456"),
                        Time = dateTime.ToString(),
                        UserName = "Administrator"
                    }
                )
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "不支持的\"ContentType\"",
            (
                await response.Content.ReadFromJsonAsync<ApiPacket>(
                    JsonSerializerOptionsFactory.CamelCase
                )
            )?.ErrorMsg
        );
    }

    [Fact]
    public async Task ShouldNotBeAbleToLoginWithoutTime()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };

        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.Administrator.123456"),
                        UserName = "Administrator"
                    },
                    "application/json"
                )
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "\"time\"无效",
            (
                await response.Content.ReadFromJsonAsync<ApiPacket>(
                    JsonSerializerOptionsFactory.CamelCase
                )
            )?.ErrorMsg
        );
    }

    [Fact]
    public async Task ShouldNotBeAbleToLoginWithoutWrongTime()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };

        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.Administrator.123456"),
                        UserName = "Administrator",
                        Time = "1sfnrugeigheiggtnnfwenrnjwrnjr"
                    },
                    "application/json"
                )
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "\"time\"无效",
            (
                await response.Content.ReadFromJsonAsync<ApiPacket>(
                    JsonSerializerOptionsFactory.CamelCase
                )
            )?.ErrorMsg
        );
    }

    [Fact]
    public async Task ShouldNotBeAbleToLoginWithoutExpiredTime()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };

        var dateTime = DateTime.Now.AddSeconds(-20).ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.Administrator.123456"),
                        UserName = "Administrator",
                        Time = dateTime
                    },
                    "application/json"
                )
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "\"time\"已过期",
            (
                await response.Content.ReadFromJsonAsync<ApiPacket>(
                    JsonSerializerOptionsFactory.CamelCase
                )
            )?.ErrorMsg
        );
    }

    [Fact]
    public async Task ShouldNotBeAbleToLoginAsGuest()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };

        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5($"{dateTime}.Guest.123456"),
                        UserName = "Guest",
                        Time = dateTime
                    },
                    "application/json"
                )
            }
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(
            "用户无效",
            (
                await response.Content.ReadFromJsonAsync<ApiPacket>(
                    JsonSerializerOptionsFactory.CamelCase
                )
            )?.ErrorMsg
        );
    }
}
