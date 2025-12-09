# VM Lifecycle Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Trends Reports

---

**Category**: Trends
**View**: `[Reporting].[vw_Trends_VM_Lifecycle]`
**RDL File**: `src/reports/Trends/VM_Lifecycle.rdl`
**SQL Source**: `src/tsql/Views/Trends/vw_VM_Lifecycle.sql`

## Purpose

Tracks VM power state changes over time, enabling analysis of uptime patterns and VM lifecycle events.

## Data Source

- **Primary Table**: `History.vInfo`
- **Update Frequency**: Historical data accumulated over multiple RVTools imports
- **Filter**: Excludes templates (Template = 0)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VM | NVARCHAR | Virtual machine name |
| VM_UUID | NVARCHAR | VM unique identifier |
| VI_SDK_Server | NVARCHAR | vCenter server |
| Datacenter | NVARCHAR | Datacenter |
| Cluster | NVARCHAR | Cluster |
| Host | NVARCHAR | ESXi host |
| Resource_pool | NVARCHAR | Resource pool |
| Powerstate | NVARCHAR | Power state (poweredOn, poweredOff, suspended) |
| State_Start_Date | DATE | Date this power state began |
| State_End_Date | DATE | Date this power state ended (current date if ongoing) |
| Days_In_State | INT | Number of days in this power state |
| Last_PowerOn_Time | DATETIME | Last power-on timestamp from vInfo.PowerOn |
| Template | BIT | Template flag (always 0 in this view) |
| OS_according_to_the_VMware_Tools | NVARCHAR | Guest OS |
| ImportBatchId | INT | Import batch reference |
| ValidFrom | DATETIME | SCD Type 2 valid-from timestamp |
| ValidTo | DATETIME | SCD Type 2 valid-to timestamp (NULL if current) |

## Days_In_State Calculation

```sql
DATEDIFF(DAY,
    CAST(ValidFrom AS DATE),
    CAST(COALESCE(ValidTo, GETUTCDATE()) AS DATE)
) AS Days_In_State
```

For current records (ValidTo IS NULL), the calculation uses GETUTCDATE() as the end date.

## Sample Queries

**Recent power state changes:**
```sql
SELECT VM, Powerstate, State_Start_Date, State_End_Date, Days_In_State
FROM [Reporting].[vw_Trends_VM_Lifecycle]
WHERE State_Start_Date >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
ORDER BY VM, State_Start_Date DESC;
```

**VM uptime percentage (last 30 days):**
```sql
SELECT VM,
       SUM(CASE WHEN Powerstate = 'poweredOn' THEN Days_In_State ELSE 0 END) AS Days_On,
       SUM(Days_In_State) AS Total_Days,
       CASE WHEN SUM(Days_In_State) > 0 THEN
           CAST(SUM(CASE WHEN Powerstate = 'poweredOn' THEN Days_In_State ELSE 0 END) * 100.0 /
                SUM(Days_In_State) AS DECIMAL(5,2))
       ELSE NULL END AS Uptime_Percent
FROM [Reporting].[vw_Trends_VM_Lifecycle]
WHERE State_Start_Date >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
GROUP BY VM
ORDER BY Uptime_Percent;
```

**VMs currently powered off:**
```sql
SELECT VM, Cluster, State_Start_Date, Days_In_State
FROM [Reporting].[vw_Trends_VM_Lifecycle]
WHERE Powerstate = 'poweredOff'
  AND ValidTo IS NULL
ORDER BY Days_In_State DESC;
```

**VMs with frequent power state changes:**
```sql
SELECT VM, COUNT(*) AS State_Changes
FROM [Reporting].[vw_Trends_VM_Lifecycle]
WHERE State_Start_Date >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
GROUP BY VM
HAVING COUNT(*) > 5
ORDER BY State_Changes DESC;
```

## Related Reports

- [VM Count Trend](./VM_Count_Trend.md) - VM count growth over time
- [VM Config Changes](./VM_Config_Changes.md) - Configuration drift history

## Notes

- The view uses SCD Type 2 history; each row represents a period when the VM was in a specific power state.
- Days_In_State may overlap across records if a VM changed state multiple times on the same day.
- Templates are excluded from this view.
- The view requires multiple RVTools imports over time to show meaningful lifecycle data.
