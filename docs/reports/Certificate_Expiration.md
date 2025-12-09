# Certificate Expiration Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Health Reports

---

**Category**: Health
**View**: `[Reporting].[vw_Health_Certificate_Expiration]`
**RDL File**: `src/reports/Health/Certificate_Expiration.rdl`
**SQL Source**: `src/tsql/Views/Health/vw_Certificate_Expiration.sql`

## Purpose

Tracks ESXi host SSL certificate expiration dates to identify certificates that are expired or expiring soon.

## Data Source

- **Primary Table**: `Current.vHost`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| HostName | NVARCHAR | ESXi host FQDN |
| VI_SDK_Server | NVARCHAR | vCenter server managing this host |
| Datacenter | NVARCHAR | Datacenter location |
| Cluster | NVARCHAR | Cluster membership |
| Certificate_Issuer | NVARCHAR | Certificate authority |
| Certificate_Subject | NVARCHAR | Certificate subject DN |
| Certificate_Status | NVARCHAR | Certificate status from vCenter |
| Certificate_Start_Date | DATETIME | Certificate valid-from date |
| Certificate_Expiry_Date | DATETIME | Certificate expiration date |
| Days_Until_Expiration | INT | Days remaining until expiration (negative if expired) |
| Expiration_Status | NVARCHAR | Calculated status: 'Expired', 'Expiring Soon' (<30 days), 'Expiring (90 days)' (<90 days), 'Valid', or 'Unknown' |
| ESX_Version | NVARCHAR | VMware ESXi version |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## Expiration Status Logic

```sql
CASE
    WHEN Certificate_Expiry_Date IS NULL THEN 'Unknown'
    WHEN Certificate_Expiry_Date < GETUTCDATE() THEN 'Expired'
    WHEN Certificate_Expiry_Date < DATEADD(DAY, 30, GETUTCDATE()) THEN 'Expiring Soon'
    WHEN Certificate_Expiry_Date < DATEADD(DAY, 90, GETUTCDATE()) THEN 'Expiring (90 days)'
    ELSE 'Valid'
END
```

## Sample Queries

**Certificates expiring within 30 days:**
```sql
SELECT HostName, Cluster, Certificate_Expiry_Date, Days_Until_Expiration, Expiration_Status
FROM [Reporting].[vw_Health_Certificate_Expiration]
WHERE Expiration_Status IN ('Expired', 'Expiring Soon')
ORDER BY Days_Until_Expiration;
```

**Certificate summary by cluster:**
```sql
SELECT Cluster,
       COUNT(*) AS Total_Hosts,
       SUM(CASE WHEN Expiration_Status = 'Expired' THEN 1 ELSE 0 END) AS Expired,
       SUM(CASE WHEN Expiration_Status = 'Expiring Soon' THEN 1 ELSE 0 END) AS Expiring_Soon,
       SUM(CASE WHEN Expiration_Status = 'Valid' THEN 1 ELSE 0 END) AS Valid
FROM [Reporting].[vw_Health_Certificate_Expiration]
GROUP BY Cluster;
```

## Related Reports

- [Host Inventory](./Host_Inventory.md) - ESXi host details including certificate info
- [Health Issues](./Health_Issues.md) - Certificate-related health alerts

## Notes

- If all certificates show 'Unknown' status, the Certificate_Expiry_Date column may be NULL in the source data. Verify RVTools is capturing certificate information (requires appropriate vCenter permissions).
- The view queries `Current.vHost`, so data reflects the most recent RVTools import.
