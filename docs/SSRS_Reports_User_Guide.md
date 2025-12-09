# SSRS Reports User Guide

**Version**: 2.0
**Last Updated**: 2025-12-09

**Navigation**: [Home](../README.md) | [Reports Overview](usage/reports.md) | [Querying Data](usage/querying-data.md)

---

## Overview

The RVTools Data Warehouse provides 24 SSRS reports organized into four categories. These reports deliver insights into VMware infrastructure inventory, health, capacity, and trends.

| Category | Reports | Purpose |
|----------|---------|---------|
| [Inventory](#inventory-reports) | 8 | What exists in your environment |
| [Health](#health-reports) | 6 | Issues requiring attention |
| [Capacity](#capacity-reports) | 4 | Resource utilization and planning |
| [Trends](#trends-reports) | 6 | Historical analysis and forecasting |

### Core vs Extended Reports

The project includes **13 core reports** covering fundamental VMware monitoring needs, plus **11 extended reports** for specialized use cases like multi-vCenter aggregation, compliance validation, and advanced trending.

---

## Inventory Reports

Reports that document what exists in your VMware environment.

| Report | Purpose | View | Details |
|--------|---------|------|---------|
| VM Inventory | Complete VM listing with specifications | `vw_VM_Inventory` | [Full Reference](reports/VM_Inventory.md) |
| Host Inventory | ESXi host details and configuration | `vw_Host_Inventory` | [Full Reference](reports/Host_Inventory.md) |
| Cluster Summary | Cluster-level aggregation | `vw_Cluster_Summary` | [Full Reference](reports/Cluster_Summary.md) |
| Datastore Inventory | Storage summary | `vw_Datastore_Inventory` | [Full Reference](reports/Datastore_Inventory.md) |
| Enterprise Summary | Multi-vCenter VM/resource totals | `vw_Enterprise_Summary` | [Full Reference](reports/Enterprise_Summary.md) |
| License Compliance | License usage vs allocation | `vw_License_Compliance` | [Full Reference](reports/License_Compliance.md) |
| Network Topology | Port groups, VLANs, switches | `vw_Network_Topology` | [Full Reference](reports/Network_Topology.md) |
| Resource Pool Utilization | Resource pool usage metrics | `vw_ResourcePool_Utilization` | [Full Reference](reports/ResourcePool_Utilization.md) |

---

## Health Reports

Reports that identify infrastructure issues requiring attention.

| Report | Purpose | View | Details |
|--------|---------|------|---------|
| Health Issues | Active problems from vHealth | `vw_Health_Issues` | [Full Reference](reports/Health_Issues.md) |
| Snapshot Aging | Old snapshots consuming storage | `vw_Snapshot_Aging` | [Full Reference](reports/Snapshot_Aging.md) |
| Tools Status | VMware Tools compliance | `vw_Tools_Status` | [Full Reference](reports/Tools_Status.md) |
| Certificate Expiration | ESXi SSL certificate tracking | `vw_Certificate_Expiration` | [Full Reference](reports/Certificate_Expiration.md) |
| Configuration Compliance | VM configuration validation | `vw_Configuration_Compliance` | [Full Reference](reports/Configuration_Compliance.md) |
| Orphaned Files | VMDKs not linked to VMs | `vw_Orphaned_Files` | [Full Reference](reports/Orphaned_Files.md) |

---

## Capacity Reports

Reports for capacity planning and resource optimization.

| Report | Purpose | View | Details |
|--------|---------|------|---------|
| Host Capacity | Host resource utilization | `vw_Host_Capacity` | [Full Reference](reports/Host_Capacity.md) |
| Datastore Capacity | Storage capacity analysis | `vw_Datastore_Capacity` | [Full Reference](reports/Datastore_Capacity.md) |
| VM Resource Allocation | VM sizing overview | `vw_VM_Resource_Allocation` | [Full Reference](reports/VM_Resource_Allocation.md) |
| VM Right-Sizing | Over-provisioned VM identification | `vw_VM_RightSizing` | [Full Reference](reports/VM_RightSizing.md) |

---

## Trends Reports

Reports that show changes over time for forecasting and analysis.

| Report | Purpose | View | Details |
|--------|---------|------|---------|
| VM Count Trend | VM growth over time | `vw_VM_Count_Trend` | [Full Reference](reports/VM_Count_Trend.md) |
| Datastore Capacity Trend | Storage growth over time | `vw_Datastore_Capacity_Trend` | [Full Reference](reports/Datastore_Capacity_Trend.md) |
| VM Config Changes | Configuration drift detection | `vw_VM_Config_Changes` | [Full Reference](reports/VM_Config_Changes.md) |
| Host Utilization | Host CPU/memory trends | `vw_Host_Utilization` | [Full Reference](reports/Host_Utilization.md) |
| VM Lifecycle | VM power state history | `vw_VM_Lifecycle` | [Full Reference](reports/VM_Lifecycle.md) |
| Storage Growth | Datastore capacity trends with regression | `vw_Storage_Growth` | [Full Reference](reports/Storage_Growth.md) |

---

## Data Requirements

### Current vs Historical Data

- **Current views** (Inventory, Health, Capacity): Query `Current.*` tables, showing the latest RVTools import
- **Trends views**: Query `History.*` tables, requiring multiple imports over time

### Prerequisites

| Report | Requirement |
|--------|-------------|
| Orphaned Files | RVTools must be run with `-GetFileInfo` flag |
| All Trends reports | Multiple RVTools imports over time |
| Configuration Compliance | vCPU, vMemory, vTools, and vHost data must be populated |

---

## Multi-vCenter Filtering

All views include a `VI_SDK_Server` column for filtering by vCenter:

```sql
-- All vCenters
SELECT * FROM [Reporting].[vw_ViewName];

-- Specific vCenter
SELECT * FROM [Reporting].[vw_ViewName]
WHERE VI_SDK_Server = 'vcenter.example.com';
```

---

## Quick Reference Queries

### Health Dashboard

```sql
-- Expiring certificates (next 30 days)
SELECT HostName, Days_Until_Expiration, Expiration_Status
FROM [Reporting].[vw_Certificate_Expiration]
WHERE Expiration_Status IN ('Expired', 'Expiring Soon');

-- Old snapshots
SELECT VM, SnapshotName, AgeDays, SizeMiB
FROM [Reporting].[vw_Snapshot_Aging]
WHERE AgeDays > 7
ORDER BY AgeDays DESC;

-- Tools needing upgrade
SELECT VM, ToolsStatus, ToolsVersion
FROM [Reporting].[vw_Tools_Status]
WHERE Upgradeable = 1;
```

### Capacity Summary

```sql
-- Low-space datastores
SELECT Datastore, FreePercent, FreeMiB / 1024 AS FreeGB
FROM [Reporting].[vw_Datastore_Capacity]
WHERE FreePercent < 20;

-- Over-utilized hosts
SELECT Host, CPUUsagePercent, MemoryUsagePercent
FROM [Reporting].[vw_Host_Capacity]
WHERE CPUUsagePercent > 80 OR MemoryUsagePercent > 80;

-- Right-sizing candidates
SELECT VM, CPUs, MemoryMiB / 1024 AS MemoryGB
FROM [Reporting].[vw_VM_Resource_Allocation]
WHERE CPUs > 4 OR MemoryMiB > 16384;
```

---

## File Locations

| Type | Location |
|------|----------|
| SQL Views | `src/tsql/Views/{Category}/` |
| RDL Reports | `src/reports/{Category}/` |
| Report Documentation | `docs/reports/` |

---

## Troubleshooting

### No Data Returned

1. Verify RVTools has been imported: `SELECT TOP 1 * FROM Audit.ImportBatch ORDER BY ImportBatchId DESC`
2. Check source table has data: `SELECT COUNT(*) FROM Current.vInfo`
3. For trends reports, verify history exists: `SELECT COUNT(*) FROM History.vInfo`

### NULL Values in Calculated Columns

- Memory/CPU metrics may be NULL if vMemory/vCPU tables are not populated
- Certificate dates may be NULL if vCenter permissions do not allow access
- Orphaned files requires `-GetFileInfo` flag when running RVTools

### Reserved Word Errors

The views handle SQL reserved words with brackets:
- `[Switch]` in Network Topology
- `[Key]` in License Compliance
- `[Limit]` in Resource Pool Utilization

---

## Next Steps

- [Reports Overview](usage/reports.md) - Lightweight view descriptions with examples
- [Querying Data](usage/querying-data.md) - SQL query examples
- [Extending Views](development/extending-views.md) - Create custom views
- [Extending Reports](development/extending-reports.md) - Create custom SSRS reports

## Need Help?

See [Troubleshooting](reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
