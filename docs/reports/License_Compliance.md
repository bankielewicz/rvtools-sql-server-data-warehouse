# License Compliance Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Inventory Reports

---

**Category**: Inventory
**View**: `[Reporting].[vw_Inventory_License_Compliance]`
**RDL File**: `src/reports/Inventory/License_Compliance.rdl`
**SQL Source**: `src/tsql/Views/Inventory/vw_License_Compliance.sql`

## Purpose

Tracks VMware license usage versus allocation and identifies licenses that are over-allocated, expiring, or near capacity.

## Data Source

- **Primary Table**: `Current.vLicense`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| LicenseName | NVARCHAR | License product name |
| LicenseKey | NVARCHAR | License key (from vLicense.Key) |
| Labels | NVARCHAR | License labels |
| Cost_Unit | NVARCHAR | Licensing metric (per CPU, per VM, etc.) |
| Total_Licenses | INT | Total licenses purchased |
| Used_Licenses | INT | Licenses currently in use |
| Available_Licenses | INT | Calculated: Total - Used |
| Usage_Percent | DECIMAL(5,2) | Percentage of licenses used |
| Expiration_Date | DATETIME | License expiration date (NULL = perpetual) |
| Days_Until_Expiration | INT | Days until license expires |
| Compliance_Status | NVARCHAR | Calculated status (see below) |
| Features | NVARCHAR | Licensed features |
| VI_SDK_Server | NVARCHAR | vCenter server |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Compliance Status Logic

```sql
CASE
    WHEN Used > Total THEN 'Over-Allocated'
    WHEN Expiration_Date < GETUTCDATE() THEN 'Expired'
    WHEN Expiration_Date < DATEADD(DAY, 30, GETUTCDATE()) THEN 'Expiring Soon'
    WHEN (Used * 100.0 / NULLIF(Total, 0)) > 90 THEN 'Near Capacity'
    ELSE 'Compliant'
END
```

## Sample Queries

**All non-compliant licenses:**
```sql
SELECT LicenseName, Total_Licenses, Used_Licenses, Usage_Percent, Compliance_Status
FROM [Reporting].[vw_Inventory_License_Compliance]
WHERE Compliance_Status != 'Compliant'
ORDER BY Compliance_Status, Days_Until_Expiration;
```

**License utilization summary:**
```sql
SELECT Compliance_Status, COUNT(*) AS License_Count
FROM [Reporting].[vw_Inventory_License_Compliance]
GROUP BY Compliance_Status;
```

## Related Reports

- [Host Inventory](./Host_Inventory.md) - ESXi hosts consuming these licenses
- [VM Inventory](./VM_Inventory.md) - VMs using licensed features

## Notes

- Licenses with NULL Total_Licenses may show NULL for Usage_Percent and Available_Licenses.
- The 'Near Capacity' threshold is hardcoded at 90% usage.
