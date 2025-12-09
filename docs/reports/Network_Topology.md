# Network Topology Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Inventory Reports

---

**Category**: Inventory
**View**: `[Reporting].[vw_Inventory_Network_Topology]`
**RDL File**: `src/reports/Inventory/Network_Topology.rdl`
**SQL Source**: `src/tsql/Views/Inventory/vw_Network_Topology.sql`

## Purpose

Maps port groups to VMs and identifies orphaned port groups (port groups with no VMs attached). Also shows switch type (standard vs distributed) and security settings.

## Data Source

- **Primary Table**: `Current.vPort`
- **Lookup Tables**: `Current.dvSwitch`, `Current.vSwitch`, `Current.vInfo`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| Port_Group | NVARCHAR | Port group name |
| VLAN | INT | VLAN ID |
| Switch_Name | NVARCHAR | Switch name (from vPort.Switch) |
| HostName | NVARCHAR | ESXi host |
| Datacenter | NVARCHAR | Datacenter |
| Cluster | NVARCHAR | Cluster |
| VI_SDK_Server | NVARCHAR | vCenter server |
| Switch_Type | NVARCHAR | 'Distributed', 'Standard', or 'Unknown' |
| VM_Count | INT | Number of VMs using this port group |
| Is_Orphaned | BIT | 1 = no VMs attached, 0 = in use |
| Promiscuous_Mode | NVARCHAR | Promiscuous mode setting |
| Mac_Changes | NVARCHAR | MAC address changes setting |
| Forged_Transmits | NVARCHAR | Forged transmits setting |
| Traffic_Shaping | NVARCHAR | Traffic shaping setting |
| Load_Balancing_Policy | NVARCHAR | Load balancing policy |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Switch Type Detection

```sql
CASE
    WHEN EXISTS (SELECT 1 FROM [Current].[dvSwitch] dv
                 WHERE dv.Name = p.[Switch] AND dv.VI_SDK_Server = p.VI_SDK_Server)
    THEN 'Distributed'
    WHEN EXISTS (SELECT 1 FROM [Current].[vSwitch] vs
                 WHERE vs.[Switch] = p.[Switch] AND vs.Host = p.Host
                 AND vs.VI_SDK_Server = p.VI_SDK_Server)
    THEN 'Standard'
    ELSE 'Unknown'
END
```

## VM Count Logic

The VM_Count is calculated by checking vInfo.Network_1 through Network_8 columns against the port group name.

## Orphan Detection

A port group is marked orphaned when no VMs in vInfo have this port group assigned to any of their 8 network slots.

## Sample Queries

**Orphaned port groups:**
```sql
SELECT Port_Group, VLAN, Switch_Name, Switch_Type, Datacenter
FROM [Reporting].[vw_Inventory_Network_Topology]
WHERE Is_Orphaned = 1
ORDER BY VLAN, Port_Group;
```

**Port groups by VLAN:**
```sql
SELECT VLAN, Port_Group, Switch_Type, VM_Count
FROM [Reporting].[vw_Inventory_Network_Topology]
ORDER BY VLAN, Port_Group;
```

**Security settings summary:**
```sql
SELECT Port_Group, Switch_Type, Promiscuous_Mode, Mac_Changes, Forged_Transmits
FROM [Reporting].[vw_Inventory_Network_Topology]
WHERE Promiscuous_Mode = '1' OR Mac_Changes = '1' OR Forged_Transmits = '1';
```

## Related Reports

- [VM Inventory](./VM_Inventory.md) - VMs connected to these networks
- [Host Inventory](./Host_Inventory.md) - Hosts hosting these port groups

## Notes

- The `[Switch]` column is a SQL reserved word and is properly bracketed in the view.
- VM_Count uses a correlated subquery checking 8 network columns (Network_1 through Network_8).
- Port groups appear once per host they're configured on, so the same port group name may appear multiple times.
