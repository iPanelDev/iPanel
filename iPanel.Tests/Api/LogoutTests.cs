using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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
public class LogoutTests : IDisposable
{
    private const string _base = "http://127.0.0.1:30000/";
    private readonly IHost _host;
    private IServiceProvider Services => _host.Services;
    private UserManager UserManager => Services.GetRequiredService<UserManager>();

    public LogoutTests()
    {
        _host = new AppBuilder(new() { InstancePassword = "🥵" }).Build();
        _host.Start();
        UserManager.Clear();
        UserManager.Add(
            "Administrator",
            new() { Password = "123456", Level = PermissionLevel.Administrator }
        );
    }

    public void Dispose()
    {
        _host.StopAsync();
        _host.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ShouldBeAbleToLogout()
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
        var body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(body);

        response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users/@self/status"));
        var result = await response.Content.ReadFromJsonAsync<JsonObject>(
            JsonSerializerOptionsFactory.CamelCase
        );
        Assert.Equal(JsonValueKind.True, result?["data"]?["logined"]?.GetValueKind());

        response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users/@self/logout"));
        body = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(body);
        Assert.Null(body?.Data);
        Assert.Null(body?.ErrorMsg);

        response = await httpClient.SendAsync(new(HttpMethod.Get, "/api/users/@self/status"));
        result = await response.Content.ReadFromJsonAsync<JsonObject>(
            JsonSerializerOptionsFactory.CamelCase
        );
        Assert.Equal(JsonValueKind.False, result?["data"]?["logined"]?.GetValueKind());
    }
}
