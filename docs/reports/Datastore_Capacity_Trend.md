# Datastore Capacity Trend Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Trends Reports

---

**Category**: Trends
**View**: `[Reporting].[vw_Datastore_Capacity_Trend]`
**RDL File**: `src/reports/Trends/Datastore_Capacity_Trend.rdl`
**SQL Source**: `src/tsql/Views/Trends/vw_Datastore_Capacity_Trend.sql`

## Purpose

Tracks storage capacity usage over time for each datastore, enabling trend analysis and capacity planning through historical data.

## Data Source

- **Primary Table**: `History.vDatastore`
- **Update Frequency**: Historical data accumulated over multiple RVTools imports
- **Filter**: Includes records where ValidTo IS NULL (current) or ValidTo > ValidFrom (historical)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| SnapshotDate | DATE | Date of the data snapshot (cast from ValidFrom) |
| DatastoreName | NVARCHAR | Datastore name |
| VI_SDK_Server | NVARCHAR | vCenter server |
| Type | NVARCHAR | Datastore type (VMFS, NFS, vSAN, etc.) |
| Capacity_MiB | BIGINT | Total datastore capacity in MiB |
| Provisioned_MiB | BIGINT | Total provisioned storage in MiB |
| In_Use_MiB | BIGINT | Actual storage consumed in MiB |
| Free_MiB | BIGINT | Available free space in MiB |
| Free_Percent | DECIMAL(5,2) | Percentage of capacity that is free |
| Num_VMs | INT | Number of VMs using this datastore |
| ImportBatchId | INT | Import batch reference |

## Sample Queries

**Capacity trend for a specific datastore:**
```sql
SELECT SnapshotDate, DatastoreName,
       Capacity_MiB / 1024.0 AS Capacity_GB,
       In_Use_MiB / 1024.0 AS InUse_GB,
       Free_Percent
FROM [Reporting].[vw_Datastore_Capacity_Trend]
WHERE DatastoreName = 'VMFS-Prod-01'
ORDER BY SnapshotDate DESC;
```

**Storage growth over last 30 days:**
```sql
WITH DateRange AS (
    SELECT DatastoreName,
           MIN(In_Use_MiB) AS Start_InUse_MiB,
           MAX(In_Use_MiB) AS End_InUse_MiB
    FROM [Reporting].[vw_Datastore_Capacity_Trend]
    WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
    GROUP BY DatastoreName
)
SELECT DatastoreName,
       Start_InUse_MiB / 1024.0 AS Start_GB,
       End_InUse_MiB / 1024.0 AS End_GB,
       (End_InUse_MiB - Start_InUse_MiB) / 1024.0 AS Growth_GB
FROM DateRange
ORDER BY Growth_GB DESC;
```

**Daily average free space by datastore:**
```sql
SELECT DatastoreName,
       AVG(Free_Percent) AS Avg_Free_Percent,
       MIN(Free_Percent) AS Min_Free_Percent,
       MAX(Free_Percent) AS Max_Free_Percent
FROM [Reporting].[vw_Datastore_Capacity_Trend]
WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
GROUP BY DatastoreName
ORDER BY Avg_Free_Percent ASC;
```

**VM count trend per datastore:**
```sql
SELECT SnapshotDate, DatastoreName, Num_VMs
FROM [Reporting].[vw_Datastore_Capacity_Trend]
WHERE DatastoreName = 'VMFS-Prod-01'
ORDER BY SnapshotDate DESC;
```

## Related Reports

- [Datastore Capacity](./Datastore_Capacity.md) - Current capacity snapshot
- [Storage Growth](./Storage_Growth.md) - Growth projection with regression support

## Notes

- The view requires multiple RVTools imports over time to show meaningful trends.
- Each SnapshotDate represents a day where data was captured; gaps indicate missing imports.
- Capacity_MiB changes may indicate datastore expansion or reconfiguration.
- Rapid In_Use_MiB growth may indicate runaway VM storage consumption.
- The view uses History.vDatastore which maintains SCD Type 2 historical records.
