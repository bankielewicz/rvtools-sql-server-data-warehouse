# VM Configuration Changes Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Trends Reports

---

**Category**: Trends
**View**: `[Reporting].[vw_VM_Config_Changes]`
**RDL File**: `src/reports/Trends/VM_Config_Changes.rdl`
**SQL Source**: `src/tsql/Views/Trends/vw_VM_Config_Changes.sql`

## Purpose

Tracks configuration drift and changes over time by capturing historical VM configurations, enabling audit trails and change detection for CPU, memory, disk, and other settings.

## Data Source

- **Primary Table**: `History.vInfo`
- **Update Frequency**: Historical data accumulated over multiple RVTools imports
- **Filter**: Only superseded records (ValidTo IS NOT NULL) representing changed or deleted configurations

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VM | NVARCHAR | Virtual machine name |
| VM_UUID | NVARCHAR | VM unique identifier |
| VI_SDK_Server | NVARCHAR | vCenter server |
| EffectiveFrom | DATETIME | When this configuration became active |
| EffectiveUntil | DATETIME | When this configuration was superseded |
| ChangedDate | DATETIME | Date the configuration was changed (alias for EffectiveUntil) |
| Powerstate | NVARCHAR | VM power state at time of configuration |
| CPUs | INT | Number of vCPUs at time of configuration |
| Memory_MB | BIGINT | Memory allocation in MB at time of configuration |
| NICs | INT | Number of virtual NICs at time of configuration |
| Disks | INT | Number of virtual disks at time of configuration |
| Datacenter | NVARCHAR | Datacenter location at time of configuration |
| Cluster | NVARCHAR | Cluster membership at time of configuration |
| Host | NVARCHAR | ESXi host at time of configuration |
| HW_version | NVARCHAR | Virtual hardware version at time of configuration |
| OS_according_to_the_VMware_Tools | NVARCHAR | Guest OS at time of configuration |
| SourceFile | NVARCHAR | RVTools export file that captured this configuration |
| ImportBatchId | INT | Import batch reference |

## Sample Queries

**Recent configuration changes (last 30 days):**
```sql
SELECT VM, ChangedDate, CPUs, Memory_MB / 1024.0 AS Memory_GB,
       Disks, NICs, Cluster
FROM [Reporting].[vw_VM_Config_Changes]
WHERE ChangedDate >= DATEADD(DAY, -30, GETUTCDATE())
ORDER BY ChangedDate DESC;
```

**VMs with CPU or memory changes:**
```sql
SELECT VM, EffectiveFrom, EffectiveUntil,
       CPUs AS PreviousCPUs, Memory_MB / 1024.0 AS PreviousMemory_GB,
       Cluster
FROM [Reporting].[vw_VM_Config_Changes]
WHERE ChangedDate >= DATEADD(DAY, -30, GETUTCDATE())
ORDER BY ChangedDate DESC;
```

**Configuration change frequency by VM:**
```sql
SELECT VM, COUNT(*) AS Change_Count,
       MIN(EffectiveFrom) AS First_Seen,
       MAX(EffectiveUntil) AS Last_Change
FROM [Reporting].[vw_VM_Config_Changes]
GROUP BY VM
HAVING COUNT(*) > 5
ORDER BY Change_Count DESC;
```

**VMs that have been deleted (existed but no longer current):**
```sql
SELECT vc.VM, vc.VM_UUID, vc.ChangedDate AS DeletedDate,
       vc.CPUs, vc.Memory_MB / 1024.0 AS Memory_GB, vc.Cluster
FROM [Reporting].[vw_VM_Config_Changes] vc
WHERE vc.ChangedDate = (
    SELECT MAX(ChangedDate)
    FROM [Reporting].[vw_VM_Config_Changes]
    WHERE VM_UUID = vc.VM_UUID
)
AND NOT EXISTS (
    SELECT 1 FROM [Current].[vInfo] ci
    WHERE ci.VM_UUID = vc.VM_UUID
    AND ci.VI_SDK_Server = vc.VI_SDK_Server
)
ORDER BY vc.ChangedDate DESC;
```

**Hardware version upgrade history:**
```sql
SELECT VM, EffectiveFrom, EffectiveUntil, HW_version, Cluster
FROM [Reporting].[vw_VM_Config_Changes]
WHERE VM = 'MyVM'
ORDER BY EffectiveFrom;
```

## Related Reports

- [VM Inventory](./VM_Inventory.md) - Current VM configurations
- [VM Lifecycle](./VM_Lifecycle.md) - Power state change history

## Notes

- This view only shows superseded (changed) records; current configurations are in `Current.vInfo`.
- EffectiveUntil indicates when the configuration was replaced by a new version.
- To see what a configuration changed TO, compare with the next record for the same VM_UUID.
- Deleted VMs appear as records where no current record exists with the same VM_UUID.
- The view uses History.vInfo which maintains SCD Type 2 historical records with ValidFrom/ValidTo timestamps.
- Multiple changes on the same day may result in multiple records with the same ChangedDate.
