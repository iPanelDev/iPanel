using iPanel.Core.Models.Users;
using iPanel.Core.Models.Packets;
using iPanel.Core.Models.Packets.Data;
using iPanel.Core.Service;
using iPanel.Utils.Json;
using iPanel.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace iPanel.Tests;

[Collection("IPANEL")]
public class UserManagementTests : IDisposable
{
    private const string _base = "http://127.0.0.1:30000/";
    private readonly IHost _host;
    private IServiceProvider Services => _host.Services;
    private UserManager UserManager => Services.GetRequiredService<UserManager>();

    public UserManagementTests()
    {
        _host = new AppBuilder(new() { InstancePassword = "🥵" }).Build();
        _host.Start();
        UserManager.Clear();
        UserManager.Add(
            "Administrator",
            new() { Password = "123456", Level = PermissionLevel.Administrator }
        );
        UserManager.Add(
            "Assistant",
            new() { Password = "123456", Level = PermissionLevel.Assistant }
        );
        UserManager.Add(
            "ReadOnly",
            new() { Password = "123456", Level = PermissionLevel.ReadOnly }
        );
        UserManager.Add("Guest", new() { Password = "123456", Level = PermissionLevel.Guest });
    }

    public void Dispose()
    {
        _host.StopAsync();
        _host.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task Login(
        HttpClient httpClient,
        PermissionLevel permissionLevel,
        string? password = null
    )
    {
        var dateTime = DateTime.Now.ToString("o");
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/@self/login")
            {
                Content = Utils.CreateContent(
                    new VerifyBody
                    {
                        MD5 = Encryption.GetMD5(
                            $"{dateTime}.{permissionLevel}.{password ?? "123456"}"
                        ),
                        Time = dateTime.ToString(),
                        UserName = permissionLevel.ToString()
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
    }

    [Fact]
    public async Task ShouldNotBeAbleToGetAllUsersWhenNotLoginYet()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        var response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users"));

        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Fact]
    public async Task ShouldNotBeAbleToRemoveSpecificUserWhenNotLoginYet()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        var response = await httpClient.SendAsync(new(HttpMethod.Delete, "/api/users/Guest"));

        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Fact]
    public async Task ShouldNotBeAbleToGetSpecificUserWhenNotLoginYet()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        var response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users/Guest"));

        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Fact]
    public async Task ShouldNotBeAbleToCreateSpecificUserWhenNotLoginYet()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/Guest")
            {
                Content = Utils.CreateContent(new User { Password = "123456" }, "application/json")
            }
        );

        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Fact]
    public async Task ShouldNotBeAbleToEditSpecificUserWhenNotLoginYet()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        var response = await httpClient.SendAsync(
            new(HttpMethod.Put, "/api/users/Guest")
            {
                Content = Utils.CreateContent(new User { Password = "123456" }, "application/json")
            }
        );

        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Fact]
    public async Task ShouldNotBeAbleToGetSpecificUser()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        var response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users/Guest"));

        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Theory]
    [InlineData(PermissionLevel.Assistant)]
    [InlineData(PermissionLevel.ReadOnly)]
    public async Task ShouldNotBeAbleToGetAllUsersWhenLoginNotAsAdministrator(
        PermissionLevel permissionLevel
    )
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, permissionLevel);

        var response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users"));
        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Theory]
    [InlineData(PermissionLevel.Assistant)]
    [InlineData(PermissionLevel.ReadOnly)]
    public async Task ShouldNotBeAbleToRemoveSpecificUserWhenLoginNotAsAdministrator(
        PermissionLevel permissionLevel
    )
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, permissionLevel);

        var response = await httpClient.SendAsync(new(HttpMethod.Delete, "/api/users/Guest"));
        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Theory]
    [InlineData(PermissionLevel.Assistant)]
    [InlineData(PermissionLevel.ReadOnly)]
    public async Task ShouldNotBeAbleToGetSpecificUserWhenLoginNotAsAdministrator(
        PermissionLevel permissionLevel
    )
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, permissionLevel);
        var response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users/Guest"));

        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Theory]
    [InlineData(PermissionLevel.Assistant)]
    [InlineData(PermissionLevel.ReadOnly)]
    public async Task ShouldNotBeAbleToCreateSpecificUserWhenLoginNotAsAdministrator(
        PermissionLevel permissionLevel
    )
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, permissionLevel);

        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/Guest")
            {
                Content = Utils.CreateContent(new User { Password = "123456" }, "application/json")
            }
        );
        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Theory]
    [InlineData(PermissionLevel.Assistant)]
    [InlineData(PermissionLevel.ReadOnly)]
    public async Task ShouldNotBeAbleToEditSpecificUserWhenLoginNotAsAdministrator(
        PermissionLevel permissionLevel
    )
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, permissionLevel);

        var response = await httpClient.SendAsync(
            new(HttpMethod.Put, "/api/users/Guest")
            {
                Content = Utils.CreateContent(new User { Password = "123456" }, "application/json")
            }
        );
        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Fact]
    public async Task ShouldBeAbleToGetAllUsers()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, PermissionLevel.Administrator);

        var response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users"));
        var body = await response.Content.ReadFromJsonAsync<ApiPacket<Dictionary<string, User>>>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(body);
        Assert.Equal(4, body?.Data?.Count);
        Assert.Null(body?.Data?.FirstOrDefault().Value.Password);
        Assert.True(body?.Data?.ContainsKey("Administrator"));
        Assert.True(body?.Data?.ContainsKey("ReadOnly"));
        Assert.True(body?.Data?.ContainsKey("Assistant"));
        Assert.True(body?.Data?.ContainsKey("Guest"));
    }

    [Fact]
    public async Task ShouldBeAbleToGetSpecificUser()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, PermissionLevel.Administrator);

        var response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users/Guest"));
        var body = await response.Content.ReadFromJsonAsync<ApiPacket<User>>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(body);
        Assert.Equal(PermissionLevel.Guest, body?.Data?.Level);
        Assert.Null(body?.Data?.Password);
    }

    [Fact]
    public async Task ShouldBeAbleToCreateSpecificUser()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, PermissionLevel.Administrator);

        var response = await httpClient.SendAsync(
            new(HttpMethod.Post, "/api/users/Guest1")
            {
                Content = Utils.CreateContent(new User { Password = "123456" }, "application/json")
            }
        );
        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
    }

    [Fact]
    public async Task ShouldBeAbleToRemoveSpecificUser()
    {
        using var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, PermissionLevel.Administrator);

        var response = await httpClient.SendAsync(new(HttpMethod.Delete, "/api/users/Guest"));
        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);

        response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users/Guest"));
        body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
        Assert.Equal("用户不存在", body?.ErrorMsg);
    }

    [Fact]
    public async Task ShouldBeAbleToEditSpecificUser()
    {
        var httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, PermissionLevel.Administrator);

        var response = await httpClient.SendAsync(
            new(HttpMethod.Put, "/api/users/ReadOnly")
            {
                Content = Utils.CreateContent(
                    new User { Password = "1234567", Level = PermissionLevel.ReadOnly },
                    "application/json"
                )
            }
        );
        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.True(response.IsSuccessStatusCode);

        httpClient = new HttpClient { BaseAddress = new(_base) };
        await Login(httpClient, PermissionLevel.ReadOnly, "1234567");
    }
}
