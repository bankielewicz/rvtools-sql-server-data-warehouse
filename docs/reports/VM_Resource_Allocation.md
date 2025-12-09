# VM Resource Allocation Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Capacity Reports

---

**Category**: Capacity
**View**: `[Reporting].[vw_VM_Resource_Allocation]`
**RDL File**: `src/reports/Capacity/VM_Resource_Allocation.rdl`
**SQL Source**: `src/tsql/Views/Capacity/vw_VM_Resource_Allocation.sql`

## Purpose

Provides detailed CPU and memory allocation and consumption metrics for each VM, enabling capacity planning and identifying resource allocation patterns.

## Data Source

- **Primary Table**: `Current.vInfo`
- **Joined Tables**: `Current.vCPU`, `Current.vMemory`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VM | NVARCHAR | Virtual machine name |
| VM_UUID | NVARCHAR | VM unique identifier |
| VI_SDK_Server | NVARCHAR | vCenter server |
| Powerstate | NVARCHAR | VM power state (poweredOn, poweredOff, suspended) |
| Template | NVARCHAR | Whether VM is a template |
| Datacenter | NVARCHAR | Datacenter location |
| Cluster | NVARCHAR | Cluster membership |
| Host | NVARCHAR | ESXi host running the VM |
| CPU_Count | INT | Number of vCPUs allocated |
| CPU_Sockets | INT | Number of virtual CPU sockets |
| Cores_Per_Socket | INT | Cores per socket configuration |
| CPU_Overall_MHz | INT | Overall CPU usage in MHz |
| CPU_Reservation_MHz | INT | CPU reservation in MHz |
| CPU_Limit_MHz | INT | CPU limit in MHz (-1 = unlimited) |
| CPU_Shares_Level | NVARCHAR | CPU shares level (low, normal, high, custom) |
| CPU_Hot_Add | NVARCHAR | Whether CPU hot-add is enabled |
| Memory_Size_MiB | BIGINT | Memory allocated in MiB |
| Memory_Consumed_MiB | BIGINT | Memory consumed by VM in MiB |
| Memory_Active_MiB | BIGINT | Active memory in MiB |
| Memory_Ballooned_MiB | BIGINT | Ballooned memory in MiB |
| Memory_Swapped_MiB | BIGINT | Swapped memory in MiB |
| Memory_Reservation_MiB | BIGINT | Memory reservation in MiB |
| Memory_Limit_MiB | BIGINT | Memory limit in MiB (-1 = unlimited) |
| Memory_Shares_Level | NVARCHAR | Memory shares level (low, normal, high, custom) |
| Memory_Hot_Add | NVARCHAR | Whether memory hot-add is enabled |
| OS_according_to_the_VMware_Tools | NVARCHAR | Guest OS reported by VMware Tools |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Sample Queries

**VMs with memory reservations:**
```sql
SELECT VM, Cluster, Memory_Size_MiB / 1024.0 AS Memory_GB,
       Memory_Reservation_MiB / 1024.0 AS Reservation_GB,
       Memory_Shares_Level
FROM [Reporting].[vw_VM_Resource_Allocation]
WHERE Memory_Reservation_MiB > 0
  AND Powerstate = 'poweredOn'
ORDER BY Memory_Reservation_MiB DESC;
```

**VMs with CPU limits configured:**
```sql
SELECT VM, Cluster, CPU_Count, CPU_Limit_MHz, CPU_Shares_Level
FROM [Reporting].[vw_VM_Resource_Allocation]
WHERE CPU_Limit_MHz > 0 AND CPU_Limit_MHz <> -1
ORDER BY CPU_Limit_MHz ASC;
```

**Resource allocation summary by cluster:**
```sql
SELECT Cluster,
       COUNT(*) AS VM_Count,
       SUM(CPU_Count) AS Total_vCPUs,
       SUM(Memory_Size_MiB) / 1024.0 AS Total_Memory_GB,
       SUM(Memory_Reservation_MiB) / 1024.0 AS Total_Reserved_GB,
       SUM(CPU_Reservation_MHz) AS Total_CPU_Reserved_MHz
FROM [Reporting].[vw_VM_Resource_Allocation]
WHERE Powerstate = 'poweredOn' AND Template = 'False'
GROUP BY Cluster
ORDER BY Total_Memory_GB DESC;
```

**VMs with memory pressure indicators:**
```sql
SELECT VM, Cluster, Memory_Size_MiB / 1024.0 AS Memory_GB,
       Memory_Ballooned_MiB / 1024.0 AS Ballooned_GB,
       Memory_Swapped_MiB / 1024.0 AS Swapped_GB
FROM [Reporting].[vw_VM_Resource_Allocation]
WHERE Memory_Ballooned_MiB > 0 OR Memory_Swapped_MiB > 0
ORDER BY Memory_Ballooned_MiB + Memory_Swapped_MiB DESC;
```

## Related Reports

- [VM Inventory](./VM_Inventory.md) - Full VM specifications
- [VM Right-Sizing](./VM_RightSizing.md) - Over-provisioned VM identification
- [Host Capacity](./Host_Capacity.md) - Host-level resource utilization

## Notes

- Memory_Ballooned_MiB and Memory_Swapped_MiB indicate memory pressure; non-zero values suggest the host is reclaiming memory.
- CPU_Limit_MHz and Memory_Limit_MiB of -1 indicate no limit is configured.
- vCPU and vMemory data comes from LEFT JOINs, so some columns may be NULL if the corresponding records don't exist in vCPU or vMemory tables.
- The view includes all VMs regardless of power state; filter on `Powerstate = 'poweredOn'` for active VMs.
