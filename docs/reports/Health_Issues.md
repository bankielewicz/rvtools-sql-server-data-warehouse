# Health Issues Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Health Reports

---

**Category**: Health
**View**: `[Reporting].[vw_Health_Issues]`
**RDL File**: `src/reports/Health/Health_Issues.rdl`
**SQL Source**: `src/tsql/Views/Health/vw_Health_Issues.sql`

## Purpose

Displays active health problems reported by vCenter that require attention, providing a consolidated view of issues across the VMware environment.

## Data Source

- **Primary Table**: `Current.vHealth`
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| ObjectName | NVARCHAR | Name of the affected object (VM, host, datastore, etc.) |
| Message | NVARCHAR | Detailed health message describing the issue |
| IssueType | NVARCHAR | Category of the health issue |
| VI_SDK_Server | NVARCHAR | vCenter server reporting this issue |
| ImportBatchId | INT | Import batch reference |
| DetectedDate | DATETIME | When the issue was detected (last import timestamp) |

## Sample Queries

**All current health issues:**
```sql
SELECT ObjectName, IssueType, Message, VI_SDK_Server
FROM [Reporting].[vw_Health_Issues]
ORDER BY IssueType, ObjectName;
```

**Health issue counts by type:**
```sql
SELECT IssueType, COUNT(*) AS Issue_Count
FROM [Reporting].[vw_Health_Issues]
GROUP BY IssueType
ORDER BY Issue_Count DESC;
```

**Health issues by vCenter:**
```sql
SELECT VI_SDK_Server,
       COUNT(*) AS Total_Issues,
       COUNT(DISTINCT IssueType) AS Issue_Types
FROM [Reporting].[vw_Health_Issues]
GROUP BY VI_SDK_Server
ORDER BY Total_Issues DESC;
```

## Related Reports

- [Configuration Compliance](./Configuration_Compliance.md) - VM configuration validation
- [Tools Status](./Tools_Status.md) - VMware Tools compliance issues

## Notes

- The vHealth tab in RVTools captures health messages from vCenter's alarm and health monitoring systems.
- Issue types vary based on vCenter version and configured alarms.
- An empty result set indicates no active health issues were reported at the time of the RVTools export.
- Historical health issues are not retained once cleared; only current issues at export time are captured.
- The view queries `Current.vHealth`, so data reflects the most recent RVTools import.
