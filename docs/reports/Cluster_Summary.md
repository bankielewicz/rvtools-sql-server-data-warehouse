# Cluster Summary Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Inventory Reports

---

**Category**: Inventory
**View**: `[Reporting].[vw_Cluster_Summary]`
**RDL File**: `src/reports/Inventory/Cluster_Summary.rdl`
**SQL Source**: `src/tsql/Views/Inventory/vw_Cluster_Summary.sql`

## Purpose

Provides a high-level overview of cluster configurations including HA, DRS, and DPM settings along with aggregate resource totals for capacity planning.

## Data Source

- **Primary Table**: `Current.vCluster`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| ClusterName | NVARCHAR | Cluster name |
| VI_SDK_Server | NVARCHAR | vCenter server managing this cluster |
| Config_status | NVARCHAR | Cluster configuration status |
| OverallStatus | NVARCHAR | Overall cluster health status |
| NumHosts | INT | Total number of hosts in the cluster |
| numEffectiveHosts | INT | Number of hosts contributing resources (not in maintenance) |
| TotalCpu | BIGINT | Total CPU capacity in MHz |
| NumCpuCores | INT | Total CPU cores across all hosts |
| NumCpuThreads | INT | Total CPU threads (with hyperthreading) |
| Effective_Cpu | BIGINT | Effective CPU capacity after HA reservations |
| TotalMemory | BIGINT | Total memory capacity in bytes |
| Effective_Memory | BIGINT | Effective memory after HA reservations |
| HA_enabled | NVARCHAR | Whether vSphere HA is enabled |
| Failover_Level | INT | Number of host failures to tolerate |
| AdmissionControlEnabled | NVARCHAR | Whether admission control is enforced |
| Host_monitoring | NVARCHAR | Host monitoring status |
| Isolation_Response | NVARCHAR | Response to host isolation |
| Restart_Priority | NVARCHAR | Default VM restart priority |
| VM_Monitoring | NVARCHAR | VM monitoring mode |
| Max_Failures | INT | Maximum VM failures before action |
| Max_Failure_Window | INT | Time window for failure counting (seconds) |
| Failure_Interval | INT | Minimum time between failures (seconds) |
| Min_Up_Time | INT | Minimum VM uptime before monitoring (seconds) |
| DRS_enabled | NVARCHAR | Whether DRS is enabled |
| DRS_default_VM_behavior | NVARCHAR | Default DRS automation level |
| DRS_vmotion_rate | INT | DRS migration threshold (1-5) |
| DPM_enabled | NVARCHAR | Whether Distributed Power Management is enabled |
| DPM_default_behavior | NVARCHAR | Default DPM automation level |
| Num_VMotions | INT | Number of vMotion events |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Sample Queries

**Cluster configuration overview:**
```sql
SELECT ClusterName, NumHosts, numEffectiveHosts,
       HA_enabled, DRS_enabled, DPM_enabled,
       Failover_Level, AdmissionControlEnabled
FROM [Reporting].[vw_Cluster_Summary]
ORDER BY ClusterName;
```

**Cluster capacity summary:**
```sql
SELECT ClusterName,
       NumCpuCores,
       TotalCpu / 1000 AS TotalCpu_GHz,
       Effective_Cpu / 1000 AS EffectiveCpu_GHz,
       TotalMemory / 1073741824 AS TotalMemory_GB,
       Effective_Memory / 1073741824 AS EffectiveMemory_GB
FROM [Reporting].[vw_Cluster_Summary]
ORDER BY TotalMemory DESC;
```

**Clusters without HA enabled:**
```sql
SELECT ClusterName, VI_SDK_Server, NumHosts, HA_enabled, DRS_enabled
FROM [Reporting].[vw_Cluster_Summary]
WHERE HA_enabled = 'false'
ORDER BY NumHosts DESC;
```

**DRS configuration details:**
```sql
SELECT ClusterName, DRS_enabled, DRS_default_VM_behavior,
       DRS_vmotion_rate, Num_VMotions
FROM [Reporting].[vw_Cluster_Summary]
WHERE DRS_enabled = 'true'
ORDER BY Num_VMotions DESC;
```

## Related Reports

- [Host Inventory](./Host_Inventory.md) - Detailed host information within clusters
- [VM Inventory](./VM_Inventory.md) - VMs running in these clusters

## Notes

- `numEffectiveHosts` excludes hosts in maintenance mode or standby.
- `Effective_Cpu` and `Effective_Memory` represent resources available after HA reservations.
- TotalMemory is in bytes; divide by 1073741824 for GB.
- DRS_vmotion_rate of 1 is most conservative (fewer migrations); 5 is most aggressive.
- Clusters without HA enabled have no failover protection.
- The view queries `Current.vCluster`, so data reflects the most recent RVTools import.
