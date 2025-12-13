namespace RVToolsShared.VMware.Models;

/// <summary>
/// Complete vSphere inventory snapshot from a single collection run.
/// This is the main result object returned by CollectFullInventoryAsync().
/// </summary>
public class VSphereInventory
{
    /// <summary>
    /// When the collection started.
    /// </summary>
    public DateTime CollectionStartTime { get; set; }

    /// <summary>
    /// When the collection completed.
    /// </summary>
    public DateTime CollectionEndTime { get; set; }

    /// <summary>
    /// Total collection duration.
    /// </summary>
    public TimeSpan Duration => CollectionEndTime - CollectionStartTime;

    /// <summary>
    /// vCenter server address that was queried.
    /// </summary>
    public string VCenterServer { get; set; } = string.Empty;

    /// <summary>
    /// vCenter version information.
    /// </summary>
    public string? VCenterVersion { get; set; }

    /// <summary>
    /// vCenter build number.
    /// </summary>
    public string? VCenterBuild { get; set; }

    #region Inventory Objects

    /// <summary>
    /// All datacenters.
    /// </summary>
    public IReadOnlyList<DatacenterInfo> Datacenters { get; set; } = [];

    /// <summary>
    /// All clusters.
    /// </summary>
    public IReadOnlyList<ClusterInfo> Clusters { get; set; } = [];

    /// <summary>
    /// All ESXi hosts.
    /// </summary>
    public IReadOnlyList<HostInfo> Hosts { get; set; } = [];

    /// <summary>
    /// All datastores.
    /// </summary>
    public IReadOnlyList<DatastoreInfo> Datastores { get; set; } = [];

    /// <summary>
    /// All networks.
    /// </summary>
    public IReadOnlyList<NetworkInfo> Networks { get; set; } = [];

    /// <summary>
    /// All resource pools.
    /// </summary>
    public IReadOnlyList<ResourcePoolInfo> ResourcePools { get; set; } = [];

    /// <summary>
    /// All folders.
    /// </summary>
    public IReadOnlyList<FolderInfo> Folders { get; set; } = [];

    /// <summary>
    /// All virtual machines (summary level).
    /// </summary>
    public IReadOnlyList<VmInfo> VirtualMachines { get; set; } = [];

    /// <summary>
    /// Detailed VM information (optional, may be empty if not collected).
    /// Key is VM ID.
    /// </summary>
    public IReadOnlyDictionary<string, VmDetail> VmDetails { get; set; } = new Dictionary<string, VmDetail>();

    /// <summary>
    /// VM disk information (optional, may be empty if not collected).
    /// Key is VM ID.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<VmDiskInfo>> VmDisks { get; set; } = new Dictionary<string, IReadOnlyList<VmDiskInfo>>();

    /// <summary>
    /// VM NIC information (optional, may be empty if not collected).
    /// Key is VM ID.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<VmNicInfo>> VmNics { get; set; } = new Dictionary<string, IReadOnlyList<VmNicInfo>>();

    /// <summary>
    /// VM snapshot information (optional, may be empty if not collected).
    /// Key is VM ID.
    /// </summary>
    public IReadOnlyDictionary<string, VmSnapshotInfo> VmSnapshots { get; set; } = new Dictionary<string, VmSnapshotInfo>();

    #endregion

    #region Statistics

    /// <summary>
    /// Total number of VMs collected.
    /// </summary>
    public int TotalVMs => VirtualMachines.Count;

    /// <summary>
    /// Total number of hosts collected.
    /// </summary>
    public int TotalHosts => Hosts.Count;

    /// <summary>
    /// Total number of datastores collected.
    /// </summary>
    public int TotalDatastores => Datastores.Count;

    /// <summary>
    /// Total number of clusters collected.
    /// </summary>
    public int TotalClusters => Clusters.Count;

    /// <summary>
    /// Any errors that occurred during collection (non-fatal).
    /// </summary>
    public List<string> CollectionErrors { get; set; } = [];

    /// <summary>
    /// Whether the collection completed successfully.
    /// </summary>
    public bool IsComplete { get; set; }

    #endregion
}
