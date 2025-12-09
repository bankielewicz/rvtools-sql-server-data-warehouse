# Reports and Views

> Available reporting views and their usage.

**Navigation**: [Home](../../README.md) | [Importing Data](./importing-data.md) | [Querying Data](./querying-data.md)

---

## Overview

The RVTools Data Warehouse includes 13 pre-built views organized into four categories:

| Category | Views | Purpose |
|----------|-------|---------|
| Inventory | 4 | Infrastructure listings |
| Health | 3 | Issues and compliance |
| Capacity | 3 | Resource utilization |
| Trends | 3 | Historical analysis |

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

---

## Next Steps

- [Querying Data](./querying-data.md) - More query examples
- [Extending Views](../development/extending-views.md) - Create custom views

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
