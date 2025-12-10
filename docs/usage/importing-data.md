# Importing Data

> Running imports and handling RVTools exports.

**Navigation**: [Home](../../README.md) | [Reports](./reports.md) | [Querying Data](./querying-data.md)

---

## Basic Import

```powershell
cd src/powershell

# Windows Authentication (default)
.\Import-RVToolsData.ps1 -ServerInstance "localhost" -LogLevel Verbose

# SQL Authentication (will prompt for credentials)
.\Import-RVToolsData.ps1 -ServerInstance "localhost" -UseSqlAuth -LogLevel Verbose
```

This processes all xlsx files in the `incoming/` folder.

## Parameters Reference

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-ServerInstance` | string | localhost | SQL Server instance |
| `-Database` | string | RVToolsDW | Database name |
| `-UseSqlAuth` | switch | false | Use SQL Server authentication (prompts if credential not provided) |
| `-Credential` | PSCredential | (none) | SQL auth credential |
| `-IncomingFolder` | string | ../incoming | Source folder |
| `-LogLevel` | string | Info | Logging verbosity |
| `-SingleFile` | string | (none) | Process single file |

## Common Scenarios

### Windows Authentication

```powershell
.\Import-RVToolsData.ps1 -ServerInstance "sqlserver.domain.com"
```

### SQL Server Authentication

```powershell
# Will prompt for credentials
.\Import-RVToolsData.ps1 -ServerInstance "sqlserver" -UseSqlAuth

# With pre-defined credential
$cred = Get-Credential
.\Import-RVToolsData.ps1 -ServerInstance "sqlserver" -UseSqlAuth -Credential $cred
```

### Single File Import

```powershell
.\Import-RVToolsData.ps1 -SingleFile "C:\exports\vcenter-prod.xlsx"
```

### Custom Incoming Folder

```powershell
.\Import-RVToolsData.ps1 -IncomingFolder "D:\RVToolsExports"
```

### Verbose Logging

```powershell
.\Import-RVToolsData.ps1 -LogLevel Verbose
```

---

## Historical Import

Use `Import-RVToolsHistoricalData.ps1` when you have historical RVTools exports with dates embedded in the filenames.

### When to Use

- Bulk importing historical data (e.g., hundreds of files spanning months/years)
- Files have dates in filename pattern: `{vcenter-name}_{d_mm_yyyy}.{domain.tld}.xlsx`
- You need accurate `ValidFrom` dates in History tables (not import timestamp)

### Filename Pattern

```
{vcenter-name}_{d_mm_yyyy}.{domain.tld}.xlsx
```

**Pattern Components:**
- `{vcenter-name}`: Alphanumeric + hyphens (e.g., `vCenter01`, `prod-vcenter`, `vc-east-01`)
- `{d_mm_yyyy}`: Day, month, year (flexible: `5_06_2024` or `05_06_2024`)
- `{domain.tld}`: Must contain at least one dot (e.g., `domain.com`, `corp.domain.com`)

| Example | Parsed Date | VIServer |
|---------|-------------|----------|
| `vCenter01_5_06_2024.domain.com.xlsx` | June 5, 2024 | vCenter01 |
| `prod-vcenter_15_12_2023.corp.domain.com.xlsx` | December 15, 2023 | prod-vcenter |
| `vc-east-01_1_1_2025.local.net.xlsx` | January 1, 2025 | vc-east-01 |

Flexible parsing handles both single-digit (5_06_2024) and double-digit (05_06_2024) formats.

### Historical Import Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-IncomingFolder` | string | ../incoming | Source folder |
| `-ServerInstance` | string | localhost | SQL Server instance |
| `-Database` | string | RVToolsDW | Database name |
| `-UseSqlAuth` | switch | false | Use SQL authentication |
| `-Credential` | PSCredential | (none) | SQL credential |
| `-LogLevel` | string | Info | Logging verbosity |
| `-WhatIf` | switch | false | Preview mode (shows files without importing) |
| `-Force` | switch | false | Skip confirmation prompt |

### Historical Import Examples

```powershell
cd src/powershell

# Preview files (see what would be imported and in what order)
.\Import-RVToolsHistoricalData.ps1 -WhatIf

# Run import with verbose logging
.\Import-RVToolsHistoricalData.ps1 -ServerInstance "localhost" -LogLevel Verbose

# Skip confirmation prompt
.\Import-RVToolsHistoricalData.ps1 -Force

# SQL Authentication
.\Import-RVToolsHistoricalData.ps1 -UseSqlAuth
```

### How Historical Import Works

1. **Scans** incoming folder for xlsx files
2. **Parses** filename to extract date and VIServer
3. **Sorts** files by date (oldest first)
4. **Imports** each file with:
   - `Audit.ImportBatch.RVToolsExportDate` = parsed date
   - `History.ValidFrom` = parsed date (not import timestamp)

### Why Chronological Order?

Files are processed oldest-first to maintain correct History table timeline. This ensures:
- `ValidFrom`/`ValidTo` ranges are accurate
- Point-in-time queries work correctly: `WHERE ValidFrom <= '2024-06-15'`

### Skipped Files

Files that don't match the expected pattern are skipped and listed at the start of the import. Move them to a different folder or rename them to match the pattern.

---

## Generating RVTools Exports

In RVTools:

1. Connect to vCenter
2. **File** → **Export all to xlsx**
3. Save to the `incoming/` folder

> **Tip**: Use consistent file naming like `vcenter-name_YYYYMMDD.xlsx`

## File Processing

### Folder Structure

| Folder | Purpose |
|--------|---------|
| `incoming/` | Drop xlsx files here |
| `processed/` | Successfully imported files |
| `errors/` | Files that failed completely |
| `failed/` | Individual failed records (CSV) |
| `logs/` | Execution logs |

### File Lifecycle

```
1. Place file in incoming/
2. Run import script
3. File moves to processed/ (success) or errors/ (failure)
4. Filename includes timestamp: export_20241208_143022.xlsx
```

## Monitoring Imports

### Check Import Status

```sql
-- Recent imports
SELECT TOP 10
    BatchId,
    FileName,
    StartTime,
    EndTime,
    Status,
    TotalRows,
    SuccessRows,
    FailedRows
FROM Audit.ImportBatch
ORDER BY StartTime DESC;
```

### Per-Sheet Statistics

```sql
SELECT
    ibd.SheetName,
    ibd.RowCount,
    ibd.ImportedCount,
    ibd.ErrorCount
FROM Audit.ImportBatchDetail ibd
JOIN Audit.ImportBatch ib ON ib.BatchId = ibd.BatchId
WHERE ib.BatchId = @BatchId
ORDER BY ibd.SheetName;
```

### Failed Records

```sql
SELECT
    SheetName,
    RowNumber,
    ErrorMessage,
    RawData
FROM Audit.FailedRecords
WHERE BatchId = @BatchId;
```

### Import Logs

```sql
SELECT
    LogTime,
    LogLevel,
    Message
FROM Audit.ImportLog
WHERE BatchId = @BatchId
ORDER BY LogTime;
```

## Scheduling Imports

### Windows Task Scheduler

1. Create a scheduled task
2. Action: Start a program
3. Program: `powershell.exe`
4. Arguments:
   ```
   -ExecutionPolicy Bypass -File "C:\RVToolsDW\src\powershell\Import-RVToolsData.ps1" -ServerInstance "localhost"
   ```

### SQL Server Agent

Create a PowerShell job step:

```powershell
Set-Location "C:\RVToolsDW\src\powershell"
.\Import-RVToolsData.ps1 -ServerInstance "localhost"
```

## Handling Failures

### Partial Failures

If some sheets fail but others succeed:
- Batch status = "Partial"
- Successful sheets are imported
- Failed records logged to `Audit.FailedRecords`

### Complete Failures

If the entire import fails:
- File moved to `errors/`
- Exception logged to `Audit.ImportLog`
- Check log file in `logs/` folder

### Retry Failed Files

```powershell
# Move file back to incoming
Move-Item "errors\failed_export.xlsx" "incoming\"

# Re-run import
.\Import-RVToolsData.ps1 -LogLevel Verbose
```

## Performance Tips

### Large Exports

For very large exports (100K+ VMs):

1. Increase SQL Server memory
2. Consider importing during off-hours
3. Monitor tempdb usage

### Frequent Imports

For daily imports:

1. Schedule during low-activity periods
2. Monitor history table growth
3. Configure appropriate retention

---

## Next Steps

- [Reports](./reports.md) - View available reports
- [Querying Data](./querying-data.md) - Query examples

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
