# Reports and Views

> Available reporting views and their usage.

**Navigation**: [Home](../../README.md) | [Importing Data](./importing-data.md) | [Querying Data](./querying-data.md)

---

## Overview

The RVTools Data Warehouse includes 24 reporting views organized into four categories. This page covers the **13 core reports**; for the complete set including 11 additional specialized reports, see the [SSRS Reports User Guide](../SSRS_Reports_User_Guide.md).

| Category | Core | Extended | Purpose |
|----------|------|----------|---------|
| Inventory | 4 | 4 | Infrastructure listings |
| Health | 3 | 3 | Issues and compliance |
| Capacity | 3 | 1 | Resource utilization |
| Trends | 3 | 3 | Historical analysis |
| **Total** | **13** | **11** | |

> **Detailed Documentation**: Each report has a complete reference page in [docs/reports/](../reports/) with full column definitions, sample queries, and technical notes.

## Inventory Views

### vw_VM_Inventory

Complete VM listing with specifications.

```sql
SELECT * FROM Reporting.vw_VM_Inventory;
```

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Powerstate | Power state |
| CPUs | vCPU count |
| MemoryMiB | Memory size |
| ProvisionedMiB | Provisioned storage |
| Host | ESXi host |
| Cluster | Cluster name |
| Datacenter | Datacenter |
| OS | Operating system |

> **Full Reference**: [VM Inventory Report Details](../reports/VM_Inventory.md)

### vw_Host_Inventory

ESXi host details.

```sql
SELECT * FROM Reporting.vw_Host_Inventory;
```

| Column | Description |
|--------|-------------|
| Host | Host name |
| ESXVersion | ESXi version |
| CPUModel | CPU model |
| Cores | Total cores |
| MemoryMiB | Total memory |
| VMCount | Number of VMs |
| Cluster | Cluster |
| Datacenter | Datacenter |

> **Full Reference**: [Host Inventory Report Details](../reports/Host_Inventory.md)

### vw_Cluster_Summary

Cluster-level overview.

```sql
SELECT * FROM Reporting.vw_Cluster_Summary;
```

| Column | Description |
|--------|-------------|
| Cluster | Cluster name |
| HostCount | Number of hosts |
| TotalCores | Total CPU cores |
| TotalMemoryMiB | Total memory |
| VMCount | Total VMs |
| HAEnabled | HA status |
| DRSEnabled | DRS status |

> **Full Reference**: [Cluster Summary Report Details](../reports/Cluster_Summary.md)

### vw_Datastore_Inventory

Storage summary.

```sql
SELECT * FROM Reporting.vw_Datastore_Inventory;
```

| Column | Description |
|--------|-------------|
| Datastore | Datastore name |
| Type | VMFS, NFS, etc. |
| CapacityMiB | Total capacity |
| UsedMiB | Used space |
| FreeMiB | Free space |
| FreePercent | Percentage free |
| VMCount | VMs using this datastore |

> **Full Reference**: [Datastore Inventory Report Details](../reports/Datastore_Inventory.md)

### Additional Inventory Reports

See [SSRS Reports User Guide](../SSRS_Reports_User_Guide.md) for full details on these extended reports:

- [Enterprise Summary](../reports/Enterprise_Summary.md) - Multi-vCenter VM and resource aggregation
- [License Compliance](../reports/License_Compliance.md) - License usage vs allocation tracking
- [Network Topology](../reports/Network_Topology.md) - Port groups, VLANs, and switch configuration
- [Resource Pool Utilization](../reports/ResourcePool_Utilization.md) - Resource pool usage metrics

## Health Views

### vw_Health_Issues

Active problems requiring attention.

```sql
SELECT * FROM Reporting.vw_Health_Issues;
```

| Column | Description |
|--------|-------------|
| Name | Object name |
| Message | Issue description |
| MessageType | Info, Warning, Error |
| VIServer | Source vCenter |

> **Full Reference**: [Health Issues Report Details](../reports/Health_Issues.md)

### vw_Snapshot_Aging

Old snapshots consuming storage.

```sql
SELECT * FROM Reporting.vw_Snapshot_Aging;
```

| Column | Description |
|--------|-------------|
| VM | VM name |
| SnapshotName | Snapshot name |
| SnapshotDate | When created |
| AgeDays | Days since creation |
| SizeMiB | Snapshot size |

**Common filter:**

```sql
SELECT * FROM Reporting.vw_Snapshot_Aging
WHERE AgeDays > 7
ORDER BY AgeDays DESC;
```

> **Full Reference**: [Snapshot Aging Report Details](../reports/Snapshot_Aging.md)

### vw_Tools_Status

VMware Tools compliance.

```sql
SELECT * FROM Reporting.vw_Tools_Status;
```

| Column | Description |
|--------|-------------|
| VM | VM name |
| ToolsStatus | Running, Not Running, Not Installed |
| ToolsVersion | Installed version |
| Upgradeable | Upgrade available |

**Find VMs needing Tools upgrade:**

```sql
SELECT * FROM Reporting.vw_Tools_Status
WHERE Upgradeable = 1;
```

> **Full Reference**: [Tools Status Report Details](../reports/Tools_Status.md)

### Additional Health Reports

See [SSRS Reports User Guide](../SSRS_Reports_User_Guide.md) for full details on these extended reports:

- [Certificate Expiration](../reports/Certificate_Expiration.md) - ESXi SSL certificate expiration tracking
- [Configuration Compliance](../reports/Configuration_Compliance.md) - VM configuration validation against standards
- [Orphaned Files](../reports/Orphaned_Files.md) - VMDK files not linked to registered VMs

## Capacity Views

### vw_Host_Capacity

Host resource utilization.

```sql
SELECT * FROM Reporting.vw_Host_Capacity;
```

| Column | Description |
|--------|-------------|
| Host | Host name |
| CPUUsagePercent | CPU utilization |
| MemoryUsagePercent | Memory utilization |
| VMCount | Number of VMs |
| vCPUPerCore | vCPU:Core ratio |

**Find overutilized hosts:**

```sql
SELECT * FROM Reporting.vw_Host_Capacity
WHERE CPUUsagePercent > 80 OR MemoryUsagePercent > 80;
```

> **Full Reference**: [Host Capacity Report Details](../reports/Host_Capacity.md)

### vw_Datastore_Capacity

Storage capacity analysis.

```sql
SELECT * FROM Reporting.vw_Datastore_Capacity;
```

| Column | Description |
|--------|-------------|
| Datastore | Datastore name |
| CapacityMiB | Total capacity |
| UsedMiB | Used space |
| FreeMiB | Free space |
| FreePercent | Percentage free |
| OverprovisionRatio | Thin provisioning ratio |

**Find low-space datastores:**

```sql
SELECT * FROM Reporting.vw_Datastore_Capacity
WHERE FreePercent < 20
ORDER BY FreePercent;
```

> **Full Reference**: [Datastore Capacity Report Details](../reports/Datastore_Capacity.md)

### vw_VM_Resource_Allocation

VM sizing analysis.

```sql
SELECT * FROM Reporting.vw_VM_Resource_Allocation;
```

| Column | Description |
|--------|-------------|
| VM | VM name |
| CPUs | Configured vCPUs |
| MemoryMiB | Configured memory |
| ProvisionedMiB | Provisioned storage |
| Host | Current host |

> **Full Reference**: [VM Resource Allocation Report Details](../reports/VM_Resource_Allocation.md)

### Additional Capacity Reports

See [SSRS Reports User Guide](../SSRS_Reports_User_Guide.md) for full details on this extended report:

- [VM Right-Sizing](../reports/VM_RightSizing.md) - Identify over-provisioned VMs for cost optimization

## Trend Views

### vw_VM_Count_Trend

VM growth over time.

```sql
SELECT * FROM Reporting.vw_VM_Count_Trend
ORDER BY SnapshotDate;
```

| Column | Description |
|--------|-------------|
| SnapshotDate | Date |
| VMCount | Total VMs |
| PoweredOnCount | Powered on VMs |
| TemplateCount | Templates |

> **Full Reference**: [VM Count Trend Report Details](../reports/VM_Count_Trend.md)

### vw_Datastore_Capacity_Trend

Storage growth over time.

```sql
SELECT * FROM Reporting.vw_Datastore_Capacity_Trend
ORDER BY SnapshotDate;
```

| Column | Description |
|--------|-------------|
| SnapshotDate | Date |
| Datastore | Datastore name |
| CapacityMiB | Capacity at that time |
| UsedMiB | Used at that time |
| FreeMiB | Free at that time |

> **Full Reference**: [Datastore Capacity Trend Report Details](../reports/Datastore_Capacity_Trend.md)

### vw_VM_Config_Changes

Configuration drift detection.

```sql
SELECT * FROM Reporting.vw_VM_Config_Changes
ORDER BY ChangeDate DESC;
```

| Column | Description |
|--------|-------------|
| VM | VM name |
| ChangeDate | When changed |
| ChangedColumn | What changed |
| OldValue | Previous value |
| NewValue | New value |

> **Full Reference**: [VM Config Changes Report Details](../reports/VM_Config_Changes.md)

### Additional Trend Reports

See [SSRS Reports User Guide](../SSRS_Reports_User_Guide.md) for full details on these extended reports:

- [Host Utilization](../reports/Host_Utilization.md) - Historical host CPU and memory utilization
- [VM Lifecycle](../reports/VM_Lifecycle.md) - VM power state changes and uptime analysis
- [Storage Growth](../reports/Storage_Growth.md) - Datastore capacity trends with linear regression support

---

## Next Steps

- [SSRS Reports User Guide](../SSRS_Reports_User_Guide.md) - Complete reference for all 24 reports
- [Querying Data](./querying-data.md) - More query examples
- [Extending Views](../development/extending-views.md) - Create custom views

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
