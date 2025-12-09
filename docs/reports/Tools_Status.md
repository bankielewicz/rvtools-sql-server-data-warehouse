# Tools Status Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Health Reports

---

**Category**: Health
**View**: `[Reporting].[vw_Tools_Status]`
**RDL File**: `src/reports/Health/Tools_Status.rdl`
**SQL Source**: `src/tsql/Views/Health/vw_Tools_Status.sql`

## Purpose

Tracks VMware Tools installation status and version compliance across all VMs, identifying machines that need tools upgrades or have tools-related issues.

## Data Source

- **Primary Table**: `Current.vTools`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| VM | NVARCHAR | Virtual machine name |
| VM_UUID | NVARCHAR | VM unique identifier |
| Powerstate | NVARCHAR | VM power state |
| Template | NVARCHAR | Whether VM is a template |
| ToolsStatus | NVARCHAR | Current VMware Tools status (toolsOk, toolsOld, toolsNotInstalled, toolsNotRunning) |
| Tools_Version | NVARCHAR | Installed VMware Tools version number |
| Required_Version | NVARCHAR | Required VMware Tools version for this ESXi host |
| Upgradeable | NVARCHAR | Whether tools can be upgraded |
| Upgrade_Policy | NVARCHAR | Automatic upgrade policy setting |
| App_status | NVARCHAR | Application heartbeat status |
| Heartbeat_status | NVARCHAR | Guest OS heartbeat status |
| Operation_Ready | NVARCHAR | Whether guest operations are ready |
| State_change_support | NVARCHAR | Supported state change operations |
| Interactive_Guest | NVARCHAR | Whether interactive guest access is available |
| Datacenter | NVARCHAR | Datacenter location |
| Cluster | NVARCHAR | Cluster membership |
| Host | NVARCHAR | ESXi host running the VM |
| Folder | NVARCHAR | VM folder location |
| OS_according_to_the_VMware_Tools | NVARCHAR | Guest OS reported by VMware Tools |
| OS_according_to_the_configuration_file | NVARCHAR | Guest OS from VM configuration |
| VI_SDK_Server | NVARCHAR | vCenter server |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Sample Queries

**VMs with outdated or missing VMware Tools:**
```sql
SELECT VM, Powerstate, ToolsStatus, Tools_Version, Required_Version, Cluster
FROM [Reporting].[vw_Tools_Status]
WHERE ToolsStatus IN ('toolsOld', 'toolsNotInstalled', 'toolsNotRunning')
  AND Powerstate = 'poweredOn'
ORDER BY ToolsStatus, VM;
```

**VMware Tools status summary:**
```sql
SELECT ToolsStatus, COUNT(*) AS VM_Count
FROM [Reporting].[vw_Tools_Status]
WHERE Powerstate = 'poweredOn' AND Template = 'False'
GROUP BY ToolsStatus
ORDER BY VM_Count DESC;
```

**VMs eligible for tools upgrade:**
```sql
SELECT VM, Cluster, Tools_Version, Required_Version, Upgrade_Policy
FROM [Reporting].[vw_Tools_Status]
WHERE Upgradeable = 'true'
  AND Powerstate = 'poweredOn'
ORDER BY Cluster, VM;
```

**VMs with heartbeat issues:**
```sql
SELECT VM, Cluster, Heartbeat_status, ToolsStatus, Powerstate
FROM [Reporting].[vw_Tools_Status]
WHERE Heartbeat_status NOT IN ('green', 'Gray')
  AND Powerstate = 'poweredOn'
ORDER BY Heartbeat_status, VM;
```

## Related Reports

- [VM Inventory](./VM_Inventory.md) - VMs with tools issues
- [Configuration Compliance](./Configuration_Compliance.md) - Tools as a compliance check

## Notes

- VMware Tools provides essential guest integration features including graceful shutdown, time sync, and performance metrics.
- `toolsOk` indicates Tools are installed and current; `toolsOld` means an upgrade is available.
- `toolsNotRunning` on powered-on VMs may indicate guest OS issues or service problems.
- Heartbeat_status of 'green' indicates healthy guest communication; 'gray' means the VM is powered off or heartbeat is not expected.
- Templates typically show `toolsNotRunning` since they are not powered on.
- The view queries `Current.vTools`, so data reflects the most recent RVTools import.
