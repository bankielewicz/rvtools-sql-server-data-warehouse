# Datastore Inventory Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Inventory Reports

---

**Category**: Inventory
**View**: `[Reporting].[vw_Datastore_Inventory]`
**RDL File**: `src/reports/Inventory/Datastore_Inventory.rdl`
**SQL Source**: `src/tsql/Views/Inventory/vw_Datastore_Inventory.sql`

## Purpose

Provides a comprehensive inventory of all datastores with capacity metrics, configuration details, and SIOC settings.

## Data Source

- **Primary Table**: `Current.vDatastore`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| DatastoreName | NVARCHAR | Datastore name |
| VI_SDK_Server | NVARCHAR | vCenter server managing this datastore |
| Config_status | NVARCHAR | Configuration status |
| Accessible | NVARCHAR | Whether the datastore is currently accessible |
| Type | NVARCHAR | Datastore type (VMFS, NFS, vSAN, VVOL, etc.) |
| Major_Version | NVARCHAR | VMFS major version number |
| Version | NVARCHAR | Full VMFS version |
| VMFS_Upgradeable | NVARCHAR | Whether VMFS can be upgraded |
| Capacity_MiB | NVARCHAR | Total capacity in MiB |
| Provisioned_MiB | NVARCHAR | Total provisioned storage in MiB |
| In_Use_MiB | NVARCHAR | Actual storage consumed in MiB |
| Free_MiB | NVARCHAR | Available free space in MiB |
| Free_Percent | NVARCHAR | Percentage of capacity that is free |
| Num_VMs | INT | Number of VMs using this datastore |
| Num_Hosts | INT | Number of hosts with access to this datastore |
| SIOC_enabled | NVARCHAR | Whether Storage I/O Control is enabled |
| SIOC_Threshold | NVARCHAR | SIOC congestion threshold in milliseconds |
| Cluster_name | NVARCHAR | Datastore cluster membership |
| Cluster_capacity_MiB | NVARCHAR | Total cluster capacity in MiB |
| Cluster_free_space_MiB | NVARCHAR | Total cluster free space in MiB |
| Block_size | NVARCHAR | VMFS block size |
| URL | NVARCHAR | Datastore URL path |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Sample Queries

**Complete datastore inventory:**
```sql
SELECT DatastoreName, Type, Version,
       TRY_CAST(Capacity_MiB AS BIGINT) / 1024.0 AS Capacity_GB,
       TRY_CAST(Free_MiB AS BIGINT) / 1024.0 AS Free_GB,
       Free_Percent, Num_VMs
FROM [Reporting].[vw_Datastore_Inventory]
ORDER BY TRY_CAST(Capacity_MiB AS BIGINT) DESC;
```

**Datastores by type:**
```sql
SELECT Type,
       COUNT(*) AS Datastore_Count,
       SUM(TRY_CAST(Capacity_MiB AS BIGINT)) / 1024.0 AS Total_Capacity_GB,
       SUM(TRY_CAST(Free_MiB AS BIGINT)) / 1024.0 AS Total_Free_GB
FROM [Reporting].[vw_Datastore_Inventory]
GROUP BY Type
ORDER BY Total_Capacity_GB DESC;
```

**Inaccessible datastores:**
```sql
SELECT DatastoreName, Type, VI_SDK_Server, Config_status
FROM [Reporting].[vw_Datastore_Inventory]
WHERE Accessible = 'False'
ORDER BY DatastoreName;
```

**SIOC configuration review:**
```sql
SELECT DatastoreName, Type, SIOC_enabled, SIOC_Threshold,
       TRY_CAST(Capacity_MiB AS BIGINT) / 1024.0 AS Capacity_GB,
       Num_VMs
FROM [Reporting].[vw_Datastore_Inventory]
WHERE Type = 'VMFS'
ORDER BY SIOC_enabled DESC, DatastoreName;
```

**VMFS upgrade candidates:**
```sql
SELECT DatastoreName, Major_Version, Version, VMFS_Upgradeable
FROM [Reporting].[vw_Datastore_Inventory]
WHERE VMFS_Upgradeable = 'true'
ORDER BY Major_Version, DatastoreName;
```

## Related Reports

- [Datastore Capacity](./Datastore_Capacity.md) - Capacity analysis and utilization status
- [Datastore Capacity Trend](./Datastore_Capacity_Trend.md) - Historical capacity trends
- [Storage Growth](./Storage_Growth.md) - Growth projection with regression analysis

## Notes

- Capacity columns are stored as NVARCHAR; use TRY_CAST for numeric operations.
- VMFS version 6 is current; earlier versions may be upgrade candidates.
- SIOC helps manage I/O contention on shared datastores but adds overhead.
- Datastores with `Accessible = 'False'` require immediate investigation.
- NFS and vSAN datastores may have different attributes than VMFS.
- The view queries `Current.vDatastore`, so data reflects the most recent RVTools import.
