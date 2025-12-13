using RVToolsShared.VMware.Models;

namespace RVToolsShared.VMware;

/// <summary>
/// Interface for communicating with VMware vSphere REST API.
/// Provides methods to collect inventory data that mirrors RVTools Excel exports.
/// </summary>
public interface IVSphereClient : IDisposable
{
    /// <summary>
    /// Gets the server address this client is connected to.
    /// </summary>
    string ServerAddress { get; }

    /// <summary>
    /// Gets whether a session is currently established.
    /// </summary>
    bool IsConnected { get; }

    #region Session Management

    /// <summary>
    /// Creates an authenticated session with the vCenter server.
    /// </summary>
    /// <param name="username">vCenter username (e.g., administrator@vsphere.local)</param>
    /// <param name="password">vCenter password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session token string</returns>
    Task<string> CreateSessionAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminates the current session with the vCenter server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to vCenter without creating a persistent session.
    /// </summary>
    /// <param name="username">vCenter username</param>
    /// <param name="password">vCenter password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection test result with version info</returns>
    Task<ConnectionTestResult> TestConnectionAsync(string username, string password, CancellationToken cancellationToken = default);

    #endregion

    #region Inventory Collection (RVTools Sheet Parity)

    /// <summary>
    /// Gets all virtual machines (vInfo sheet equivalent).
    /// </summary>
    Task<IReadOnlyList<VmInfo>> GetVirtualMachinesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed information for a specific VM.
    /// </summary>
    Task<VmDetail?> GetVirtualMachineDetailAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all ESXi hosts (vHost sheet equivalent).
    /// </summary>
    Task<IReadOnlyList<HostInfo>> GetHostsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed information for a specific host.
    /// </summary>
    Task<HostDetail?> GetHostDetailAsync(string hostId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all datastores (vDatastore sheet equivalent).
    /// </summary>
    Task<IReadOnlyList<DatastoreInfo>> GetDatastoresAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all clusters (vCluster sheet equivalent).
    /// </summary>
    Task<IReadOnlyList<ClusterInfo>> GetClustersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all networks (vNetwork equivalent).
    /// </summary>
    Task<IReadOnlyList<NetworkInfo>> GetNetworksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all resource pools (vRP sheet equivalent).
    /// </summary>
    Task<IReadOnlyList<ResourcePoolInfo>> GetResourcePoolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all datacenters.
    /// </summary>
    Task<IReadOnlyList<DatacenterInfo>> GetDatacentersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all folders.
    /// </summary>
    Task<IReadOnlyList<FolderInfo>> GetFoldersAsync(CancellationToken cancellationToken = default);

    #endregion

    #region VM Hardware Details

    /// <summary>
    /// Gets disk information for a VM (vDisk sheet equivalent).
    /// </summary>
    Task<IReadOnlyList<VmDiskInfo>> GetVmDisksAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets network adapter information for a VM (vNIC portion of vNetwork).
    /// </summary>
    Task<IReadOnlyList<VmNicInfo>> GetVmNicsAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets snapshot information for a VM (vSnapshot sheet equivalent).
    /// </summary>
    Task<VmSnapshotInfo?> GetVmSnapshotsAsync(string vmId, CancellationToken cancellationToken = default);

    #endregion

    #region Full Inventory Collection

    /// <summary>
    /// Collects complete inventory from vCenter in a single operation.
    /// This is the main method used by the collection job to gather all data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete inventory snapshot</returns>
    Task<VSphereInventory> CollectFullInventoryAsync(CancellationToken cancellationToken = default);

    #endregion
}
