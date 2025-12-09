# Snapshot Aging Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Health Reports

---

**Category**: Health
**View**: `[Reporting].[vw_Snapshot_Aging]`
**RDL File**: `src/reports/Health/Snapshot_Aging.rdl`
**SQL Source**: `src/tsql/Views/Health/vw_Snapshot_Aging.sql`

## Purpose

Identifies VM snapshots and their age, helping to locate old snapshots that consume storage and can impact VM performance.

## Data Source

- **Primary Table**: `Current.vSnapshot`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VM | NVARCHAR | Virtual machine name |
| VM_UUID | NVARCHAR | VM unique identifier |
| Powerstate | NVARCHAR | VM power state |
| SnapshotName | NVARCHAR | Name of the snapshot |
| Description | NVARCHAR | Snapshot description |
| SnapshotDate | DATETIME | When the snapshot was created |
| Filename | NVARCHAR | Snapshot file name |
| Size_MiB_vmsn | NVARCHAR | Size of the snapshot memory file in MiB |
| Size_MiB_total | NVARCHAR | Total size of snapshot files in MiB |
| AgeDays | INT | Number of days since snapshot creation |
| Quiesced | NVARCHAR | Whether the snapshot was quiesced |
| State | NVARCHAR | Snapshot state |
| Datacenter | NVARCHAR | Datacenter location |
| Cluster | NVARCHAR | Cluster membership |
| Host | NVARCHAR | ESXi host running the VM |
| Folder | NVARCHAR | VM folder location |
| OS_according_to_the_VMware_Tools | NVARCHAR | Guest OS reported by VMware Tools |
| VI_SDK_Server | NVARCHAR | vCenter server |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Age Calculation Logic

```sql
DATEDIFF(DAY, TRY_CAST(Date_time AS DATETIME2), GETUTCDATE()) AS AgeDays
```

## Sample Queries

**Snapshots older than 7 days:**
```sql
SELECT VM, SnapshotName, SnapshotDate, AgeDays, Size_MiB_total, Cluster
FROM [Reporting].[vw_Snapshot_Aging]
WHERE AgeDays > 7
ORDER BY AgeDays DESC;
```

**Largest snapshots by total size:**
```sql
SELECT VM, SnapshotName, AgeDays,
       TRY_CAST(Size_MiB_total AS BIGINT) / 1024.0 AS Size_GB,
       Datacenter, Cluster
FROM [Reporting].[vw_Snapshot_Aging]
ORDER BY TRY_CAST(Size_MiB_total AS BIGINT) DESC;
```

**Snapshot age summary:**
```sql
SELECT
    CASE
        WHEN AgeDays <= 1 THEN '0-1 days'
        WHEN AgeDays <= 7 THEN '2-7 days'
        WHEN AgeDays <= 30 THEN '8-30 days'
        ELSE '30+ days'
    END AS Age_Bucket,
    COUNT(*) AS Snapshot_Count,
    SUM(TRY_CAST(Size_MiB_total AS BIGINT)) / 1024.0 AS Total_Size_GB
FROM [Reporting].[vw_Snapshot_Aging]
GROUP BY
    CASE
        WHEN AgeDays <= 1 THEN '0-1 days'
        WHEN AgeDays <= 7 THEN '2-7 days'
        WHEN AgeDays <= 30 THEN '8-30 days'
        ELSE '30+ days'
    END
ORDER BY MIN(AgeDays);
```

**VMs with multiple snapshots:**
```sql
SELECT VM, COUNT(*) AS Snapshot_Count,
       SUM(TRY_CAST(Size_MiB_total AS BIGINT)) / 1024.0 AS Total_Size_GB,
       MAX(AgeDays) AS Oldest_Snapshot_Days
FROM [Reporting].[vw_Snapshot_Aging]
GROUP BY VM
HAVING COUNT(*) > 1
ORDER BY Snapshot_Count DESC;
```

## Related Reports

- [VM Inventory](./VM_Inventory.md) - VMs with snapshots
- [Datastore Capacity](./Datastore_Capacity.md) - Storage impact of snapshots

## Notes

- Snapshots older than 72 hours are generally considered stale and should be reviewed for deletion.
- Large or numerous snapshots can significantly impact VM performance and storage consumption.
- AgeDays may be NULL if SnapshotDate cannot be parsed.
- Size columns may contain string values; use TRY_CAST for numeric operations.
- The view queries `Current.vSnapshot`, so data reflects the most recent RVTools import.
