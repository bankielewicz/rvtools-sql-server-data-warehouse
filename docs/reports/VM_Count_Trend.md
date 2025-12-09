# VM Count Trend Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Trends Reports

---

**Category**: Trends
**View**: `[Reporting].[vw_VM_Count_Trend]`
**RDL File**: `src/reports/Trends/VM_Count_Trend.rdl`
**SQL Source**: `src/tsql/Views/Trends/vw_VM_Count_Trend.sql`

## Purpose

Tracks virtual machine count growth over time with breakdown by power state, enabling trend analysis for capacity planning and VM sprawl monitoring.

## Data Source

- **Primary Table**: `History.vInfo`
- **Update Frequency**: Historical data accumulated over multiple RVTools imports
- **Filter**: Includes records where ValidTo IS NULL (current) or ValidTo > ValidFrom (historical)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| SnapshotDate | DATE | Date of the data snapshot (cast from ValidFrom) |
| VI_SDK_Server | NVARCHAR | vCenter server |
| VMCount | INT | Total number of VMs |
| TemplateCount | INT | Number of templates |
| PoweredOnCount | INT | Number of powered-on VMs |
| PoweredOffCount | INT | Number of powered-off VMs |
| SuspendedCount | INT | Number of suspended VMs |
| ImportBatchId | INT | Import batch reference (minimum for the day) |

## Sample Queries

**VM count trend over last 30 days:**
```sql
SELECT SnapshotDate, VI_SDK_Server,
       VMCount, PoweredOnCount, PoweredOffCount, TemplateCount
FROM [Reporting].[vw_VM_Count_Trend]
WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
ORDER BY VI_SDK_Server, SnapshotDate DESC;
```

**Total VM count across all vCenters by date:**
```sql
SELECT SnapshotDate,
       SUM(VMCount) AS Total_VMs,
       SUM(PoweredOnCount) AS Total_PoweredOn,
       SUM(PoweredOffCount) AS Total_PoweredOff,
       SUM(TemplateCount) AS Total_Templates
FROM [Reporting].[vw_VM_Count_Trend]
WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
GROUP BY SnapshotDate
ORDER BY SnapshotDate DESC;
```

**VM growth rate analysis:**
```sql
WITH DatePairs AS (
    SELECT
        SnapshotDate,
        VI_SDK_Server,
        VMCount,
        LAG(VMCount) OVER (PARTITION BY VI_SDK_Server ORDER BY SnapshotDate) AS PrevVMCount
    FROM [Reporting].[vw_VM_Count_Trend]
)
SELECT SnapshotDate, VI_SDK_Server,
       VMCount, PrevVMCount,
       VMCount - ISNULL(PrevVMCount, VMCount) AS DailyChange
FROM DatePairs
WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
  AND PrevVMCount IS NOT NULL
ORDER BY SnapshotDate DESC;
```

**Powered-off VM ratio over time:**
```sql
SELECT SnapshotDate,
       VMCount,
       PoweredOffCount,
       CAST(PoweredOffCount AS DECIMAL(10,2)) / NULLIF(VMCount, 0) * 100 AS PoweredOff_Percent
FROM [Reporting].[vw_VM_Count_Trend]
WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
ORDER BY SnapshotDate DESC;
```

**Monthly VM count summary:**
```sql
SELECT
    YEAR(SnapshotDate) AS Year,
    MONTH(SnapshotDate) AS Month,
    VI_SDK_Server,
    AVG(VMCount) AS Avg_VMCount,
    MAX(VMCount) AS Max_VMCount,
    MIN(VMCount) AS Min_VMCount
FROM [Reporting].[vw_VM_Count_Trend]
GROUP BY YEAR(SnapshotDate), MONTH(SnapshotDate), VI_SDK_Server
ORDER BY Year DESC, Month DESC, VI_SDK_Server;
```

## Related Reports

- [VM Inventory](./VM_Inventory.md) - Current VM snapshot
- [VM Lifecycle](./VM_Lifecycle.md) - VM power state history

## Notes

- The view requires multiple RVTools imports over time to show meaningful trends.
- VMCount includes all VMs regardless of power state; TemplateCount is separate.
- High PoweredOffCount over time may indicate VM sprawl or cleanup opportunities.
- Sudden drops in VMCount may indicate VM deletions or data collection issues.
- The view aggregates by date and vCenter using GROUP BY, so each SnapshotDate/VI_SDK_Server combination has one row.
- The view uses History.vInfo which maintains SCD Type 2 historical records.
