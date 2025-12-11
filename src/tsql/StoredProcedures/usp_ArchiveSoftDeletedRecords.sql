/*
    RVTools Data Warehouse - Archive Soft-Deleted Records

    Purpose: Archives soft-deleted records from Current to History after retention period.
             Records are MOVED to History (not permanently deleted) to maintain audit trail.

    Process:
        1. For each Current table with soft-delete columns:
        2. Find records WHERE IsDeleted=1 AND DeletedDate < (NOW - RetentionDays)
        3. INSERT into corresponding History table with ValidTo = DeletedDate
        4. DELETE from Current table (record preserved in History)
        5. Log archived counts to Audit.ImportLog

    Parameters:
        @RetentionDays  - Override retention (default from Config.Settings 'SoftDeleteRetentionDays')
        @DryRun         - If 1, preview only (no changes made)
        @TableName      - Optional: process single table only
        @DebugMode      - If 1, print detailed progress

    Design Decisions:
        - Records are archived to History, NOT permanently deleted
        - History records get ValidTo = DeletedDate (proper SCD Type 2 closure)
        - Default retention: 90 days (configurable in Config.Settings)
        - Supports DryRun mode for safe preview

    Usage:
        -- Preview what would be archived
        EXEC dbo.usp_ArchiveSoftDeletedRecords @DryRun = 1;

        -- Archive all tables with default retention
        EXEC dbo.usp_ArchiveSoftDeletedRecords;

        -- Archive single table with custom retention
        EXEC dbo.usp_ArchiveSoftDeletedRecords @TableName = 'vInfo', @RetentionDays = 30;

    Related Files:
        - src/tsql/Database/006_SoftDeleteSettings.sql (configuration)
        - src/tsql/Tables/Current/002_AddSoftDeleteColumns.sql (schema)
        - src/tsql/StoredProcedures/usp_MergeTable.sql (soft-delete detection)
*/

USE [RVToolsDW]
GO

IF OBJECT_ID('dbo.usp_ArchiveSoftDeletedRecords', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_ArchiveSoftDeletedRecords;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE dbo.usp_ArchiveSoftDeletedRecords
    @RetentionDays INT = NULL,           -- Override retention (NULL = use Config.Settings)
    @DryRun BIT = 0,                     -- Preview only, no changes
    @TableName NVARCHAR(100) = NULL,     -- Process single table (NULL = all tables)
    @DebugMode BIT = 0                   -- Print detailed progress
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    DECLARE @CutoffDate DATETIME2;
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CurrentTable NVARCHAR(100);
    DECLARE @RowCount INT;
    DECLARE @TotalArchived INT = 0;
    DECLARE @ErrorMsg NVARCHAR(MAX);

    -- Results table for summary
    DECLARE @Results TABLE (
        TableName NVARCHAR(100),
        RecordsToArchive INT,
        RecordsArchived INT,
        Status NVARCHAR(20)
    );

    -- ========================================================================
    -- Get retention days from Config.Settings if not provided
    -- ========================================================================
    IF @RetentionDays IS NULL
    BEGIN
        SELECT @RetentionDays = TRY_CAST(SettingValue AS INT)
        FROM Config.Settings
        WHERE SettingName = 'SoftDeleteRetentionDays';

        IF @RetentionDays IS NULL
            SET @RetentionDays = 90;  -- Default fallback
    END

    SET @CutoffDate = DATEADD(DAY, -@RetentionDays, GETUTCDATE());

    IF @DebugMode = 1
    BEGIN
        PRINT 'Archive Soft-Deleted Records';
        PRINT '============================';
        PRINT 'Retention Days: ' + CAST(@RetentionDays AS VARCHAR);
        PRINT 'Cutoff Date: ' + CONVERT(VARCHAR, @CutoffDate, 120);
        PRINT 'Dry Run: ' + CASE WHEN @DryRun = 1 THEN 'YES (preview only)' ELSE 'NO (will archive)' END;
        PRINT 'Table Filter: ' + ISNULL(@TableName, '(all tables)');
        PRINT '';
    END

    -- ========================================================================
    -- Process each table with soft-delete columns
    -- ========================================================================
    DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT DISTINCT t.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        INNER JOIN sys.columns c ON t.object_id = c.object_id
        WHERE s.name = 'Current'
          AND c.name = 'IsDeleted'
          AND (@TableName IS NULL OR t.name = @TableName)
        ORDER BY t.name;

    OPEN table_cursor;
    FETCH NEXT FROM table_cursor INTO @CurrentTable;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @DebugMode = 1
            PRINT 'Processing: Current.' + @CurrentTable;

        -- Count records to archive
        DECLARE @CountToArchive INT = 0;
        SET @SQL = N'SELECT @cnt = COUNT(*) FROM [Current].' + QUOTENAME(@CurrentTable) +
                   ' WHERE IsDeleted = 1 AND DeletedDate < @cutoff';

        BEGIN TRY
            EXEC sp_executesql @SQL,
                N'@cutoff DATETIME2, @cnt INT OUTPUT',
                @cutoff = @CutoffDate,
                @cnt = @CountToArchive OUTPUT;
        END TRY
        BEGIN CATCH
            SET @CountToArchive = 0;
        END CATCH

        IF @CountToArchive = 0
        BEGIN
            IF @DebugMode = 1
                PRINT '  No records to archive';

            INSERT INTO @Results (TableName, RecordsToArchive, RecordsArchived, Status)
            VALUES (@CurrentTable, 0, 0, 'Skipped');

            FETCH NEXT FROM table_cursor INTO @CurrentTable;
            CONTINUE;
        END

        IF @DebugMode = 1
            PRINT '  Records to archive: ' + CAST(@CountToArchive AS VARCHAR);

        IF @DryRun = 1
        BEGIN
            -- Dry run - just record the count
            INSERT INTO @Results (TableName, RecordsToArchive, RecordsArchived, Status)
            VALUES (@CurrentTable, @CountToArchive, 0, 'DryRun');

            IF @DebugMode = 1
                PRINT '  [DRY RUN] Would archive ' + CAST(@CountToArchive AS VARCHAR) + ' records';
        END
        ELSE
        BEGIN
            -- Build column list for History insert (exclude soft-delete columns, they're not in History)
            DECLARE @HistoryColumns NVARCHAR(MAX) = '';
            DECLARE @CurrentColumns NVARCHAR(MAX) = '';

            SELECT
                @HistoryColumns = @HistoryColumns +
                    CASE WHEN LEN(@HistoryColumns) > 0 THEN ', ' ELSE '' END +
                    QUOTENAME(c.name),
                @CurrentColumns = @CurrentColumns +
                    CASE WHEN LEN(@CurrentColumns) > 0 THEN ', ' ELSE '' END +
                    'c.' + QUOTENAME(c.name)
            FROM sys.columns c
            INNER JOIN sys.tables t ON c.object_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = 'Current'
              AND t.name = @CurrentTable
              AND c.name NOT IN (
                  -- Exclude system/soft-delete columns not in History
                  'LastSeenBatchId', 'LastSeenDate', 'IsDeleted',
                  'DeletedBatchId', 'DeletedDate', 'DeletedReason',
                  'CreatedDate', 'LastModifiedDate', 'ModifiedDate'
              )
              AND c.is_identity = 0
              AND c.is_computed = 0
            ORDER BY c.column_id;

            BEGIN TRY
                BEGIN TRANSACTION;

                -- Step 1: Insert into History with ValidTo = DeletedDate
                SET @SQL = N'
                INSERT INTO [History].' + QUOTENAME(@CurrentTable) + ' (
                    ImportBatchId, ValidFrom, ValidTo, SourceFile,
                    ' + @HistoryColumns + '
                )
                SELECT
                    c.ImportBatchId,
                    c.DeletedDate,      -- ValidFrom = when it was marked deleted
                    c.DeletedDate,      -- ValidTo = same (record is closed)
                    ''Archived-SoftDelete'',
                    ' + @CurrentColumns + '
                FROM [Current].' + QUOTENAME(@CurrentTable) + ' c
                WHERE c.IsDeleted = 1
                  AND c.DeletedDate < @cutoff;

                SELECT @archived = @@ROWCOUNT;
                ';

                EXEC sp_executesql @SQL,
                    N'@cutoff DATETIME2, @archived INT OUTPUT',
                    @cutoff = @CutoffDate,
                    @archived = @RowCount OUTPUT;

                IF @DebugMode = 1
                    PRINT '  Inserted ' + CAST(@RowCount AS VARCHAR) + ' records into History';

                -- Step 2: Delete from Current
                SET @SQL = N'
                DELETE FROM [Current].' + QUOTENAME(@CurrentTable) + '
                WHERE IsDeleted = 1
                  AND DeletedDate < @cutoff;
                ';

                EXEC sp_executesql @SQL,
                    N'@cutoff DATETIME2',
                    @cutoff = @CutoffDate;

                IF @DebugMode = 1
                    PRINT '  Deleted ' + CAST(@RowCount AS VARCHAR) + ' records from Current';

                COMMIT TRANSACTION;

                INSERT INTO @Results (TableName, RecordsToArchive, RecordsArchived, Status)
                VALUES (@CurrentTable, @CountToArchive, @RowCount, 'Success');

                SET @TotalArchived = @TotalArchived + @RowCount;

            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0
                    ROLLBACK TRANSACTION;

                SET @ErrorMsg = ERROR_MESSAGE();

                IF @DebugMode = 1
                    PRINT '  ERROR: ' + @ErrorMsg;

                INSERT INTO @Results (TableName, RecordsToArchive, RecordsArchived, Status)
                VALUES (@CurrentTable, @CountToArchive, 0, 'Failed');

                -- Log error
                INSERT INTO Audit.ErrorLog (ProcedureName, TableName, Operation, ErrorMessage)
                VALUES ('usp_ArchiveSoftDeletedRecords', @CurrentTable, 'ARCHIVE', @ErrorMsg);
            END CATCH
        END

        FETCH NEXT FROM table_cursor INTO @CurrentTable;
    END

    CLOSE table_cursor;
    DEALLOCATE table_cursor;

    -- ========================================================================
    -- Log summary to Audit.ImportLog
    -- ========================================================================
    IF @DryRun = 0 AND @TotalArchived > 0
    BEGIN
        INSERT INTO Audit.ImportLog (Source, LogLevel, Message)
        VALUES (
            'usp_ArchiveSoftDeletedRecords',
            'Info',
            'Archived ' + CAST(@TotalArchived AS VARCHAR) +
            ' soft-deleted records to History (retention: ' +
            CAST(@RetentionDays AS VARCHAR) + ' days)'
        );
    END

    -- ========================================================================
    -- Return summary
    -- ========================================================================
    SELECT
        TableName,
        RecordsToArchive,
        RecordsArchived,
        Status
    FROM @Results
    ORDER BY TableName;

    -- Summary row
    SELECT
        'TOTAL' AS TableName,
        SUM(RecordsToArchive) AS RecordsToArchive,
        SUM(RecordsArchived) AS RecordsArchived,
        CASE WHEN @DryRun = 1 THEN 'DryRun' ELSE 'Completed' END AS Status
    FROM @Results;

    IF @DebugMode = 1
    BEGIN
        PRINT '';
        PRINT 'Archive completed.';
        PRINT 'Total records archived: ' + CAST(@TotalArchived AS VARCHAR);
        PRINT 'Duration: ' + CAST(DATEDIFF(SECOND, @StartTime, GETUTCDATE()) AS VARCHAR) + ' seconds';
    END

END
GO

PRINT 'Created dbo.usp_ArchiveSoftDeletedRecords';
GO
