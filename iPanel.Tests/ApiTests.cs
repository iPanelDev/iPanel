using iPanel.Core.Models.Packets;
using iPanel.Utils.Json;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

namespace iPanel.Tests;

[Collection("IPANEL")]
public class ApiTests : IDisposable
{
    private const string _base = "http://127.0.0.1:30000/api";
    private readonly IHost _host;
    private readonly HttpClient _httpClient;

    public ApiTests()
    {
        _host = new AppBuilder(new() { InstancePassword = "🥵" }).Build();
        _host.StartAsync();
        _httpClient = new();
    }

    public void Dispose()
    {
        _host.StopAsync();
        _host.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ShouldBeAbleToVisitRoot()
    {
        var response = await _httpClient.GetAsync(_base);
        Assert.True(response.IsSuccessStatusCode);
        Assert.True(!string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task ShouldBeAbleToGetVersion()
    {
        var response = await _httpClient.GetAsync(_base + "/meta/version");
        Assert.True(response.IsSuccessStatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiPacket>(
            JsonSerializerOptionsFactory.CamelCase
        );
        Assert.True(result?.Data?.ToString() == Constant.Version);
    }

    [Fact]
    public async Task ShouldBeAbleToGetStatus()
    {
        var response = await _httpClient.GetAsync(_base + "/users/@self/status");
        Assert.True(response.IsSuccessStatusCode);

        var result = await response.Content.ReadFromJsonAsync<JsonObject>(
            JsonSerializerOptionsFactory.CamelCase
        );
        Assert.True(result?["data"]?["logined"]?.GetValueKind() == JsonValueKind.False);
    }

    [Theory]
    [InlineData("/users")]
    [InlineData("/users/@self")]
    [InlineData("/users/114514")]
    [InlineData("/instances")]
    [InlineData("/instances/114514")]
    [InlineData("/instances/114514/subscribe?connectionId=114154")]
    [InlineData("/instances/114514/start")]
    [InlineData("/instances/114514/stop")]
    [InlineData("/instances/114514/kill")]
    [InlineData("/instances/114514/input")]
    public async Task ShouldBeAbleToGet401or403or405(string url)
    {
        var response = await _httpClient.SendAsync(new(HttpMethod.Get, _base + url));
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden
                || response.StatusCode == HttpStatusCode.Unauthorized
                || response.StatusCode == HttpStatusCode.MethodNotAllowed
        );
    }

    [Fact]
    public async Task ShouldBeAbleToGet404()
    {
        var response = await _httpClient.SendAsync(new(HttpMethod.Get, _base + "/114514"));
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
    }
}
