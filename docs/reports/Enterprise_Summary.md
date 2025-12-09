# Enterprise Summary Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Inventory Reports

---

**Category**: Inventory
**View**: `[Reporting].[vw_MultiVCenter_Enterprise_Summary]`
**RDL File**: `src/reports/Inventory/Enterprise_Summary.rdl`
**SQL Source**: `src/tsql/Views/Inventory/vw_Enterprise_Summary.sql`

## Purpose

Provides an aggregated summary of VM counts and resource allocation across all vCenters, with one row per vCenter server.

## Data Source

- **Primary Table**: `Current.vInfo`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)
- **Grouping**: Aggregated by VI_SDK_Server

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VI_SDK_Server | NVARCHAR | vCenter server (COALESCE to 'Unknown' if NULL) |
| VMs_PoweredOn | INT | Count of powered-on VMs (excludes templates) |
| VMs_PoweredOff | INT | Count of powered-off VMs (excludes templates) |
| Templates | INT | Count of templates |
| Total_VMs | INT | Total VM count (excludes templates) |
| Total_vCPUs | INT | Sum of vCPUs for powered-on VMs |
| Total_vMemory_MiB | BIGINT | Sum of memory (MiB) for powered-on VMs |
| Total_Provisioned_MiB | BIGINT | Sum of provisioned storage across all VMs |
| Total_InUse_MiB | BIGINT | Sum of in-use storage across all VMs |
| Cluster_Count | INT | Count of distinct clusters |
| Host_Count | INT | Count of distinct hosts |
| Datacenter_Count | INT | Count of distinct datacenters |
| Latest_ImportBatchId | INT | Most recent import batch ID |
| Latest_Import_Date | DATETIME | Most recent import timestamp |

## Aggregation Logic

- **VM Counts**: Uses CASE expressions to categorize by Template flag and Powerstate
- **Resource Totals**: Only sums resources for powered-on, non-template VMs
- **Storage Totals**: Sums across all VMs regardless of power state

## Sample Queries

**Enterprise-wide summary:**
```sql
SELECT VI_SDK_Server, Total_VMs, VMs_PoweredOn,
       Total_vCPUs,
       Total_vMemory_MiB / 1024.0 AS Total_vMemory_GiB,
       Cluster_Count, Host_Count
FROM [Reporting].[vw_MultiVCenter_Enterprise_Summary]
ORDER BY Total_VMs DESC;
```

**Total resources across all vCenters:**
```sql
SELECT
    SUM(Total_VMs) AS Enterprise_Total_VMs,
    SUM(VMs_PoweredOn) AS Enterprise_PoweredOn,
    SUM(Total_vCPUs) AS Enterprise_vCPUs,
    SUM(Total_vMemory_MiB) / 1024.0 AS Enterprise_Memory_GiB,
    SUM(Host_Count) AS Enterprise_Hosts
FROM [Reporting].[vw_MultiVCenter_Enterprise_Summary];
```

## Related Reports

- [VM Inventory](./VM_Inventory.md) - Detailed VM information across all vCenters
- [Resource Pool Utilization](./ResourcePool_Utilization.md) - Resource pool breakdown by vCenter

## Notes

- Each row represents one vCenter server.
- vCPU and memory totals only include powered-on VMs to reflect actual resource consumption.
- Storage totals include all VMs regardless of power state.
- If VI_SDK_Server is NULL in source data, it appears as 'Unknown'.
