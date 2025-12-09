# Configuration

> Configuration options and settings reference.

**Navigation**: [Home](../README.md) | [Getting Started](./getting-started.md) | [Installation](./installation.md)

---

## PowerShell Parameters

The `Import-RVToolsData.ps1` script accepts these parameters:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-ServerInstance` | string | localhost | SQL Server instance name |
| `-Database` | string | RVToolsDW | Target database name |
| `-Credential` | PSCredential | (none) | SQL auth credential (uses Windows auth if omitted) |
| `-IncomingFolder` | string | ../incoming | Folder containing xlsx files |
| `-LogLevel` | string | Info | Verbose, Info, Warning, Error |
| `-SingleFile` | string | (none) | Process only this specific file |

### Examples

```powershell
# Basic import with Windows auth
.\Import-RVToolsData.ps1 -ServerInstance "localhost"

# SQL authentication
$cred = Get-Credential
.\Import-RVToolsData.ps1 -ServerInstance "sql.domain.com" -Credential $cred

# Verbose logging
.\Import-RVToolsData.ps1 -LogLevel Verbose

# Single file import
.\Import-RVToolsData.ps1 -SingleFile "C:\exports\vcenter-export.xlsx"

# Custom incoming folder
.\Import-RVToolsData.ps1 -IncomingFolder "D:\RVToolsExports"
```

## Log Levels

| Level | Description |
|-------|-------------|
| Verbose | All details including row counts per sheet |
| Info | Import progress and summary |
| Warning | Non-fatal issues (skipped sheets, type conversions) |
| Error | Fatal errors only |

## Config.Settings Table

Application settings stored in the database:

```sql
SELECT SettingName, SettingValue, Description
FROM Config.Settings;
```

| Setting | Default | Description |
|---------|---------|-------------|
| HistoryRetentionDays | 365 | Days to retain history data |
| EnableAuditLogging | true | Log all import operations |
| MaxBatchSize | 10000 | Maximum rows per batch insert |

### Modifying Settings

```sql
UPDATE Config.Settings
SET SettingValue = '730'
WHERE SettingName = 'HistoryRetentionDays';
```

## History Retention

The `usp_PurgeOldHistory` stored procedure removes history records older than the configured retention period:

```sql
-- Manual purge (uses Config.Settings value)
EXEC usp_PurgeOldHistory;

-- Override retention days
EXEC usp_PurgeOldHistory @RetentionDays = 180;
```

### Scheduling Purge

Schedule a SQL Server Agent job to run periodically:

```sql
-- Example: Run monthly
EXEC usp_PurgeOldHistory;
```

## Table Retention Settings

Per-table retention can be configured in `Config.TableRetention`:

```sql
SELECT TableName, RetentionDays
FROM Config.TableRetention;
```

This allows different retention periods for different tables (e.g., keep vInfo history longer than vHealth).

---

## Next Steps

- [Importing Data](./usage/importing-data.md) - Running imports
- [Architecture Overview](./architecture/overview.md) - System design

## Need Help?

See [Troubleshooting](./reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
