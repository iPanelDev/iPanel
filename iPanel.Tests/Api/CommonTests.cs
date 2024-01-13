using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using iPanel.Core.Models.Packets;
using iPanel.Utils.Json;

using Microsoft.Extensions.Hosting;

using Xunit;

namespace iPanel.Tests.Api;

[Collection("IPANEL")]
public class CommonTests : IDisposable
{
    private const string _base = "http://127.0.0.1:30000/";
    private readonly IHost _host;
    private readonly HttpClient _httpClient;

    public CommonTests()
    {
        _host = new AppBuilder(new() { InstancePassword = "🥵" }).Build();
        _host.StartAsync();
        _httpClient = new() { BaseAddress = new(_base) };
    }

    public void Dispose()
    {
        _host.StopAsync();
        _host.Dispose();
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ShouldBeAbleToVisitRoot()
    {
        var response = await _httpClient.GetAsync("/api");
        Assert.True(response.IsSuccessStatusCode);
        Assert.False(string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task ShouldBeAbleToGetVersion()
    {
        var response = await _httpClient.GetAsync("/api/version");
        Assert.True(response.IsSuccessStatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );
        Assert.Equal(Constant.Version, result?.Data?.ToString());
    }

    [Fact]
    public async Task ShouldBeAbleToGetStatus()
    {
        var response = await _httpClient.GetAsync("/api/users/@self/status");
        Assert.True(response.IsSuccessStatusCode);

        var result = await response.Content.ReadFromJsonAsync<JsonObject>(
            JsonSerializerOptionsFactory.CamelCase
        );
        Assert.Equal(JsonValueKind.False, result?["data"]?["logined"]?.GetValueKind());
    }

    [Fact]
    public async Task ShouldBeAbleToGet400()
    {
        var response = await _httpClient.SendAsync(
            new(HttpMethod.Get, "/api/instances/114514/subscribe")
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/users/@self")]
    [InlineData("/api/users/114514")]
    [InlineData("/api/instances")]
    [InlineData("/api/instances/114514")]
    [InlineData("/api/instances/114514/subscribe?connectionId=114154")]
    [InlineData("/api/instances/114514/start")]
    [InlineData("/api/instances/114514/stop")]
    [InlineData("/api/instances/114514/kill")]
    public async Task ShouldBeAbleToGet401(string url)
    {
        var response = await _httpClient.SendAsync(new(HttpMethod.Get, url));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/users/@self/login")]
    [InlineData("/api/instances/114514/input")]
    public async Task ShouldBeAbleToGet405(string url)
    {
        var response = await _httpClient.SendAsync(new(HttpMethod.Get, url));
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);

        response = await _httpClient.SendAsync(new(HttpMethod.Delete, url));
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);

        response = await _httpClient.SendAsync(new(HttpMethod.Put, url));
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);

        response = await _httpClient.SendAsync(new(HttpMethod.Head, url));
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task ShouldBeAbleToGet404()
    {
        var response = await _httpClient.SendAsync(new(HttpMethod.Get, "/api/114514"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
