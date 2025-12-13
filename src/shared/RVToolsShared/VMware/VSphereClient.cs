using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVToolsShared.VMware.Exceptions;
using RVToolsShared.VMware.Models;

namespace RVToolsShared.VMware;

/// <summary>
/// Client for communicating with VMware vSphere REST API.
/// Implements IVSphereClient for inventory collection.
/// </summary>
public class VSphereClient : IVSphereClient
{
    private readonly HttpClient _httpClient;
    private readonly VSphereClientOptions _options;
    private readonly ILogger<VSphereClient>? _logger;
    private readonly string _serverAddress;
    private string? _sessionToken;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    /// <summary>
    /// Creates a new VSphereClient for the specified server.
    /// </summary>
    /// <param name="connectionOptions">Connection-specific options</param>
    /// <param name="clientOptions">Global client options</param>
    /// <param name="logger">Optional logger</param>
    public VSphereClient(
        VSphereConnectionOptions connectionOptions,
        VSphereClientOptions? clientOptions = null,
        ILogger<VSphereClient>? logger = null)
    {
        _serverAddress = connectionOptions.ServerAddress;
        _options = clientOptions ?? new VSphereClientOptions();
        _logger = logger;

        var handler = new HttpClientHandler();

        if (connectionOptions.IgnoreSslErrors || _options.IgnoreSslErrors)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            _logger?.LogWarning("SSL certificate validation disabled for {Server}", _serverAddress);
        }

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(connectionOptions.BaseUrl),
            Timeout = TimeSpan.FromSeconds(connectionOptions.TimeoutSeconds ?? _options.DefaultTimeoutSeconds)
        };

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Creates a new VSphereClient with simple parameters (for testing).
    /// </summary>
    public VSphereClient(string serverAddress, bool ignoreSslErrors = false, int timeoutSeconds = 60, ILogger<VSphereClient>? logger = null)
        : this(new VSphereConnectionOptions
        {
            ServerAddress = serverAddress,
            Username = string.Empty,
            Password = string.Empty,
            IgnoreSslErrors = ignoreSslErrors,
            TimeoutSeconds = timeoutSeconds
        }, null, logger)
    {
    }

    /// <inheritdoc />
    public string ServerAddress => _serverAddress;

    /// <inheritdoc />
    public bool IsConnected => !string.IsNullOrEmpty(_sessionToken);

    #region Session Management

    /// <inheritdoc />
    public async Task<string> CreateSessionAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Creating session for {Server}", _serverAddress);

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/session");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw VSphereApiException.AuthenticationFailed(_serverAddress, "Invalid username or password");
            }

            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsStringAsync(cancellationToken);
            _sessionToken = token.Trim('"');

            _logger?.LogInformation("Session established with {Server}", _serverAddress);
            return _sessionToken;
        }
        catch (HttpRequestException ex)
        {
            throw VSphereApiException.ConnectionFailed(_serverAddress, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw VSphereApiException.Timeout(_serverAddress, "/api/session", _options.DefaultTimeoutSeconds);
        }
    }

    /// <inheritdoc />
    public async Task DeleteSessionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_sessionToken))
            return;

        _logger?.LogDebug("Deleting session for {Server}", _serverAddress);

        try
        {
            var request = CreateAuthenticatedRequest(HttpMethod.Delete, "/api/session");
            await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to delete session for {Server}", _serverAddress);
        }
        finally
        {
            _sessionToken = null;
        }
    }

    /// <inheritdoc />
    public async Task<ConnectionTestResult> TestConnectionAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Create session
            await CreateSessionAsync(username, password, cancellationToken);

            // Get vCenter version info (optional, for display purposes)
            string? version = null;
            string? build = null;
            string? productName = null;

            try
            {
                var aboutInfo = await GetAsync<VCenterAboutInfo>("/api/appliance/system/version", cancellationToken);
                if (aboutInfo != null)
                {
                    version = aboutInfo.Version;
                    build = aboutInfo.Build;
                    productName = aboutInfo.Product;
                }
            }
            catch
            {
                // Version info is optional, don't fail the test
                _logger?.LogDebug("Could not retrieve version info from {Server}", _serverAddress);
            }

            // Delete session
            await DeleteSessionAsync(cancellationToken);

            stopwatch.Stop();
            return ConnectionTestResult.Successful(version, build, productName, null, stopwatch.ElapsedMilliseconds);
        }
        catch (VSphereApiException ex)
        {
            stopwatch.Stop();
            return ConnectionTestResult.Failed(ex.Message, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return ConnectionTestResult.Failed($"Unexpected error: {ex.Message}", stopwatch.ElapsedMilliseconds);
        }
    }

    #endregion

    #region Inventory Collection

    /// <inheritdoc />
    public async Task<IReadOnlyList<VmInfo>> GetVirtualMachinesAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<VmInfo>("/api/vcenter/vm", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VmDetail?> GetVirtualMachineDetailAsync(string vmId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<VmDetail>($"/api/vcenter/vm/{vmId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<HostInfo>> GetHostsAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<HostInfo>("/api/vcenter/host", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<HostDetail?> GetHostDetailAsync(string hostId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<HostDetail>($"/api/vcenter/host/{hostId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DatastoreInfo>> GetDatastoresAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<DatastoreInfo>("/api/vcenter/datastore", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClusterInfo>> GetClustersAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<ClusterInfo>("/api/vcenter/cluster", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NetworkInfo>> GetNetworksAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<NetworkInfo>("/api/vcenter/network", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ResourcePoolInfo>> GetResourcePoolsAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<ResourcePoolInfo>("/api/vcenter/resource-pool", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DatacenterInfo>> GetDatacentersAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<DatacenterInfo>("/api/vcenter/datacenter", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FolderInfo>> GetFoldersAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<FolderInfo>("/api/vcenter/folder", cancellationToken);
    }

    #endregion

    #region VM Hardware Details

    /// <inheritdoc />
    public async Task<IReadOnlyList<VmDiskInfo>> GetVmDisksAsync(string vmId, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<Dictionary<string, VmDiskInfo>>($"/api/vcenter/vm/{vmId}/hardware/disk", cancellationToken);
        return response?.Values.ToList() ?? [];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VmNicInfo>> GetVmNicsAsync(string vmId, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<Dictionary<string, VmNicInfo>>($"/api/vcenter/vm/{vmId}/hardware/ethernet", cancellationToken);
        return response?.Values.ToList() ?? [];
    }

    /// <inheritdoc />
    public async Task<VmSnapshotInfo?> GetVmSnapshotsAsync(string vmId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<VmSnapshotInfo>($"/api/vcenter/vm/{vmId}/snapshots", cancellationToken);
    }

    #endregion

    #region Full Inventory Collection

    /// <inheritdoc />
    public async Task<VSphereInventory> CollectFullInventoryAsync(CancellationToken cancellationToken = default)
    {
        EnsureSession();

        var inventory = new VSphereInventory
        {
            CollectionStartTime = DateTime.UtcNow,
            VCenterServer = _serverAddress
        };

        _logger?.LogInformation("Starting full inventory collection from {Server}", _serverAddress);

        try
        {
            // Collect base inventory objects in parallel
            var datacenterTask = GetDatacentersAsync(cancellationToken);
            var clusterTask = GetClustersAsync(cancellationToken);
            var hostTask = GetHostsAsync(cancellationToken);
            var datastoreTask = GetDatastoresAsync(cancellationToken);
            var networkTask = GetNetworksAsync(cancellationToken);
            var resourcePoolTask = GetResourcePoolsAsync(cancellationToken);
            var folderTask = GetFoldersAsync(cancellationToken);
            var vmTask = GetVirtualMachinesAsync(cancellationToken);

            await Task.WhenAll(
                datacenterTask, clusterTask, hostTask, datastoreTask,
                networkTask, resourcePoolTask, folderTask, vmTask);

            inventory.Datacenters = await datacenterTask;
            inventory.Clusters = await clusterTask;
            inventory.Hosts = await hostTask;
            inventory.Datastores = await datastoreTask;
            inventory.Networks = await networkTask;
            inventory.ResourcePools = await resourcePoolTask;
            inventory.Folders = await folderTask;
            inventory.VirtualMachines = await vmTask;

            _logger?.LogInformation(
                "Base inventory collected: {VMs} VMs, {Hosts} hosts, {Datastores} datastores, {Clusters} clusters",
                inventory.TotalVMs, inventory.TotalHosts, inventory.TotalDatastores, inventory.TotalClusters);

            // Collect detailed VM information if enabled
            if (_options.CollectDetailedVmInfo)
            {
                await CollectVmDetailsAsync(inventory, cancellationToken);
            }

            inventory.IsComplete = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during inventory collection from {Server}", _serverAddress);
            inventory.CollectionErrors.Add(ex.Message);
        }
        finally
        {
            inventory.CollectionEndTime = DateTime.UtcNow;
        }

        _logger?.LogInformation(
            "Inventory collection completed in {Duration:F1}s from {Server}",
            inventory.Duration.TotalSeconds, _serverAddress);

        return inventory;
    }

    private async Task CollectVmDetailsAsync(VSphereInventory inventory, CancellationToken cancellationToken)
    {
        var vmIds = inventory.VirtualMachines.Select(vm => vm.VmId).ToList();
        var vmDetails = new Dictionary<string, VmDetail>();
        var vmDisks = new Dictionary<string, IReadOnlyList<VmDiskInfo>>();
        var vmNics = new Dictionary<string, IReadOnlyList<VmNicInfo>>();
        var vmSnapshots = new Dictionary<string, VmSnapshotInfo>();

        _logger?.LogDebug("Collecting detailed info for {Count} VMs", vmIds.Count);

        // Process VMs in batches to control parallelism
        var semaphore = new SemaphoreSlim(_options.MaxParallelVmDetails);

        var tasks = vmIds.Select(async vmId =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                // Collect VM detail
                var detail = await GetVirtualMachineDetailAsync(vmId, cancellationToken);
                if (detail != null)
                {
                    lock (vmDetails) { vmDetails[vmId] = detail; }
                }

                // Collect VM disks
                if (_options.CollectVmDisks)
                {
                    try
                    {
                        var disks = await GetVmDisksAsync(vmId, cancellationToken);
                        lock (vmDisks) { vmDisks[vmId] = disks; }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Failed to get disks for VM {VmId}", vmId);
                    }
                }

                // Collect VM NICs
                if (_options.CollectVmNics)
                {
                    try
                    {
                        var nics = await GetVmNicsAsync(vmId, cancellationToken);
                        lock (vmNics) { vmNics[vmId] = nics; }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Failed to get NICs for VM {VmId}", vmId);
                    }
                }

                // Collect VM snapshots
                if (_options.CollectVmSnapshots)
                {
                    try
                    {
                        var snapshots = await GetVmSnapshotsAsync(vmId, cancellationToken);
                        if (snapshots != null)
                        {
                            lock (vmSnapshots) { vmSnapshots[vmId] = snapshots; }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Failed to get snapshots for VM {VmId}", vmId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to get details for VM {VmId}", vmId);
                inventory.CollectionErrors.Add($"VM {vmId}: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        inventory.VmDetails = vmDetails;
        inventory.VmDisks = vmDisks;
        inventory.VmNics = vmNics;
        inventory.VmSnapshots = vmSnapshots;

        _logger?.LogDebug(
            "Collected details: {Details} details, {Disks} disk sets, {Nics} NIC sets, {Snapshots} snapshot sets",
            vmDetails.Count, vmDisks.Count, vmNics.Count, vmSnapshots.Count);
    }

    #endregion

    #region HTTP Helpers

    private HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, endpoint);
        request.Headers.Add("vmware-api-session-id", _sessionToken);
        return request;
    }

    private void EnsureSession()
    {
        if (string.IsNullOrEmpty(_sessionToken))
        {
            throw VSphereApiException.SessionNotEstablished(_serverAddress);
        }
    }

    private async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken) where T : class
    {
        EnsureSession();

        var request = CreateAuthenticatedRequest(HttpMethod.Get, endpoint);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _sessionToken = null;
                throw VSphereApiException.SessionExpired(_serverAddress);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw VSphereApiException.ApiError(_serverAddress, endpoint, response.StatusCode, body);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw VSphereApiException.ConnectionFailed(_serverAddress, ex);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw VSphereApiException.Timeout(_serverAddress, endpoint, _options.DefaultTimeoutSeconds);
        }
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        var result = await GetAsync<List<T>>(endpoint, cancellationToken);
        return result ?? [];
    }

    #endregion

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the client resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Try to delete session synchronously (best effort)
            if (!string.IsNullOrEmpty(_sessionToken))
            {
                try
                {
                    var request = CreateAuthenticatedRequest(HttpMethod.Delete, "/api/session");
                    _httpClient.Send(request);
                }
                catch
                {
                    // Ignore errors during disposal
                }
            }

            _httpClient.Dispose();
        }

        _disposed = true;
    }

    #endregion
}

/// <summary>
/// vCenter about/version information.
/// </summary>
internal class VCenterAboutInfo
{
    public string? Version { get; set; }
    public string? Build { get; set; }
    public string? Product { get; set; }
    public string? Type { get; set; }
}
