# Soft-Delete/Tombstone Feature

**Navigation**: [Home](../../README.md) | [Installation](../installation.md) | [Extending Tables](extending-tables.md)

---

## Overview

The soft-delete feature tracks when records are removed from vCenter (VMs deleted, hosts decommissioned, datastores unmounted, etc.) instead of immediately removing them from Current tables. This provides:

- **Audit trail** - Know when and why records disappeared
- **Recovery window** - Identify accidental deletions before archival
- **Reporting accuracy** - Show deleted items in reports with status indicators
- **Compliance** - Meet data retention requirements for decommissioned assets

## How It Works

When soft-delete is enabled:

1. **Import detects missing records** - Records in Current but not in new import are flagged
2. **Soft-delete applied** - `IsDeleted = 1`, `DeletedDate` and `DeletedBatchId` set
3. **Last seen tracking** - `LastSeenDate` and `LastSeenBatchId` track last appearance
4. **Archive process** - Scheduled job moves old soft-deleted records to History

```
Normal Import Flow:
  Source Data → Staging → MERGE → Current ← soft-delete flagging
                                     ↓
                             Archive old deleted
                                     ↓
                                  History
```

## Prerequisites

Before enabling soft-delete:

1. Database deployed with core tables (Staging, Current, History)
2. `usp_RefreshColumnMapping` executed at least once
3. Admin access to execute DDL scripts

## Installation

### Step 1: Add Soft-Delete Settings

```sql
-- Execute against RVToolsDW
-- src/tsql/Database/006_SoftDeleteSettings.sql

INSERT INTO Config.Settings (SettingKey, SettingValue, Description)
VALUES
    ('EnableSoftDelete', 'true', 'Enable soft-delete tracking for removed records'),
    ('SoftDeleteRetentionDays', '90', 'Days to keep soft-deleted records before archiving');
```

### Step 2: Add Columns to Current Tables

```sql
-- Execute against RVToolsDW
-- src/tsql/Tables/Current/002_AddSoftDeleteColumns.sql

-- This script adds 6 columns to all 27 Current tables:
-- IsDeleted, DeletedDate, DeletedBatchId, DeletedReason, LastSeenDate, LastSeenBatchId
```

### Step 3: Refresh Column Mapping

```sql
-- Register new columns in metadata
EXEC dbo.usp_RefreshColumnMapping @DebugMode = 1;
```

### Step 4: Re-deploy Merge Procedure

```sql
-- Re-deploy with soft-delete logic
-- src/tsql/StoredProcedures/usp_MergeTable.sql

-- The updated procedure includes:
-- - SOFT_DELETE step: Flags missing records
-- - UPDATE_LAST_SEEN step: Updates tracking columns
```

### Step 5: Deploy Archive Procedure

```sql
-- Deploy archive stored procedure
-- src/tsql/StoredProcedures/usp_ArchiveSoftDeletedRecords.sql
```

### Step 6: Re-deploy Views (Optional)

If you want views to filter out soft-deleted records by default:

```sql
-- Re-deploy all views in src/tsql/Views/
-- Views include: WHERE IsDeleted = 0 OR IsDeleted IS NULL
```

## Column Reference

| Column | Type | Description |
|--------|------|-------------|
| `IsDeleted` | BIT | Flag: 1 = soft-deleted, 0 = active |
| `DeletedDate` | DATETIME2 | UTC timestamp when record was flagged |
| `DeletedBatchId` | INT | ImportBatchId that triggered soft-delete |
| `DeletedReason` | NVARCHAR(100) | Reason code (e.g., 'MISSING_FROM_SOURCE') |
| `LastSeenDate` | DATETIME2 | Last import date where record was present |
| `LastSeenBatchId` | INT | Last ImportBatchId where record appeared |

## Archive Procedure

The `usp_ArchiveSoftDeletedRecords` procedure moves old soft-deleted records to History:

```sql
-- Archive records deleted more than 90 days ago (default)
EXEC dbo.usp_ArchiveSoftDeletedRecords;

-- Override retention days
EXEC dbo.usp_ArchiveSoftDeletedRecords @RetentionDays = 30;

-- Dry run (see what would be archived)
EXEC dbo.usp_ArchiveSoftDeletedRecords @DryRun = 1;

-- Archive specific table only
EXEC dbo.usp_ArchiveSoftDeletedRecords @TableName = 'vInfo';
```

### Scheduling Archive

Create a SQL Server Agent job or scheduled task:

```sql
-- Weekly archive (recommended)
EXEC dbo.usp_ArchiveSoftDeletedRecords;
```

## Querying Soft-Deleted Records

### View All Soft-Deleted VMs

```sql
SELECT
    VM,
    VMName,
    VI_SDK_Server,
    DeletedDate,
    LastSeenDate,
    DeletedReason
FROM Current.vInfo
WHERE IsDeleted = 1
ORDER BY DeletedDate DESC;
```

### View Recently Deleted Records (Last 7 Days)

```sql
SELECT
    'vInfo' AS TableName,
    VM AS RecordKey,
    VMName AS RecordName,
    DeletedDate,
    DeletedReason
FROM Current.vInfo
WHERE IsDeleted = 1 AND DeletedDate >= DATEADD(DAY, -7, GETUTCDATE())
UNION ALL
SELECT
    'vHost',
    Host,
    Host,
    DeletedDate,
    DeletedReason
FROM Current.vHost
WHERE IsDeleted = 1 AND DeletedDate >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY DeletedDate DESC;
```

### Count Soft-Deleted Records by Table

```sql
SELECT
    'vInfo' AS TableName,
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS DeletedCount,
    SUM(CASE WHEN IsDeleted = 0 OR IsDeleted IS NULL THEN 1 ELSE 0 END) AS ActiveCount
FROM Current.vInfo
UNION ALL
SELECT 'vHost',
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END),
    SUM(CASE WHEN IsDeleted = 0 OR IsDeleted IS NULL THEN 1 ELSE 0 END)
FROM Current.vHost
UNION ALL
SELECT 'vDatastore',
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END),
    SUM(CASE WHEN IsDeleted = 0 OR IsDeleted IS NULL THEN 1 ELSE 0 END)
FROM Current.vDatastore;
```

### Restore a Soft-Deleted Record

If a record was incorrectly flagged as deleted:

```sql
-- Restore a specific VM
UPDATE Current.vInfo
SET
    IsDeleted = 0,
    DeletedDate = NULL,
    DeletedBatchId = NULL,
    DeletedReason = NULL
WHERE VM_UUID = 'your-vm-uuid' AND VI_SDK_Server = 'vcenter.domain.com';
```

## Configuration Options

### Enable/Disable Soft-Delete

```sql
-- Disable soft-delete (records will be permanently removed)
UPDATE Config.Settings SET SettingValue = 'false' WHERE SettingKey = 'EnableSoftDelete';

-- Enable soft-delete
UPDATE Config.Settings SET SettingValue = 'true' WHERE SettingKey = 'EnableSoftDelete';
```

### Change Retention Period

```sql
-- Keep soft-deleted records for 180 days before archiving
UPDATE Config.Settings SET SettingValue = '180' WHERE SettingKey = 'SoftDeleteRetentionDays';
```

## View Considerations

### Default Views

Reporting views in `src/tsql/Views/` are designed to:
- Filter out soft-deleted records by default (`WHERE IsDeleted = 0 OR IsDeleted IS NULL`)
- Show only active records in dashboards

### Including Soft-Deleted Records

To include soft-deleted records in a view or report:

```sql
-- All records including deleted
SELECT * FROM Current.vInfo;

-- Only active records
SELECT * FROM Current.vInfo WHERE IsDeleted = 0 OR IsDeleted IS NULL;

-- Only soft-deleted records
SELECT * FROM Current.vInfo WHERE IsDeleted = 1;
```

## Troubleshooting

### Soft-Delete Not Working

1. **Check if enabled**:
   ```sql
   SELECT * FROM Config.Settings WHERE SettingKey = 'EnableSoftDelete';
   ```

2. **Verify columns exist**:
   ```sql
   SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = 'Current' AND TABLE_NAME = 'vInfo' AND COLUMN_NAME LIKE '%Deleted%';
   ```

3. **Check merge procedure version**:
   - Re-deploy `usp_MergeTable.sql` with soft-delete logic

### Records Not Being Flagged

The MERGE process only flags records when:
- The record exists in Current but NOT in the new import
- The import successfully processed (no errors in Staging)
- The record's natural key doesn't match any incoming data

### Archive Not Working

1. **Check retention setting**:
   ```sql
   SELECT * FROM Config.Settings WHERE SettingKey = 'SoftDeleteRetentionDays';
   ```

2. **Verify archive procedure exists**:
   ```sql
   SELECT name FROM sys.procedures WHERE name = 'usp_ArchiveSoftDeletedRecords';
   ```

3. **Run with dry run**:
   ```sql
   EXEC dbo.usp_ArchiveSoftDeletedRecords @DryRun = 1;
   ```

## Best Practices

1. **Start with longer retention** - Begin with 90+ days to catch issues
2. **Monitor deleted counts** - Review soft-deleted records regularly
3. **Schedule archive jobs** - Run weekly to prevent table bloat
4. **Update views** - Ensure Reporting views filter deleted records
5. **Document exceptions** - Note cases where deleted records should appear in reports

---

## Related Documentation

- [Installation Guide](../installation.md) - Complete deployment steps
- [Extending Tables](extending-tables.md) - Adding new tables to the schema
- [Configuration](../configuration.md) - Config.Settings reference
- [Architecture Overview](../architecture/overview.md) - System design
