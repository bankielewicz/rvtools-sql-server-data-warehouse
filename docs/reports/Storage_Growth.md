# Storage Growth Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Trends Reports

---

**Category**: Trends
**View**: `[Reporting].[vw_Trends_Storage_Growth]`
**RDL File**: `src/reports/Trends/Storage_Growth.rdl`
**SQL Source**: `src/tsql/Views/Trends/vw_Storage_Growth.sql`

## Purpose

Provides historical datastore capacity data for trend analysis and growth projection. The view includes a DayNumber column to facilitate linear regression calculations for forecasting.

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
| In_Use_MiB | BIGINT | Used space in MiB |
| Free_MiB | BIGINT | Free space in MiB |
| Free_Percent | DECIMAL(5,2) | Percentage of free space |
| DayNumber | INT | Days since first snapshot for this datastore (for regression) |
| ImportBatchId | INT | Import batch reference |

## DayNumber Calculation

The DayNumber column is calculated using a window function to enable linear regression:

```sql
DATEDIFF(DAY,
    MIN(CAST(ValidFrom AS DATE)) OVER (PARTITION BY Name, VI_SDK_Server),
    CAST(ValidFrom AS DATE)
) AS DayNumber
```

This gives each datastore's snapshots a sequence starting at 0, making it suitable for linear regression where DayNumber is the X-axis.

## Sample Queries

**Storage trend for a specific datastore:**
```sql
SELECT SnapshotDate, DatastoreName, In_Use_MiB, Free_MiB, Free_Percent, DayNumber
FROM [Reporting].[vw_Trends_Storage_Growth]
WHERE DatastoreName = 'VMFS-Prod-01'
ORDER BY SnapshotDate;
```

**Datastores with sufficient data points for regression (3+ snapshots):**
```sql
SELECT DatastoreName, VI_SDK_Server,
       COUNT(*) AS DataPoints,
       MIN(SnapshotDate) AS FirstSnapshot,
       MAX(SnapshotDate) AS LastSnapshot
FROM [Reporting].[vw_Trends_Storage_Growth]
GROUP BY DatastoreName, VI_SDK_Server
HAVING COUNT(*) >= 3
ORDER BY DataPoints DESC;
```

**Recent storage growth (last 30 days):**
```sql
SELECT DatastoreName, SnapshotDate, In_Use_MiB, Free_Percent
FROM [Reporting].[vw_Trends_Storage_Growth]
WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
ORDER BY DatastoreName, SnapshotDate;
```

## Related Reports

- [Datastore Capacity Trend](./Datastore_Capacity_Trend.md) - Historical capacity data
- [Datastore Capacity](./Datastore_Capacity.md) - Current capacity snapshot

## Notes

- The view requires multiple RVTools imports over time to show meaningful trends.
- DayNumber starts at 0 for each datastore's first recorded snapshot.
- Free_Percent may be NULL if not captured in the source data.
- For growth projection calculations, use DayNumber as the independent variable and In_Use_MiB as the dependent variable in linear regression.
