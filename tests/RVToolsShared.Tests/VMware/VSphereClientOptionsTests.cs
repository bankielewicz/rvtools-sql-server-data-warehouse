using RVToolsShared.VMware;

namespace RVToolsShared.Tests.VMware;

/// <summary>
/// Unit tests for VSphereClientOptions and VSphereConnectionOptions.
/// </summary>
public class VSphereClientOptionsTests
{
    [Fact]
    public void VSphereClientOptions_HasCorrectDefaults()
    {
        var options = new VSphereClientOptions();

        Assert.Equal(60, options.DefaultTimeoutSeconds);
        Assert.Equal(3, options.MaxConcurrentConnections);
        Assert.Equal(3, options.RetryAttempts);
        Assert.Equal(5, options.RetryDelaySeconds);
        Assert.False(options.IgnoreSslErrors);
        Assert.Equal(1000, options.PageSize);
        Assert.True(options.CollectDetailedVmInfo);
        Assert.True(options.CollectVmDisks);
        Assert.True(options.CollectVmNics);
        Assert.True(options.CollectVmSnapshots);
        Assert.Equal(10, options.MaxParallelVmDetails);
    }

    [Fact]
    public void VSphereClientOptions_SectionName_IsCorrect()
    {
        Assert.Equal("VSphere", VSphereClientOptions.SectionName);
    }

    [Fact]
    public void VSphereConnectionOptions_BaseUrl_DefaultPort()
    {
        var options = new VSphereConnectionOptions
        {
            ServerAddress = "vcenter.example.com",
            Username = "admin",
            Password = "pass"
        };

        Assert.Equal("https://vcenter.example.com", options.BaseUrl);
    }

    [Fact]
    public void VSphereConnectionOptions_BaseUrl_CustomPort()
    {
        var options = new VSphereConnectionOptions
        {
            ServerAddress = "vcenter.example.com",
            Port = 8443,
            Username = "admin",
            Password = "pass"
        };

        Assert.Equal("https://vcenter.example.com:8443", options.BaseUrl);
    }

    [Fact]
    public void VSphereConnectionOptions_DefaultPort_Is443()
    {
        var options = new VSphereConnectionOptions
        {
            ServerAddress = "vcenter.example.com",
            Username = "admin",
            Password = "pass"
        };

        Assert.Equal(443, options.Port);
    }

    [Fact]
    public void VSphereConnectionOptions_IgnoreSslErrors_DefaultsFalse()
    {
        var options = new VSphereConnectionOptions
        {
            ServerAddress = "vcenter.example.com",
            Username = "admin",
            Password = "pass"
        };

        Assert.False(options.IgnoreSslErrors);
    }

    [Fact]
    public void VSphereConnectionOptions_TimeoutSeconds_NullByDefault()
    {
        var options = new VSphereConnectionOptions
        {
            ServerAddress = "vcenter.example.com",
            Username = "admin",
            Password = "pass"
        };

        Assert.Null(options.TimeoutSeconds);
    }
}
