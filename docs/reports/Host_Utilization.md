# Host Utilization Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Trends Reports

---

**Category**: Trends
**View**: `[Reporting].[vw_Trends_Host_Utilization]`
**RDL File**: `src/reports/Trends/Host_Utilization.rdl`
**SQL Source**: `src/tsql/Views/Trends/vw_Host_Utilization.sql`

## Purpose

Tracks historical ESXi host CPU and memory utilization over time, enabling trend analysis and capacity planning.

## Data Source

- **Primary Table**: `History.vHost`
- **Update Frequency**: Historical data accumulated over multiple RVTools imports
- **Filter**: Includes records where ValidTo IS NULL (current) or ValidTo > ValidFrom (historical)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| SnapshotDate | DATE | Date of the data snapshot (cast from ValidFrom) |
| HostName | NVARCHAR | ESXi host FQDN |
| VI_SDK_Server | NVARCHAR | vCenter server |
| Datacenter | NVARCHAR | Datacenter |
| Cluster | NVARCHAR | Cluster |
| CPU_Usage_Percent | DECIMAL(5,2) | CPU utilization percentage |
| Physical_CPUs | INT | Number of physical CPU sockets |
| Cores_per_CPU | INT | Cores per CPU socket |
| Total_Cores | INT | Total physical cores |
| Total_vCPUs | INT | Total vCPUs allocated to VMs on this host |
| vCPU_to_Core_Ratio | DECIMAL(5,2) | vCPU:Core consolidation ratio |
| Memory_Usage_Percent | DECIMAL(5,2) | Memory utilization percentage |
| Physical_Memory_MiB | BIGINT | Physical memory installed |
| Allocated_vMemory_MiB | BIGINT | vRAM allocated to VMs |
| VM_Count | INT | Number of VMs on this host |
| VMs_per_Core | DECIMAL(5,2) | VM density per physical core |
| in_Maintenance_Mode | NVARCHAR | Maintenance mode status |
| ESX_Version | NVARCHAR | VMware ESXi version |
| ImportBatchId | INT | Import batch reference |

## Sample Queries

**Host utilization over last 30 days:**
```sql
SELECT SnapshotDate, HostName, CPU_Usage_Percent, Memory_Usage_Percent, VM_Count
FROM [Reporting].[vw_Trends_Host_Utilization]
WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
  AND in_Maintenance_Mode = '0'
ORDER BY HostName, SnapshotDate;
```

**Average utilization by host:**
```sql
SELECT HostName, Cluster,
       AVG(CPU_Usage_Percent) AS Avg_CPU_Percent,
       AVG(Memory_Usage_Percent) AS Avg_Memory_Percent,
       AVG(VM_Count) AS Avg_VM_Count
FROM [Reporting].[vw_Trends_Host_Utilization]
WHERE SnapshotDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
GROUP BY HostName, Cluster
ORDER BY Avg_CPU_Percent DESC;
```

**Hosts with high consolidation ratios:**
```sql
SELECT DISTINCT HostName, Cluster, Total_Cores, Total_vCPUs, vCPU_to_Core_Ratio
FROM [Reporting].[vw_Trends_Host_Utilization]
WHERE vCPU_to_Core_Ratio > 4
ORDER BY vCPU_to_Core_Ratio DESC;
```

## Related Reports

- [Host Capacity](./Host_Capacity.md) - Current host capacity snapshot
- [Host Inventory](./Host_Inventory.md) - Full host specifications

## Notes

- The view requires multiple RVTools imports over time to show meaningful trends.
- CPU_Usage_Percent and Memory_Usage_Percent may be NULL if not captured in the source data.
- Hosts in maintenance mode can be filtered using `in_Maintenance_Mode = '0'`.
- Each snapshot date may have multiple records per host if the host data changed during that day.
