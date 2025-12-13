using RVToolsShared.VMware.Models;

namespace RVToolsShared.Tests.VMware;

/// <summary>
/// Unit tests for ConnectionTestResult.
/// </summary>
public class ConnectionTestResultTests
{
    [Fact]
    public void Successful_CreatesSuccessfulResult()
    {
        var result = ConnectionTestResult.Successful(
            version: "8.0.2",
            build: "22617221",
            productName: "VMware vCenter Server",
            instanceUuid: "test-uuid",
            responseTimeMs: 150);

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Equal("8.0.2", result.Version);
        Assert.Equal("22617221", result.Build);
        Assert.Equal("VMware vCenter Server", result.ProductName);
        Assert.Equal("test-uuid", result.InstanceUuid);
        Assert.Equal(150, result.ResponseTimeMs);
    }

    [Fact]
    public void Successful_WithoutParameters_CreatesMinimalResult()
    {
        var result = ConnectionTestResult.Successful();

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Version);
        Assert.Null(result.Build);
        Assert.Null(result.ProductName);
        Assert.Null(result.InstanceUuid);
        Assert.Equal(0, result.ResponseTimeMs);
    }

    [Fact]
    public void Failed_CreatesFailedResult()
    {
        var result = ConnectionTestResult.Failed(
            errorMessage: "Connection refused",
            responseTimeMs: 5000);

        Assert.False(result.Success);
        Assert.Equal("Connection refused", result.ErrorMessage);
        Assert.Equal(5000, result.ResponseTimeMs);
        Assert.Null(result.Version);
    }

    [Fact]
    public void TestedAt_HasDefaultValue()
    {
        var before = DateTime.UtcNow;
        var result = ConnectionTestResult.Successful();
        var after = DateTime.UtcNow;

        Assert.True(result.TestedAt >= before);
        Assert.True(result.TestedAt <= after);
    }
}
