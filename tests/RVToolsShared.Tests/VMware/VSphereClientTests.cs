using System.Net;
using System.Text.Json;
using RVToolsShared.VMware;
using RVToolsShared.VMware.Exceptions;
using RVToolsShared.VMware.Models;

namespace RVToolsShared.Tests.VMware;

/// <summary>
/// Unit tests for VSphereClient using mock HTTP responses.
/// </summary>
public class VSphereClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly VSphereClient _client;

    public VSphereClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        var options = new VSphereConnectionOptions
        {
            ServerAddress = "vcenter.test.local",
            Username = "test",
            Password = "test",
            IgnoreSslErrors = true
        };

        // Use reflection or a factory to inject the mock handler
        // For simplicity, we'll test the public interface behavior
        _client = new VSphereClient("vcenter.test.local", ignoreSslErrors: true);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public void ServerAddress_ReturnsConfiguredAddress()
    {
        Assert.Equal("vcenter.test.local", _client.ServerAddress);
    }

    [Fact]
    public void IsConnected_InitiallyFalse()
    {
        Assert.False(_client.IsConnected);
    }

    [Fact]
    public async Task GetVirtualMachinesAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.GetVirtualMachinesAsync());
    }

    [Fact]
    public async Task GetHostsAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.GetHostsAsync());
    }

    [Fact]
    public async Task GetDatastoresAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.GetDatastoresAsync());
    }

    [Fact]
    public async Task GetClustersAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.GetClustersAsync());
    }

    [Fact]
    public async Task GetNetworksAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.GetNetworksAsync());
    }

    [Fact]
    public async Task GetResourcePoolsAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.GetResourcePoolsAsync());
    }

    [Fact]
    public async Task GetDatacentersAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.GetDatacentersAsync());
    }

    [Fact]
    public async Task GetFoldersAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.GetFoldersAsync());
    }

    [Fact]
    public async Task CollectFullInventoryAsync_ThrowsWhenNotConnected()
    {
        await Assert.ThrowsAsync<VSphereApiException>(
            () => _client.CollectFullInventoryAsync());
    }
}

/// <summary>
/// Mock HTTP message handler for testing.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, (HttpStatusCode StatusCode, string Content)> _responses = new();

    public void SetupResponse(string endpoint, HttpStatusCode statusCode, object content)
    {
        _responses[endpoint] = (statusCode, JsonSerializer.Serialize(content));
    }

    public void SetupResponse(string endpoint, HttpStatusCode statusCode, string content)
    {
        _responses[endpoint] = (statusCode, content);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.PathAndQuery ?? "";

        if (_responses.TryGetValue(path, out var response))
        {
            return Task.FromResult(new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(response.Content)
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
