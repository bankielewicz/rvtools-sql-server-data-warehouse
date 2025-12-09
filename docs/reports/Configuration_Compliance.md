# Configuration Compliance Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Health Reports

---

**Category**: Health
**View**: `[Reporting].[vw_Health_Configuration_Compliance]`
**RDL File**: `src/reports/Health/Configuration_Compliance.rdl`
**SQL Source**: `src/tsql/Views/Health/vw_Configuration_Compliance.sql`

## Purpose

Validates VMs against four configuration compliance standards and provides an overall compliance status.

## Data Source

- **Primary Table**: `Current.vInfo`
- **Joined Tables**: `Current.vCPU`, `Current.vMemory`, `Current.vHost`, `Current.vTools`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)
- **Filter**: Excludes templates (Template = 0)

## Compliance Checks

| Check | Rule | Compliant When |
|-------|------|----------------|
| vCPU Ratio | vCPU:Physical Core ratio | <= 4:1 |
| Memory Reservation | Memory reservation percentage | >= 50% |
| Boot Delay | VM boot delay | >= 10 seconds |
| VMware Tools | Tools status and upgradeable flag | Tools is 'toolsOk' or 'toolsOld' AND Upgradeable = '0' |

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VM | NVARCHAR | Virtual machine name |
| VM_UUID | NVARCHAR | VM unique identifier |
| VI_SDK_Server | NVARCHAR | vCenter server |
| Powerstate | NVARCHAR | VM power state |
| Datacenter | NVARCHAR | Datacenter |
| Cluster | NVARCHAR | Cluster |
| Host | NVARCHAR | ESXi host |
| Resource_pool | NVARCHAR | Resource pool |
| OS_according_to_the_VMware_Tools | NVARCHAR | Guest OS |
| Annotation | NVARCHAR | VM notes/annotation |
| CPU_Count | INT | Number of vCPUs |
| Host_Physical_Cores | INT | Physical cores on the host |
| vCPU_to_Core_Ratio | DECIMAL(5,2) | Calculated vCPU:Core ratio |
| Memory_Allocated_MiB | BIGINT | Memory allocated |
| Memory_Reservation_MiB | BIGINT | Memory reservation |
| Memory_Reservation_Percent | DECIMAL(5,2) | Reservation as % of allocated |
| Boot_Delay_Seconds | INT | Boot delay setting |
| Tools_Status | NVARCHAR | VMware Tools status |
| Tools_Version | NVARCHAR | Tools version |
| Required_Version | NVARCHAR | Required Tools version |
| Tools_Upgradeable | NVARCHAR | Whether Tools can be upgraded |
| vCPU_Ratio_Compliant | BIT | 1 = compliant, 0 = non-compliant |
| Memory_Reservation_Compliant | BIT | 1 = compliant, 0 = non-compliant |
| Boot_Delay_Compliant | BIT | 1 = compliant, 0 = non-compliant |
| Tools_Compliant | BIT | 1 = compliant, 0 = non-compliant |
| Overall_Compliance_Status | NVARCHAR | 'Compliant' or 'Non-Compliant' |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Overall Compliance Logic

A VM is 'Compliant' only when ALL four checks pass:

```sql
CASE
    WHEN (
        (h.Num_Cores > 0 AND c.CPUs / h.Num_Cores <= 4) AND
        (m.Size_MiB > 0 AND (m.Reservation * 100.0 / m.Size_MiB) >= 50) AND
        (i.Boot_delay >= 10) AND
        (t.Tools IN ('toolsOk', 'toolsOld') AND t.Upgradeable = '0')
    ) THEN 'Compliant'
    ELSE 'Non-Compliant'
END
```

## Sample Queries

**Compliance summary:**
```sql
SELECT Overall_Compliance_Status, COUNT(*) AS VM_Count
FROM [Reporting].[vw_Health_Configuration_Compliance]
WHERE Powerstate = 'poweredOn'
GROUP BY Overall_Compliance_Status;
```

**Non-compliant VMs with details:**
```sql
SELECT VM, Cluster, vCPU_to_Core_Ratio, Memory_Reservation_Percent,
       Boot_Delay_Seconds, Tools_Status, Overall_Compliance_Status
FROM [Reporting].[vw_Health_Configuration_Compliance]
WHERE Overall_Compliance_Status = 'Non-Compliant'
  AND Powerstate = 'poweredOn'
ORDER BY VM;
```

**VMs failing specific checks:**
```sql
-- VMs with high vCPU:Core ratio
SELECT VM, Host, CPU_Count, Host_Physical_Cores, vCPU_to_Core_Ratio
FROM [Reporting].[vw_Health_Configuration_Compliance]
WHERE vCPU_Ratio_Compliant = 0;

-- VMs with insufficient memory reservation
SELECT VM, Memory_Allocated_MiB, Memory_Reservation_MiB, Memory_Reservation_Percent
FROM [Reporting].[vw_Health_Configuration_Compliance]
WHERE Memory_Reservation_Compliant = 0;
```

## Related Reports

- [Health Issues](./Health_Issues.md) - Active health problems
- [Tools Status](./Tools_Status.md) - VMware Tools compliance detail
- [VM Inventory](./VM_Inventory.md) - Full VM specifications

## Notes

- The compliance thresholds are hardcoded in the view (4:1 ratio, 50% reservation, 10s boot delay).
- Host_Physical_Cores comes from a CTE joining to vHost.
- Tools compliance checks for Upgradeable = '0' (string, not integer).
- VMs missing data in joined tables may show NULL for some metrics and will be marked non-compliant.
