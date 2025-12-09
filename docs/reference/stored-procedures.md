# Stored Procedures Reference

> Stored procedure documentation and usage.

**Navigation**: [Home](../../README.md) | [RVTools Tabs](./rvtools-tabs.md) | [Troubleshooting](./troubleshooting.md)

---

## Overview

| Procedure | Purpose |
|-----------|---------|
| `usp_ProcessImport` | Main orchestrator - processes all tables with per-table error handling |
| `usp_MergeTable` | Dynamic MERGE using metadata from Config.ColumnMapping |
| `usp_RefreshColumnMapping` | Auto-populates Config tables from database metadata |
| `usp_PurgeOldHistory` | Remove old history records |

---

## usp_ProcessImport

Main stored procedure that orchestrates the entire import process with **per-table error handling**.

### Key Features
- Processes each table independently (failures don't cascade)
- Returns partial success status if some tables fail
- Logs progress to `Audit.MergeProgress`
- Errors captured in `Audit.ErrorLog`

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `@ImportBatchId` | INT | Required | Batch ID from PowerShell |
| `@SourceFile` | NVARCHAR(500) | NULL | Source xlsx file name |

### Usage

```sql
-- Called by PowerShell after loading staging tables
EXEC [dbo].[usp_ProcessImport]
    @ImportBatchId = 1,
    @SourceFile = 'export.xlsx';
```

### Process Flow

1. Log start of processing
2. Get active tables from `Config.TableMapping`
3. For each table (in priority order):
   - Call `usp_MergeTable` in TRY/CATCH
   - Track success/failure counts
   - Continue to next table on error
4. Determine final status (Success/Partial/Failed)
5. Update `Audit.ImportBatch` record
6. Return summary

### Returns

```sql
-- Summary result set
SELECT
    @ImportBatchId AS ImportBatchId,
    @FinalStatus AS Status,           -- 'Success', 'Partial', or 'Failed'
    @SheetsProcessed AS SheetsProcessed,
    @SheetsSucceeded AS SheetsSucceeded,
    @SheetsFailed AS SheetsFailed,
    @SheetsSkipped AS SheetsSkipped,
    @TotalMerged AS TotalRowsMerged,
    DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE()) AS DurationMs;
```

### Status Values

| Status | Meaning |
|--------|---------|
| Success | All tables merged successfully |
| Partial | Some tables succeeded, some failed |
| Failed | All tables failed |

---

## usp_MergeTable

Dynamic MERGE procedure that uses metadata from `Config.ColumnMapping` to build SQL at runtime.

### Key Features
- **Metadata-driven**: Reads column definitions from Config tables
- **Automatic type conversion**: Uses TRY_CAST based on TargetDataType
- **Boolean handling**: Converts True/1/Yes to 1
- **NULL key filtering**: Skips rows where all natural key columns are NULL
- **Full error logging**: Captures errors with dynamic SQL to `Audit.ErrorLog`
- **Progress tracking**: Records status in `Audit.MergeProgress`

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `@ImportBatchId` | INT | Required | Current import batch |
| `@TableName` | NVARCHAR(100) | Required | Table to merge (e.g., 'vInfo') |
| `@SourceFile` | NVARCHAR(500) | NULL | Source file name for history |
| `@MergedCount` | INT | OUTPUT | Number of rows merged |

### Usage

```sql
DECLARE @Count INT;
EXEC [dbo].[usp_MergeTable]
    @ImportBatchId = 1,
    @TableName = 'vInfo',
    @SourceFile = 'export.xlsx',
    @MergedCount = @Count OUTPUT;

SELECT @Count AS RowsMerged;
```

### Process Flow

1. **Log Start**: Insert record into `Audit.MergeProgress` (Status='InProgress')
2. **Validate**: Check table exists in `Config.TableMapping`
3. **Build SQL**: Read `Config.ColumnMapping` to construct:
   - SELECT with TRY_CAST for type conversions
   - ON clause from natural key columns
   - UPDATE SET for non-key columns
   - INSERT column list
4. **Close History**: Update `History` records where entity removed from source
5. **Execute MERGE**: Run dynamic SQL against `Current` table
6. **Insert History**: Add new history records for merged rows
7. **Log Complete**: Update `Audit.MergeProgress` with results

### Type Conversions (Automatic)

| Target Type | Conversion |
|-------------|------------|
| INT, BIGINT | `TRY_CAST(... AS int)` |
| DATETIME2 | `TRY_CAST(... AS datetime2)` |
| DECIMAL | `TRY_CAST(... AS decimal(p,s))` |
| BIT | `CASE WHEN ... IN ('True','1','Yes') THEN 1 ELSE 0 END` |
| NVARCHAR | Direct copy |

### Returns

Output parameter `@MergedCount` contains the number of rows processed.

Progress and errors logged to:
- `Audit.MergeProgress` - Status, timing, row counts
- `Audit.ErrorLog` - Detailed errors with dynamic SQL

---

## usp_RefreshColumnMapping

Auto-populates `Config.TableMapping` and `Config.ColumnMapping` from database metadata.

### When to Use
- After deploying new tables
- After schema changes (add/remove columns)
- To reset mapping to defaults

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `@DebugMode` | BIT | 0 | If 1, prints detailed progress |

### Usage

```sql
-- Refresh with debug output
EXEC [dbo].[usp_RefreshColumnMapping] @DebugMode = 1;

-- Silent refresh
EXEC [dbo].[usp_RefreshColumnMapping];
```

### Process Flow

1. **Truncate** existing mapping tables
2. **Insert TableMapping** with predefined natural keys for all 27 tables
3. **Insert ColumnMapping** from `sys.columns` for Current schema
4. **Mark natural keys** based on TableMapping.NaturalKeyColumns
5. **Report summary** (tables configured, columns mapped)

### Returns

```sql
SELECT
    TablesConfigured,
    ColumnsConfigured,
    NaturalKeyColumns,
    BooleanColumns,
    MergeableColumns;
```

### Natural Key Definitions

The procedure defines natural keys for each table. Key examples:

| Table | Natural Key |
|-------|-------------|
| vInfo | VM_UUID, VI_SDK_Server |
| vHost | Host, VI_SDK_Server |
| vNIC | Host, Network_Device, VI_SDK_Server |
| vHealth | Name, Message_type, VI_SDK_Server |

---

## usp_PurgeOldHistory

Removes history records older than the configured retention period.

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `@RetentionDays` | INT | Config.Settings | Days to retain |
| `@DryRun` | BIT | 0 | If 1, only reports what would be deleted |

### Usage

```sql
-- Check what would be deleted (dry run)
EXEC [dbo].[usp_PurgeOldHistory] @DryRun = 1;

-- Purge with config setting (default 365 days)
EXEC [dbo].[usp_PurgeOldHistory];

-- Purge with custom retention
EXEC [dbo].[usp_PurgeOldHistory] @RetentionDays = 180;
```

### Process Flow

1. Get retention days (parameter or from Config.Settings)
2. Calculate cutoff date
3. For each History table:
   - Count records to delete
   - If not dry run, delete records
   - Track counts
4. Log the purge operation
5. Return results

### Retention Logic

Only deletes records where:
- `ValidTo IS NOT NULL` (closed/superseded records)
- `ValidTo < @CutoffDate` (older than retention period)

Current records (ValidTo IS NULL) are never deleted.

### Returns

```sql
-- Per-table results
SELECT
    TableName,
    RowsToDelete,
    RowsDeleted,
    CutoffDate,
    RetentionDays,
    DryRun
FROM #PurgeResults
WHERE RowsToDelete > 0;
```

---

## Error Handling

All procedures use structured error handling:

```sql
BEGIN TRY
    BEGIN TRANSACTION
    -- Processing
    COMMIT TRANSACTION
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION

    -- Log error
    INSERT INTO Audit.ImportLog (ImportBatchId, LogLevel, Message)
    VALUES (@ImportBatchId, 'Error', ERROR_MESSAGE())

    -- Re-throw
    THROW;
END CATCH
```

---

## Scheduling

### usp_PurgeOldHistory Schedule

Run monthly via SQL Server Agent:

```sql
-- Agent job step
EXEC [dbo].[usp_PurgeOldHistory]
    @RetentionDays = 365;
```

---

## Extending

To add a new table to the merge process:

1. Create Staging, Current, and History tables
2. Add natural key definition to `usp_RefreshColumnMapping` (in the INSERT INTO TableMapping)
3. Run `EXEC dbo.usp_RefreshColumnMapping` to populate column mapping
4. The table will automatically be included in `usp_ProcessImport`

**No new stored procedure needed** - the dynamic `usp_MergeTable` handles all tables.

See [Extending Tables](../development/extending-tables.md) for details.

---

## Next Steps

- [Troubleshooting](./troubleshooting.md) - Common issues
- [Data Flow](../architecture/data-flow.md) - ETL process

## Need Help?

See [Troubleshooting](./troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
