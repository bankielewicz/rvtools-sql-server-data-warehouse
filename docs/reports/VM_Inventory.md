# VM Inventory Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Inventory Reports

---

**Category**: Inventory
**View**: `[Reporting].[vw_VM_Inventory]`
**RDL File**: `src/reports/Inventory/VM_Inventory.rdl`
**SQL Source**: `src/tsql/Views/Inventory/vw_VM_Inventory.sql`

## Purpose

Provides a comprehensive inventory of all virtual machines with key specifications including resource allocation, storage consumption, network configuration, and location details.

## Data Source

- **Primary Table**: `Current.vInfo`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VM | NVARCHAR | Virtual machine name |
| VM_UUID | NVARCHAR | VM unique identifier |
| VI_SDK_Server | NVARCHAR | vCenter server managing this VM |
| Powerstate | NVARCHAR | VM power state (poweredOn, poweredOff, suspended) |
| Template | NVARCHAR | Whether VM is a template |
| Config_status | NVARCHAR | VM configuration status |
| Guest_state | NVARCHAR | Guest OS state |
| CPUs | INT | Number of vCPUs allocated |
| Memory | BIGINT | Memory allocated in MB |
| NICs | INT | Number of virtual network adapters |
| Disks | INT | Number of virtual disks |
| Total_disk_capacity_MiB | NVARCHAR | Total virtual disk capacity in MiB |
| Provisioned_MiB | NVARCHAR | Total provisioned storage in MiB |
| In_Use_MiB | NVARCHAR | Actual storage consumed in MiB |
| Primary_IP_Address | NVARCHAR | Primary IP address reported by VMware Tools |
| DNS_Name | NVARCHAR | DNS hostname |
| Datacenter | NVARCHAR | Datacenter location |
| Cluster | NVARCHAR | Cluster membership |
| Host | NVARCHAR | ESXi host running the VM |
| Folder | NVARCHAR | VM folder location in vCenter |
| Resource_pool | NVARCHAR | Resource pool assignment |
| OS_according_to_the_VMware_Tools | NVARCHAR | Guest OS reported by VMware Tools |
| OS_according_to_the_configuration_file | NVARCHAR | Guest OS from VM configuration |
| HW_version | NVARCHAR | Virtual hardware version |
| Firmware | NVARCHAR | Firmware type (BIOS or EFI) |
| Creation_date | NVARCHAR | VM creation date |
| PowerOn | NVARCHAR | Last power-on timestamp |
| Annotation | NVARCHAR | VM notes/description |
| Path | NVARCHAR | VMX file path |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Sample Queries

**Complete VM inventory:**
```sql
SELECT VM, Powerstate, CPUs, Memory / 1024.0 AS Memory_GB,
       OS_according_to_the_VMware_Tools, Cluster, Host
FROM [Reporting].[vw_VM_Inventory]
WHERE Template = 'False'
ORDER BY VM;
```

**VM counts by power state:**
```sql
SELECT Powerstate, COUNT(*) AS VM_Count
FROM [Reporting].[vw_VM_Inventory]
WHERE Template = 'False'
GROUP BY Powerstate
ORDER BY VM_Count DESC;
```

**VMs by operating system:**
```sql
SELECT OS_according_to_the_VMware_Tools AS OS,
       COUNT(*) AS VM_Count,
       SUM(CPUs) AS Total_vCPUs,
       SUM(Memory) / 1024.0 AS Total_Memory_GB
FROM [Reporting].[vw_VM_Inventory]
WHERE Powerstate = 'poweredOn' AND Template = 'False'
GROUP BY OS_according_to_the_VMware_Tools
ORDER BY VM_Count DESC;
```

**VMs by cluster and resource pool:**
```sql
SELECT Cluster, Resource_pool,
       COUNT(*) AS VM_Count,
       SUM(CPUs) AS Total_vCPUs,
       SUM(Memory) / 1024.0 AS Total_Memory_GB
FROM [Reporting].[vw_VM_Inventory]
WHERE Powerstate = 'poweredOn' AND Template = 'False'
GROUP BY Cluster, Resource_pool
ORDER BY Cluster, Resource_pool;
```

**Older hardware versions (upgrade candidates):**
```sql
SELECT VM, HW_version, Powerstate, Cluster
FROM [Reporting].[vw_VM_Inventory]
WHERE TRY_CAST(REPLACE(HW_version, 'vmx-', '') AS INT) < 13
  AND Template = 'False'
ORDER BY HW_version, VM;
```

**VMs without IP addresses (potential tools issues):**
```sql
SELECT VM, Powerstate, Guest_state, DNS_Name, Cluster
FROM [Reporting].[vw_VM_Inventory]
WHERE Primary_IP_Address IS NULL
  AND Powerstate = 'poweredOn'
  AND Template = 'False'
ORDER BY Cluster, VM;
```

## Related Reports

- [Host Inventory](./Host_Inventory.md) - ESXi hosts running these VMs
- [Cluster Summary](./Cluster_Summary.md) - Cluster-level aggregation
- [Tools Status](./Tools_Status.md) - VMware Tools compliance for VMs

## Notes

- Memory is in MB; divide by 1024 for GB.
- Storage columns (Total_disk_capacity_MiB, Provisioned_MiB, In_Use_MiB) are NVARCHAR; use TRY_CAST for numeric operations.
- Primary_IP_Address requires VMware Tools to be running and may be NULL otherwise.
- Hardware versions below vmx-13 (vSphere 6.5) may be candidates for upgrade.
- Templates have `Template = 'True'` and should typically be filtered out of operational queries.
- The view queries `Current.vInfo`, so data reflects the most recent RVTools import.
