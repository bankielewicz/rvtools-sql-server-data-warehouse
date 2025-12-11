/*
    RVTools Data Warehouse - Add Soft Delete Columns to Current Tables

    Purpose: Adds soft-delete/tombstone tracking columns to ALL 27 Current.* tables

    Columns Added:
        - LastSeenBatchId INT NULL: Last import batch that included this record
        - LastSeenDate DATETIME2 NULL: Timestamp of last import that included this record
        - IsDeleted BIT NOT NULL DEFAULT 0: Soft delete flag (1 = deleted from source)
        - DeletedBatchId INT NULL: Import batch that detected deletion
        - DeletedDate DATETIME2 NULL: When marked as deleted
        - DeletedReason NVARCHAR(50) NULL: 'NotInSource', 'ManualPurge', 'Archived'

    Design Decisions:
        - Immediate marking: Records marked deleted on first missing import
        - Multi-vCenter safe: Only marks deleted for same VI_SDK_Server being imported
        - Partial import safe: Only detects deletions for tables with data in import
        - 90-day retention: After 90 days, deleted records archived to History

    Usage: Execute against RVToolsDW database after 001_AllCurrentTables.sql
           sqlcmd -S localhost -d RVToolsDW -i 002_AddSoftDeleteColumns.sql

    Related Files:
        - src/tsql/Database/006_SoftDeleteSettings.sql (configuration)
        - src/tsql/StoredProcedures/usp_MergeTable.sql (detection logic)
        - src/tsql/StoredProcedures/usp_ArchiveSoftDeletedRecords.sql (archive procedure)
*/

USE [RVToolsDW]
GO

SET NOCOUNT ON;
PRINT 'Adding soft-delete columns to all 27 Current.* tables...';
PRINT '';

-- ============================================================================
-- Helper: Add columns to a specific table if they don't exist
-- ============================================================================
IF OBJECT_ID('tempdb..#AddSoftDeleteColumns') IS NOT NULL
    DROP PROCEDURE #AddSoftDeleteColumns;
GO

CREATE PROCEDURE #AddSoftDeleteColumns
    @TableName NVARCHAR(100)
AS
BEGIN
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @FullTableName NVARCHAR(200) = '[Current].[' + @TableName + ']';
    DECLARE @ConstraintName NVARCHAR(200) = 'DF_' + @TableName + '_IsDeleted';

    -- Check if table exists
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables t
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName
    )
    BEGIN
        PRINT '  SKIPPED: ' + @FullTableName + ' does not exist';
        RETURN;
    END

    -- Add LastSeenBatchId if not exists
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns c
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND c.name = 'LastSeenBatchId'
    )
    BEGIN
        SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD [LastSeenBatchId] INT NULL;';
        EXEC sp_executesql @SQL;
    END

    -- Add LastSeenDate if not exists
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns c
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND c.name = 'LastSeenDate'
    )
    BEGIN
        SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD [LastSeenDate] DATETIME2 NULL;';
        EXEC sp_executesql @SQL;
    END

    -- Add IsDeleted if not exists (with default constraint)
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns c
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND c.name = 'IsDeleted'
    )
    BEGIN
        SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD [IsDeleted] BIT NOT NULL CONSTRAINT ' + @ConstraintName + ' DEFAULT 0;';
        EXEC sp_executesql @SQL;
    END

    -- Add DeletedBatchId if not exists
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns c
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND c.name = 'DeletedBatchId'
    )
    BEGIN
        SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD [DeletedBatchId] INT NULL;';
        EXEC sp_executesql @SQL;
    END

    -- Add DeletedDate if not exists
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns c
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND c.name = 'DeletedDate'
    )
    BEGIN
        SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD [DeletedDate] DATETIME2 NULL;';
        EXEC sp_executesql @SQL;
    END

    -- Add DeletedReason if not exists
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns c
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND c.name = 'DeletedReason'
    )
    BEGIN
        SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD [DeletedReason] NVARCHAR(50) NULL;';
        EXEC sp_executesql @SQL;
    END

    PRINT '  Added columns to: ' + @FullTableName;
END
GO

-- ============================================================================
-- VM-Related Tables (10)
-- ============================================================================
PRINT 'VM-Related Tables:';
EXEC #AddSoftDeleteColumns 'vInfo';
EXEC #AddSoftDeleteColumns 'vCPU';
EXEC #AddSoftDeleteColumns 'vMemory';
EXEC #AddSoftDeleteColumns 'vDisk';
EXEC #AddSoftDeleteColumns 'vPartition';
EXEC #AddSoftDeleteColumns 'vNetwork';
EXEC #AddSoftDeleteColumns 'vCD';
EXEC #AddSoftDeleteColumns 'vUSB';
EXEC #AddSoftDeleteColumns 'vSnapshot';
EXEC #AddSoftDeleteColumns 'vTools';
PRINT '';

-- ============================================================================
-- Host-Related Tables (3)
-- ============================================================================
PRINT 'Host-Related Tables:';
EXEC #AddSoftDeleteColumns 'vHost';
EXEC #AddSoftDeleteColumns 'vHBA';
EXEC #AddSoftDeleteColumns 'vNIC';
PRINT '';

-- ============================================================================
-- Network/Switch Tables (5)
-- ============================================================================
PRINT 'Network/Switch Tables:';
EXEC #AddSoftDeleteColumns 'vSwitch';
EXEC #AddSoftDeleteColumns 'vPort';
EXEC #AddSoftDeleteColumns 'dvSwitch';
EXEC #AddSoftDeleteColumns 'dvPort';
EXEC #AddSoftDeleteColumns 'vSC_VMK';
PRINT '';

-- ============================================================================
-- Storage Tables (3)
-- ============================================================================
PRINT 'Storage Tables:';
EXEC #AddSoftDeleteColumns 'vDatastore';
EXEC #AddSoftDeleteColumns 'vMultiPath';
EXEC #AddSoftDeleteColumns 'vFileInfo';
PRINT '';

-- ============================================================================
-- Cluster/Resource Tables (2)
-- ============================================================================
PRINT 'Cluster/Resource Tables:';
EXEC #AddSoftDeleteColumns 'vCluster';
EXEC #AddSoftDeleteColumns 'vRP';
PRINT '';

-- ============================================================================
-- Infrastructure Tables (2)
-- ============================================================================
PRINT 'Infrastructure Tables:';
EXEC #AddSoftDeleteColumns 'vSource';
EXEC #AddSoftDeleteColumns 'vLicense';
PRINT '';

-- ============================================================================
-- Health/Metadata Tables (2)
-- ============================================================================
PRINT 'Health/Metadata Tables:';
EXEC #AddSoftDeleteColumns 'vHealth';
EXEC #AddSoftDeleteColumns 'vMetaData';
PRINT '';

-- ============================================================================
-- Cleanup temp procedure
-- ============================================================================
DROP PROCEDURE #AddSoftDeleteColumns;
GO

-- ============================================================================
-- Create Index for IsDeleted filtering (optional but recommended)
-- ============================================================================
PRINT 'Creating filtered indexes for IsDeleted = 0 queries...';

DECLARE @IndexSQL NVARCHAR(MAX);
DECLARE @TableName NVARCHAR(100);
DECLARE @IndexName NVARCHAR(200);

DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT t.name
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'Current'
    ORDER BY t.name;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @IndexName = 'IX_' + @TableName + '_IsDeleted';

    -- Check if index already exists
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        INNER JOIN sys.tables t ON i.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND i.name = @IndexName
    )
    BEGIN
        -- Check if IsDeleted column exists
        IF EXISTS (
            SELECT 1 FROM sys.columns c
            INNER JOIN sys.tables t ON c.object_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = 'Current' AND t.name = @TableName AND c.name = 'IsDeleted'
        )
        BEGIN
            SET @IndexSQL = 'CREATE NONCLUSTERED INDEX ' + QUOTENAME(@IndexName) +
                           ' ON [Current].' + QUOTENAME(@TableName) + ' (IsDeleted) WHERE IsDeleted = 0;';
            BEGIN TRY
                EXEC sp_executesql @IndexSQL;
                PRINT '  Created index: ' + @IndexName;
            END TRY
            BEGIN CATCH
                PRINT '  FAILED to create index: ' + @IndexName + ' - ' + ERROR_MESSAGE();
            END CATCH
        END
    END
    ELSE
    BEGIN
        PRINT '  Index exists: ' + @IndexName;
    END

    FETCH NEXT FROM table_cursor INTO @TableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;
GO

-- ============================================================================
-- Initialize LastSeenBatchId/LastSeenDate for existing records
-- This prevents existing records from being marked as deleted on next import
-- ============================================================================
PRINT 'Initializing LastSeenBatchId/LastSeenDate for existing records...';

DECLARE @InitSQL NVARCHAR(MAX);
DECLARE @InitTableName NVARCHAR(100);
DECLARE @InitRowCount INT;

DECLARE init_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT t.name
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    INNER JOIN sys.columns c ON t.object_id = c.object_id
    WHERE s.name = 'Current'
      AND c.name = 'LastSeenBatchId'
    ORDER BY t.name;

OPEN init_cursor;
FETCH NEXT FROM init_cursor INTO @InitTableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Update existing records: set LastSeenBatchId/Date from ImportBatchId/LastModifiedDate
    SET @InitSQL = N'
    UPDATE [Current].' + QUOTENAME(@InitTableName) + '
    SET LastSeenBatchId = ImportBatchId,
        LastSeenDate = ISNULL(LastModifiedDate, GETUTCDATE())
    WHERE LastSeenBatchId IS NULL
      AND ImportBatchId IS NOT NULL;
    SELECT @cnt = @@ROWCOUNT;';

    BEGIN TRY
        EXEC sp_executesql @InitSQL, N'@cnt INT OUTPUT', @cnt = @InitRowCount OUTPUT;
        IF @InitRowCount > 0
            PRINT '  Initialized ' + CAST(@InitRowCount AS VARCHAR) + ' records in Current.' + @InitTableName;
    END TRY
    BEGIN CATCH
        PRINT '  FAILED to initialize Current.' + @InitTableName + ': ' + ERROR_MESSAGE();
    END CATCH

    FETCH NEXT FROM init_cursor INTO @InitTableName;
END

CLOSE init_cursor;
DEALLOCATE init_cursor;
GO

-- ============================================================================
-- Verification
-- ============================================================================
PRINT '';
PRINT 'Verification - Soft-delete columns added to Current tables:';
PRINT '';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    MAX(CASE WHEN c.name = 'LastSeenBatchId' THEN 'YES' ELSE 'NO' END) AS LastSeenBatchId,
    MAX(CASE WHEN c.name = 'LastSeenDate' THEN 'YES' ELSE 'NO' END) AS LastSeenDate,
    MAX(CASE WHEN c.name = 'IsDeleted' THEN 'YES' ELSE 'NO' END) AS IsDeleted,
    MAX(CASE WHEN c.name = 'DeletedBatchId' THEN 'YES' ELSE 'NO' END) AS DeletedBatchId,
    MAX(CASE WHEN c.name = 'DeletedDate' THEN 'YES' ELSE 'NO' END) AS DeletedDate,
    MAX(CASE WHEN c.name = 'DeletedReason' THEN 'YES' ELSE 'NO' END) AS DeletedReason
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE s.name = 'Current'
  AND c.name IN ('LastSeenBatchId', 'LastSeenDate', 'IsDeleted', 'DeletedBatchId', 'DeletedDate', 'DeletedReason')
GROUP BY s.name, t.name
ORDER BY t.name;
GO

PRINT '';
PRINT 'Soft-delete columns installation complete.';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Run usp_RefreshColumnMapping to register new columns';
PRINT '  2. Deploy updated usp_MergeTable with soft-delete detection';
PRINT '  3. Deploy usp_ArchiveSoftDeletedRecords for cleanup';
GO
