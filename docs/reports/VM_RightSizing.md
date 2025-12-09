# VM Right-Sizing Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Capacity Reports

---

**Category**: Capacity
**View**: `[Reporting].[vw_Capacity_VM_RightSizing]`
**RDL File**: `src/reports/Capacity/VM_RightSizing.rdl`
**SQL Source**: `src/tsql/Views/Capacity/vw_VM_RightSizing.sql`

## Purpose

Identifies over-provisioned VMs by comparing allocated resources (CPU, memory) against actual usage metrics, enabling cost optimization through right-sizing.

## Data Source

- **Primary Table**: `Current.vInfo`
- **Joined Tables**: `Current.vCPU`, `Current.vMemory`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)
- **Filter**: Only powered-on VMs (excludes templates and powered-off VMs)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VM | NVARCHAR | Virtual machine name |
| VM_UUID | NVARCHAR | VM unique identifier |
| VI_SDK_Server | NVARCHAR | vCenter server |
| Powerstate | NVARCHAR | VM power state (always 'poweredOn' in this view) |
| Datacenter | NVARCHAR | Datacenter location |
| Cluster | NVARCHAR | Cluster membership |
| Host | NVARCHAR | ESXi host running the VM |
| Resource_pool | NVARCHAR | Resource pool assignment |
| CPU_Allocated | INT | Number of vCPUs allocated |
| CPU_Readiness_Percent | DECIMAL(5,2) | CPU readiness percentage (high values indicate CPU contention) |
| CPU_Reservation_MHz | INT | CPU reservation in MHz |
| Memory_Allocated_MiB | BIGINT | Memory allocated in MiB |
| Memory_Active_MiB | BIGINT | Active memory in MiB |
| Memory_Consumed_MiB | BIGINT | Consumed memory in MiB |
| Memory_Ballooned_MiB | BIGINT | Ballooned memory in MiB |
| Memory_Reservation_MiB | BIGINT | Memory reservation in MiB |
| Memory_Active_Percent | DECIMAL(5,2) | Percentage of allocated memory that is active |
| Memory_Reservation_Percent | DECIMAL(5,2) | Percentage of memory reserved |
| OS_according_to_the_VMware_Tools | NVARCHAR | Guest OS reported by VMware Tools |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Key Metrics for Right-Sizing

- **Memory_Active_Percent**: Low values (<50%) indicate over-provisioned memory
- **CPU_Readiness_Percent**: High values (>5%) indicate CPU contention; low values may indicate over-provisioning

## Sample Queries

**VMs with low memory utilization (candidates for downsizing):**
```sql
SELECT VM, Cluster, CPU_Allocated,
       Memory_Allocated_MiB / 1024.0 AS Memory_GB,
       Memory_Active_Percent
FROM [Reporting].[vw_Capacity_VM_RightSizing]
WHERE Memory_Active_Percent < 30
ORDER BY Memory_Allocated_MiB DESC;
```

**Top 10 largest VMs by memory allocation:**
```sql
SELECT TOP 10 VM, CPU_Allocated,
       Memory_Allocated_MiB / 1024.0 AS Memory_GB,
       Memory_Active_Percent,
       CPU_Readiness_Percent
FROM [Reporting].[vw_Capacity_VM_RightSizing]
ORDER BY Memory_Allocated_MiB DESC;
```

**VMs with high CPU readiness (potential CPU starvation):**
```sql
SELECT VM, Cluster, Host, CPU_Allocated, CPU_Readiness_Percent
FROM [Reporting].[vw_Capacity_VM_RightSizing]
WHERE CPU_Readiness_Percent > 5
ORDER BY CPU_Readiness_Percent DESC;
```

## Related Reports

- [VM Resource Allocation](./VM_Resource_Allocation.md) - Full VM resource details
- [VM Inventory](./VM_Inventory.md) - VM specifications

## Notes

- Memory_Active_Percent may be NULL if vMemory data is not available for a VM.
- The view only includes powered-on VMs; powered-off VMs and templates are excluded.
- CPU_Readiness_Percent is taken from `vInfo.Overall_Cpu_Readiness`.
