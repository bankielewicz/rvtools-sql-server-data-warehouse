# Datastore Capacity Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Capacity Reports

---

**Category**: Capacity
**View**: `[Reporting].[vw_Datastore_Capacity]`
**RDL File**: `src/reports/Capacity/Datastore_Capacity.rdl`
**SQL Source**: `src/tsql/Views/Capacity/vw_Datastore_Capacity.sql`

## Purpose

Monitors storage capacity utilization across all datastores, identifying those at risk of running out of space with over-provisioning metrics and status indicators.

## Data Source

- **Primary Table**: `Current.vDatastore`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| DatastoreName | NVARCHAR | Datastore name |
| VI_SDK_Server | NVARCHAR | vCenter server managing this datastore |
| Type | NVARCHAR | Datastore type (VMFS, NFS, vSAN, etc.) |
| Cluster_name | NVARCHAR | Storage cluster membership |
| Capacity_MiB | BIGINT | Total datastore capacity in MiB |
| Provisioned_MiB | BIGINT | Total provisioned storage in MiB |
| In_Use_MiB | BIGINT | Actual storage consumed in MiB |
| Free_MiB | BIGINT | Available free space in MiB |
| Free_Percent | DECIMAL(5,2) | Percentage of capacity that is free |
| OverProvisioningPercent | DECIMAL(5,2) | Provisioned storage as percentage of capacity (>100% indicates over-provisioning) |
| CapacityStatus | NVARCHAR | Calculated status: 'Critical', 'Warning', or 'Normal' |
| Num_VMs | INT | Number of VMs using this datastore |
| Num_Hosts | INT | Number of hosts with access to this datastore |
| Accessible | NVARCHAR | Whether the datastore is currently accessible |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Capacity Status Logic

```sql
CASE
    WHEN Free_Percent < 10 THEN 'Critical'
    WHEN Free_Percent < 20 THEN 'Warning'
    ELSE 'Normal'
END
```

## Over-Provisioning Calculation

```sql
(Provisioned_MiB / Capacity_MiB) * 100 AS OverProvisioningPercent
```

Values above 100% indicate the datastore is over-provisioned (thin provisioning in use).

## Sample Queries

**Datastores with critical capacity:**
```sql
SELECT DatastoreName, Type, Capacity_MiB / 1024.0 AS Capacity_GB,
       Free_MiB / 1024.0 AS Free_GB, Free_Percent, CapacityStatus
FROM [Reporting].[vw_Datastore_Capacity]
WHERE CapacityStatus = 'Critical'
ORDER BY Free_Percent ASC;
```

**Over-provisioned datastores:**
```sql
SELECT DatastoreName, Capacity_MiB / 1024.0 AS Capacity_GB,
       Provisioned_MiB / 1024.0 AS Provisioned_GB,
       OverProvisioningPercent, Num_VMs
FROM [Reporting].[vw_Datastore_Capacity]
WHERE OverProvisioningPercent > 150
ORDER BY OverProvisioningPercent DESC;
```

**Capacity summary by datastore type:**
```sql
SELECT Type,
       COUNT(*) AS Datastore_Count,
       SUM(Capacity_MiB) / 1024.0 AS Total_Capacity_GB,
       SUM(Free_MiB) / 1024.0 AS Total_Free_GB,
       AVG(Free_Percent) AS Avg_Free_Percent
FROM [Reporting].[vw_Datastore_Capacity]
GROUP BY Type
ORDER BY Total_Capacity_GB DESC;
```

## Related Reports

- [Datastore Inventory](./Datastore_Inventory.md) - Full datastore details
- [Storage Growth](./Storage_Growth.md) - Storage growth projection
- [Datastore Capacity Trend](./Datastore_Capacity_Trend.md) - Historical capacity trends

## Notes

- Over-provisioning is common with thin-provisioned VMs and is not inherently problematic if actual usage (In_Use_MiB) remains within capacity.
- Datastores with `Accessible = 'False'` may indicate connectivity issues and should be investigated.
- The view queries `Current.vDatastore`, so data reflects the most recent RVTools import.
