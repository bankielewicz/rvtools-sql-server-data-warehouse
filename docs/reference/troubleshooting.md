# Troubleshooting

> Common issues and solutions.

**Navigation**: [Home](../../README.md) | [RVTools Tabs](./rvtools-tabs.md) | [Stored Procedures](./stored-procedures.md)

---

## Import Issues

### PowerShell Module Not Found

**Error:**
```
Module not found: .../modules/RVToolsImport.psm1
```

**Solution:**
Run the import script from the `src/powershell` directory:
```powershell
cd src/powershell
.\Import-RVToolsData.ps1
```

### ImportExcel Module Missing

**Error:**
```
The term 'Import-Excel' is not recognized
```

**Solution:**
```powershell
Install-Module -Name ImportExcel -Scope CurrentUser -Force
```

### SqlServer Module Missing

**Error:**
```
The term 'Invoke-Sqlcmd' is not recognized
```

**Solution:**
```powershell
Install-Module -Name SqlServer -Scope CurrentUser -Force
```

### Cannot Connect to SQL Server

**Error:**
```
Login failed for user
```

**Solutions:**

1. Check SQL Server is running
2. Verify instance name:
   ```powershell
   .\Import-RVToolsData.ps1 -ServerInstance "localhost\SQLEXPRESS"
   ```
3. For SQL auth, use -UseSqlAuth (will prompt for credentials):
   ```powershell
   .\Import-RVToolsData.ps1 -ServerInstance "server" -UseSqlAuth

   # Or with pre-defined credential
   $cred = Get-Credential
   .\Import-RVToolsData.ps1 -ServerInstance "server" -UseSqlAuth -Credential $cred
   ```
4. Enable SQL Server remote connections

### File Not Found

**Error:**
```
Cannot find path 'incoming\export.xlsx'
```

**Solution:**
- Verify the file exists in the incoming folder
- Check file extension is `.xlsx`
- Use `-SingleFile` for specific path:
  ```powershell
  .\Import-RVToolsData.ps1 -SingleFile "C:\full\path\to\file.xlsx"
  ```

---

## Database Issues

### Database Does Not Exist

**Error:**
```
Cannot open database "RVToolsDW"
```

**Solution:**
Deploy the database first:
```sql
-- Run against master
:r src/tsql/Database/001_CreateDatabase.sql
```

### Missing Tables

**Error:**
```
Invalid object name 'Staging.vInfo'
```

**Solution:**
Run all table creation scripts:
```sql
:r src/tsql/Database/002_CreateSchemas.sql
:r src/tsql/Tables/Staging/001_AllStagingTables.sql
:r src/tsql/Tables/Current/001_AllCurrentTables.sql
:r src/tsql/Tables/History/001_AllHistoryTables.sql
```

### Missing Stored Procedures

**Error:**
```
Could not find stored procedure 'usp_ProcessImport'
```

**Solution:**
```sql
:r src/tsql/StoredProcedures/usp_ProcessImport.sql
:r src/tsql/StoredProcedures/usp_MergeTable_vInfo.sql
:r src/tsql/StoredProcedures/usp_PurgeOldHistory.sql
```

---

## Data Issues

### Type Conversion Errors

**Symptom:** Records in FailedRecords table

**Diagnosis:**
```sql
SELECT SheetName, ErrorMessage, RawData
FROM Audit.FailedRecords
WHERE BatchId = @BatchId;
```

**Common causes:**
- Unexpected data format in RVTools export
- NULL in required field
- String too long for column

**Solution:** Review and fix source data, or expand column size

### Duplicate Key Errors

**Error:**
```
Violation of UNIQUE KEY constraint
```

**Cause:** Same entity from multiple vCenters, or natural key not unique

**Solution:**
1. Verify natural key includes VI_SDK_Server
2. Check if additional columns needed in natural key:
```sql
SELECT TableName, NaturalKeyColumns FROM Config.TableMapping WHERE TableName = 'vHealth';
```

### Missing Sheets

**Symptom:** Some tables not imported

**Diagnosis:**
```sql
SELECT SheetName, RowCount, ErrorCount
FROM Audit.ImportBatchDetail
WHERE BatchId = @BatchId;
```

**Causes:**
- RVTools export doesn't include all tabs
- Sheet name doesn't match expected name

---

## Merge Issues

### Check Which Tables Failed

```sql
SELECT TableName, Status, RowsInStaging, RowsProcessed, ErrorMessage
FROM Audit.MergeProgress
WHERE ImportBatchId = @BatchId AND Status = 'Failed';
```

### View Detailed Errors with SQL

```sql
SELECT TableName, Operation, ErrorMessage, LEFT(DynamicSQL, 1000) AS SQL
FROM Audit.ErrorLog
WHERE ImportBatchId = @BatchId
ORDER BY ErrorLogId;
```

### MERGE Duplicate Row Error

**Error:**
```
The MERGE statement attempted to UPDATE or DELETE the same row more than once
```

**Cause:** Natural key is not unique in source data

**Diagnosis:**
```sql
-- Find duplicates (example for vHealth)
SELECT Name, VI_SDK_Server, COUNT(*) AS DupeCount
FROM Staging.vHealth
WHERE ImportBatchId = @BatchId
GROUP BY Name, VI_SDK_Server
HAVING COUNT(*) > 1;
```

**Solution:** Add additional column to natural key in `usp_RefreshColumnMapping`

### NULL Natural Key Error

**Error:**
```
Violation of UNIQUE KEY constraint... duplicate key value is (<NULL>, <NULL>)
```

**Cause:** Placeholder rows with NULL values in all key columns (common in vFileInfo)

**Solution:** These rows are now automatically filtered by `usp_MergeTable`

### Refresh Column Mapping

After schema changes, refresh the mapping:
```sql
EXEC dbo.usp_RefreshColumnMapping @DebugMode = 1;
```

### Check Natural Key Configuration

```sql
-- View all natural key definitions
SELECT TableName, NaturalKeyColumns FROM Config.TableMapping ORDER BY TableName;

-- Check which columns are marked as natural keys for a table
SELECT CurrentColumnName, IsNaturalKey
FROM Config.ColumnMapping
WHERE TableName = 'vNIC' AND IsNaturalKey = 1;
```

---

## Performance Issues

### Slow Imports

**Symptoms:**
- Import takes hours
- High CPU/memory usage

**Solutions:**

1. **Increase batch size** (if too small):
   ```sql
   UPDATE Config.Settings
   SET SettingValue = '50000'
   WHERE SettingName = 'MaxBatchSize';
   ```

2. **Check tempdb**:
   ```sql
   SELECT size * 8 / 1024 AS SizeMB
   FROM tempdb.sys.database_files;
   ```

3. **Rebuild indexes** after large imports:
   ```sql
   ALTER INDEX ALL ON Current.vInfo REBUILD;
   ```

### Slow Queries

**Solutions:**

1. **Check indexes exist**:
   ```sql
   SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('Current.vInfo');
   ```

2. **Use views** instead of direct table queries

3. **Add date filters** for history queries:
   ```sql
   WHERE ValidFrom >= DATEADD(MONTH, -6, GETDATE())
   ```

---

## History Issues

### History Growing Too Large

**Diagnosis:**
```sql
SELECT
    s.name + '.' + t.name AS TableName,
    SUM(p.rows) AS RowCount
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
JOIN sys.partitions p ON t.object_id = p.object_id
WHERE s.name = 'History' AND p.index_id IN (0, 1)
GROUP BY s.name, t.name
ORDER BY RowCount DESC;
```

**Solution:**
Run purge procedure:
```sql
-- Check what would be deleted
EXEC usp_PurgeOldHistory @DryRun = 1;

-- Purge old records
EXEC usp_PurgeOldHistory @RetentionDays = 180;
```

### Missing History Records

**Cause:** History tables weren't deployed or triggers not working

**Diagnosis:**
```sql
SELECT TOP 1 * FROM History.vInfo ORDER BY ValidFrom DESC;
```

**Solution:** Re-run stored procedures to populate history

---

## Log File Analysis

### Finding Log Files

```
logs/
├── Import_20241208_143022.log
├── Import_20241208_150115.log
└── ...
```

### Common Log Patterns

**Successful import:**
```
[INFO] Starting import: export.xlsx
[INFO] Processing sheet vInfo (5000 rows)
[INFO] Merged 4998 rows, 2 failed
[INFO] Import completed: 27 sheets processed
```

**Failed import:**
```
[ERROR] Failed to connect to SQL Server
[ERROR] Exception: ...
```

### Database Logs

```sql
SELECT
    LogTime,
    LogLevel,
    Message
FROM Audit.ImportLog
WHERE ImportBatchId = @BatchId
ORDER BY LogTime;
```

---

## Getting Help

If your issue isn't listed here:

1. Check the [documentation](../../README.md)
2. Search [existing issues](https://github.com/bankielewicz/RVToolsDW/issues)
3. [Open a new issue](https://github.com/bankielewicz/RVToolsDW/issues/new) with:
   - Error message
   - Steps to reproduce
   - Environment details (SQL Server version, PowerShell version)
   - Relevant log entries
