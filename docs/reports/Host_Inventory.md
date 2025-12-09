# Host Inventory Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Inventory Reports

---

**Category**: Inventory
**View**: `[Reporting].[vw_Host_Inventory]`
**RDL File**: `src/reports/Inventory/Host_Inventory.rdl`
**SQL Source**: `src/tsql/Views/Inventory/vw_Host_Inventory.sql`

## Purpose

Provides a comprehensive inventory of all ESXi hosts with hardware specifications, CPU and memory details, software versions, and certificate status.

## Data Source

- **Primary Table**: `Current.vHost`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| Host | NVARCHAR | ESXi host FQDN |
| UUID | NVARCHAR | Host unique identifier |
| VI_SDK_Server | NVARCHAR | vCenter server managing this host |
| Datacenter | NVARCHAR | Datacenter location |
| Cluster | NVARCHAR | Cluster membership |
| Config_status | NVARCHAR | Configuration status |
| in_Maintenance_Mode | NVARCHAR | Whether host is in maintenance mode |
| in_Quarantine_Mode | NVARCHAR | Whether host is in quarantine mode |
| CPU_Model | NVARCHAR | CPU model name |
| Speed | NVARCHAR | CPU speed in MHz |
| Num_CPU | INT | Number of physical CPU sockets |
| Cores_per_CPU | INT | Cores per CPU socket |
| Num_Cores | INT | Total physical cores |
| HT_Available | NVARCHAR | Whether hyperthreading is available |
| HT_Active | NVARCHAR | Whether hyperthreading is active |
| Num_Memory | BIGINT | Total physical memory in MB |
| Num_NICs | INT | Number of physical network adapters |
| Num_HBAs | INT | Number of host bus adapters |
| Num_VMs | INT | Number of VMs on this host |
| Num_vCPUs | INT | Total vCPUs allocated to VMs |
| vCPUs_per_Core | NVARCHAR | vCPU to physical core ratio |
| vRAM | NVARCHAR | Total vRAM allocated to VMs |
| ESX_Version | NVARCHAR | VMware ESXi version and build |
| Current_EVC | NVARCHAR | Current EVC mode |
| Max_EVC | NVARCHAR | Maximum supported EVC mode |
| Vendor | NVARCHAR | Server hardware vendor |
| Model | NVARCHAR | Server model |
| Serial_number | NVARCHAR | Server serial number |
| Service_tag | NVARCHAR | Dell service tag or equivalent |
| BIOS_Version | NVARCHAR | BIOS version |
| BIOS_Date | NVARCHAR | BIOS release date |
| Boot_time | NVARCHAR | Last host boot timestamp |
| Time_Zone_Name | NVARCHAR | Host time zone |
| Certificate_Expiry_Date | NVARCHAR | SSL certificate expiration date |
| Certificate_Status | NVARCHAR | Certificate status |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Sample Queries

**Complete host inventory:**
```sql
SELECT Host, Cluster, ESX_Version, Vendor, Model,
       Num_CPU, Cores_per_CPU, Num_Cores,
       Num_Memory / 1024.0 AS Memory_GB,
       Num_VMs
FROM [Reporting].[vw_Host_Inventory]
ORDER BY Cluster, Host;
```

**Hosts by ESXi version:**
```sql
SELECT ESX_Version, COUNT(*) AS Host_Count,
       SUM(Num_Cores) AS Total_Cores,
       SUM(Num_Memory) / 1024.0 AS Total_Memory_GB
FROM [Reporting].[vw_Host_Inventory]
GROUP BY ESX_Version
ORDER BY Host_Count DESC;
```

**Hardware inventory summary:**
```sql
SELECT Vendor, Model, COUNT(*) AS Host_Count,
       AVG(Num_Cores) AS Avg_Cores,
       AVG(Num_Memory) / 1024.0 AS Avg_Memory_GB
FROM [Reporting].[vw_Host_Inventory]
GROUP BY Vendor, Model
ORDER BY Host_Count DESC;
```

**Hosts in maintenance or quarantine:**
```sql
SELECT Host, Cluster, in_Maintenance_Mode, in_Quarantine_Mode
FROM [Reporting].[vw_Host_Inventory]
WHERE in_Maintenance_Mode = '1' OR in_Quarantine_Mode = '1'
ORDER BY Cluster, Host;
```

**CPU and memory allocation ratios:**
```sql
SELECT Host, Cluster, Num_Cores, Num_vCPUs, vCPUs_per_Core,
       Num_Memory / 1024.0 AS Memory_GB,
       TRY_CAST(vRAM AS BIGINT) / 1024.0 AS vRAM_GB,
       Num_VMs
FROM [Reporting].[vw_Host_Inventory]
WHERE in_Maintenance_Mode = '0'
ORDER BY TRY_CAST(vCPUs_per_Core AS DECIMAL(5,2)) DESC;
```

## Related Reports

- [VM Inventory](./VM_Inventory.md) - VMs running on these hosts
- [Host Capacity](./Host_Capacity.md) - Host resource utilization metrics
- [Cluster Summary](./Cluster_Summary.md) - Cluster-level aggregation

## Notes

- Num_Memory is in MB; divide by 1024 for GB.
- vCPUs_per_Core ratios above 4:1 may indicate over-subscription.
- EVC mode affects vMotion compatibility between hosts.
- Hosts in maintenance mode don't run production VMs but remain in the cluster.
- Boot_time indicates the last host reboot; long uptimes may indicate missed patches.
- The view queries `Current.vHost`, so data reflects the most recent RVTools import.
