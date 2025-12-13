using System.Net;
using RVToolsShared.VMware.Exceptions;

namespace RVToolsShared.Tests.VMware;

/// <summary>
/// Unit tests for VSphereApiException factory methods.
/// </summary>
public class VSphereApiExceptionTests
{
    [Fact]
    public void AuthenticationFailed_CreatesCorrectException()
    {
        var ex = VSphereApiException.AuthenticationFailed("vcenter.test.local", "Invalid credentials");

        Assert.Contains("vcenter.test.local", ex.Message);
        Assert.Contains("Invalid credentials", ex.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal("vcenter.test.local", ex.ServerAddress);
    }

    [Fact]
    public void AuthenticationFailed_WithoutDetails_CreatesCorrectException()
    {
        var ex = VSphereApiException.AuthenticationFailed("vcenter.test.local");

        Assert.Contains("vcenter.test.local", ex.Message);
        Assert.Contains("Authentication failed", ex.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public void ConnectionFailed_CreatesCorrectException()
    {
        var inner = new HttpRequestException("Connection refused");
        var ex = VSphereApiException.ConnectionFailed("vcenter.test.local", inner);

        Assert.Contains("vcenter.test.local", ex.Message);
        Assert.Contains("Connection refused", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal("vcenter.test.local", ex.ServerAddress);
    }

    [Fact]
    public void SessionNotEstablished_CreatesCorrectException()
    {
        var ex = VSphereApiException.SessionNotEstablished("vcenter.test.local");

        Assert.Contains("vcenter.test.local", ex.Message);
        Assert.Contains("No session established", ex.Message);
        Assert.Contains("CreateSessionAsync", ex.Message);
        Assert.Equal("vcenter.test.local", ex.ServerAddress);
    }

    [Fact]
    public void SessionExpired_CreatesCorrectException()
    {
        var ex = VSphereApiException.SessionExpired("vcenter.test.local");

        Assert.Contains("vcenter.test.local", ex.Message);
        Assert.Contains("Session expired", ex.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal("vcenter.test.local", ex.ServerAddress);
    }

    [Fact]
    public void ApiError_CreatesCorrectException()
    {
        var ex = VSphereApiException.ApiError(
            "vcenter.test.local",
            "/api/vcenter/vm",
            HttpStatusCode.InternalServerError,
            "{\"error\": \"Server error\"}");

        Assert.Contains("/api/vcenter/vm", ex.Message);
        Assert.Contains("500", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Equal("/api/vcenter/vm", ex.Endpoint);
        Assert.Equal("vcenter.test.local", ex.ServerAddress);
        Assert.Equal("{\"error\": \"Server error\"}", ex.ResponseBody);
    }

    [Fact]
    public void Timeout_CreatesCorrectException()
    {
        var ex = VSphereApiException.Timeout("vcenter.test.local", "/api/vcenter/vm", 60);

        Assert.Contains("vcenter.test.local", ex.Message);
        Assert.Contains("/api/vcenter/vm", ex.Message);
        Assert.Contains("60 seconds", ex.Message);
        Assert.Contains("timed out", ex.Message);
        Assert.Equal(HttpStatusCode.RequestTimeout, ex.StatusCode);
        Assert.Equal("/api/vcenter/vm", ex.Endpoint);
    }
}
