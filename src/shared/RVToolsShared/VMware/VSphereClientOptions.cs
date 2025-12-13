namespace RVToolsShared.VMware;

/// <summary>
/// Configuration options for VSphereClient.
/// Maps to appsettings.json VSphere section.
/// </summary>
public class VSphereClientOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "VSphere";

    /// <summary>
    /// Default timeout for API requests in seconds.
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of concurrent connections per vCenter.
    /// </summary>
    public int MaxConcurrentConnections { get; set; } = 3;

    /// <summary>
    /// Number of retry attempts for failed requests.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Whether to ignore SSL certificate errors (not recommended for production).
    /// </summary>
    public bool IgnoreSslErrors { get; set; } = false;

    /// <summary>
    /// Page size for paginated API requests.
    /// </summary>
    public int PageSize { get; set; } = 1000;

    /// <summary>
    /// Whether to collect detailed VM information (slower but more complete).
    /// </summary>
    public bool CollectDetailedVmInfo { get; set; } = true;

    /// <summary>
    /// Whether to collect VM disk information.
    /// </summary>
    public bool CollectVmDisks { get; set; } = true;

    /// <summary>
    /// Whether to collect VM NIC information.
    /// </summary>
    public bool CollectVmNics { get; set; } = true;

    /// <summary>
    /// Whether to collect VM snapshot information.
    /// </summary>
    public bool CollectVmSnapshots { get; set; } = true;

    /// <summary>
    /// Maximum number of VMs to collect details for in parallel.
    /// </summary>
    public int MaxParallelVmDetails { get; set; } = 10;
}

/// <summary>
/// Connection-specific options for a single vCenter.
/// </summary>
public class VSphereConnectionOptions
{
    /// <summary>
    /// vCenter server hostname or IP address.
    /// </summary>
    public required string ServerAddress { get; set; }

    /// <summary>
    /// Port number (default 443).
    /// </summary>
    public int Port { get; set; } = 443;

    /// <summary>
    /// vCenter username.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// vCenter password.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Whether to ignore SSL certificate errors for this connection.
    /// </summary>
    public bool IgnoreSslErrors { get; set; } = false;

    /// <summary>
    /// Request timeout in seconds (overrides global default).
    /// </summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets the base URL for API requests.
    /// </summary>
    public string BaseUrl => Port == 443
        ? $"https://{ServerAddress}"
        : $"https://{ServerAddress}:{Port}";
}
