# Host Capacity Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Capacity Reports

---

**Category**: Capacity
**View**: `[Reporting].[vw_Host_Capacity]`
**RDL File**: `src/reports/Capacity/Host_Capacity.rdl`
**SQL Source**: `src/tsql/Views/Capacity/vw_Host_Capacity.sql`

## Purpose

Monitors ESXi host CPU and memory utilization with status indicators to identify hosts under stress or approaching capacity limits.

## Data Source

- **Primary Table**: `Current.vHost`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| Host | NVARCHAR | ESXi host FQDN |
| VI_SDK_Server | NVARCHAR | vCenter server managing this host |
| Datacenter | NVARCHAR | Datacenter location |
| Cluster | NVARCHAR | Cluster membership |
| Num_Cores | INT | Total physical CPU cores |
| CPU_Usage_Percent | DECIMAL(5,2) | Current CPU utilization percentage |
| CPUStatus | NVARCHAR | Calculated status: 'Critical', 'Warning', or 'Normal' |
| Memory_MB | BIGINT | Total physical memory in MB |
| Memory_Usage_Percent | DECIMAL(5,2) | Current memory utilization percentage |
| MemoryStatus | NVARCHAR | Calculated status: 'Critical', 'Warning', or 'Normal' |
| Num_VMs | INT | Number of VMs running on this host |
| Num_vCPUs | INT | Total vCPUs allocated to VMs on this host |
| vCPUs_per_Core | DECIMAL(5,2) | vCPU to physical core ratio |
| vRAM_MB | BIGINT | Total vRAM allocated to VMs in MB |
| in_Maintenance_Mode | NVARCHAR | Whether host is in maintenance mode |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Status Threshold Logic

**CPU Status:**
```sql
CASE
    WHEN CPU_Usage_Percent >= 85 THEN 'Critical'
    WHEN CPU_Usage_Percent >= 70 THEN 'Warning'
    ELSE 'Normal'
END
```

**Memory Status:**
```sql
CASE
    WHEN Memory_Usage_Percent >= 85 THEN 'Critical'
    WHEN Memory_Usage_Percent >= 70 THEN 'Warning'
    ELSE 'Normal'
END
```

## Sample Queries

**Hosts with critical resource utilization:**
```sql
SELECT Host, Cluster, CPU_Usage_Percent, CPUStatus,
       Memory_Usage_Percent, MemoryStatus, Num_VMs
FROM [Reporting].[vw_Host_Capacity]
WHERE CPUStatus = 'Critical' OR MemoryStatus = 'Critical'
ORDER BY CPU_Usage_Percent DESC;
```

**Host consolidation ratios:**
```sql
SELECT Host, Cluster, Num_Cores, Num_vCPUs, vCPUs_per_Core,
       Memory_MB / 1024.0 AS Memory_GB,
       vRAM_MB / 1024.0 AS vRAM_GB,
       Num_VMs
FROM [Reporting].[vw_Host_Capacity]
WHERE in_Maintenance_Mode = '0'
ORDER BY vCPUs_per_Core DESC;
```

**Capacity summary by cluster:**
```sql
SELECT Cluster,
       COUNT(*) AS Host_Count,
       SUM(Num_Cores) AS Total_Cores,
       SUM(Memory_MB) / 1024.0 AS Total_Memory_GB,
       AVG(CPU_Usage_Percent) AS Avg_CPU_Percent,
       AVG(Memory_Usage_Percent) AS Avg_Memory_Percent,
       SUM(Num_VMs) AS Total_VMs
FROM [Reporting].[vw_Host_Capacity]
WHERE in_Maintenance_Mode = '0'
GROUP BY Cluster
ORDER BY Total_VMs DESC;
```

## Related Reports

- [Host Inventory](./Host_Inventory.md) - Full host specifications
- [VM Resource Allocation](./VM_Resource_Allocation.md) - VM-level resource details
- [Host Utilization](./Host_Utilization.md) - Historical host utilization trends

## Notes

- Hosts in maintenance mode (`in_Maintenance_Mode = '1'`) should typically be excluded from capacity analysis.
- High vCPUs_per_Core ratios (>4:1) may indicate over-subscription and potential CPU contention.
- CPU_Usage_Percent and Memory_Usage_Percent may be NULL if not captured in the source data.
- The view queries `Current.vHost`, so data reflects the most recent RVTools import.
