# Resource Pool Utilization Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Inventory Reports

---

**Category**: Inventory
**View**: `[Reporting].[vw_MultiVCenter_ResourcePool_Utilization]`
**RDL File**: `src/reports/Inventory/ResourcePool_Utilization.rdl`
**SQL Source**: `src/tsql/Views/Inventory/vw_ResourcePool_Utilization.sql`

## Purpose

Aggregates VM resources by resource pool across vCenters, showing CPU and memory allocation and utilization per pool.

## Data Source

- **Primary Table**: `Current.vInfo`
- **Joined Tables**: `Current.vCPU`, `Current.vMemory`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)
- **Grouping**: Aggregated by Resource_pool, VI_SDK_Server, Datacenter, Cluster
- **Filter**: Excludes templates (Template = 0)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| ResourcePool | NVARCHAR | Resource pool name (defaults to 'Resources' if NULL) |
| VI_SDK_Server | NVARCHAR | vCenter server |
| Datacenter | NVARCHAR | Datacenter |
| Cluster | NVARCHAR | Cluster |
| VMs_PoweredOn | INT | Count of powered-on VMs in this pool |
| Total_VMs | INT | Total VM count in this pool |
| Total_vCPUs | INT | Sum of vCPUs for powered-on VMs |
| Total_CPU_Reservation_MHz | BIGINT | Sum of CPU reservations (MHz) |
| Total_CPU_Limit_MHz | BIGINT | Sum of CPU limits (MHz) |
| Total_Memory_MiB | BIGINT | Sum of memory allocated (MiB) |
| Total_Active_Memory_MiB | BIGINT | Sum of active memory (MiB) |
| Total_Memory_Reservation_MiB | BIGINT | Sum of memory reservations (MiB) |
| Total_Memory_Limit_MiB | BIGINT | Sum of memory limits (MiB) |
| Avg_Memory_Active_Percent | DECIMAL(5,2) | Average memory active percentage across pool |
| ImportBatchId | INT | Most recent import batch ID |
| LastModifiedDate | DATETIME | Most recent import timestamp |

## Aggregation Logic

- Resource pool name defaults to 'Resources' when vInfo.Resource_pool is NULL
- CPU and memory metrics only aggregate powered-on VMs
- Avg_Memory_Active_Percent = (Total_Active_Memory_MiB / Total_Memory_MiB) * 100

## Sample Queries

**Resource pool summary:**
```sql
SELECT ResourcePool, VI_SDK_Server, Cluster,
       Total_VMs, VMs_PoweredOn, Total_vCPUs,
       Total_Memory_MiB / 1024.0 AS Memory_GiB,
       Avg_Memory_Active_Percent
FROM [Reporting].[vw_MultiVCenter_ResourcePool_Utilization]
ORDER BY Total_Memory_MiB DESC;
```

**Pools with low memory utilization:**
```sql
SELECT ResourcePool, Cluster, Total_VMs,
       Total_Memory_MiB / 1024.0 AS Memory_GiB,
       Avg_Memory_Active_Percent
FROM [Reporting].[vw_MultiVCenter_ResourcePool_Utilization]
WHERE Avg_Memory_Active_Percent < 30
ORDER BY Total_Memory_MiB DESC;
```

## Related Reports

- [VM Resource Allocation](./VM_Resource_Allocation.md) - Per-VM resource details
- [Enterprise Summary](./Enterprise_Summary.md) - vCenter-level totals

## Notes

- Resource pool data comes from the vInfo.Resource_pool column, not a separate vResourcePool table.
- If no resource pool is assigned, the VM appears under 'Resources' (default pool name).
- CPU and memory limits use the reserved word `[Limit]` which is properly bracketed in the view.
