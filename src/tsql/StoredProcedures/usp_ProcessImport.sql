/*
    RVTools Data Warehouse - Main Import Orchestrator

    Purpose: Main stored procedure that orchestrates the import process
             Called after PowerShell loads data into staging tables

    Parameters:
        @ImportBatchId - The batch ID created by PowerShell
        @SourceFile - Source xlsx file name

    Process:
        1. Process each table independently (no cascading failures)
        2. For each table: Archive changes to History, Merge to Current
        3. Update audit records with partial success status
        4. Return summary including any failures

    Error Handling:
        - Each table is processed in its own TRY/CATCH
        - Failures are logged but don't stop other tables
        - Final status reflects partial success if some tables failed

    Usage:
        EXEC [dbo].[usp_ProcessImport] @ImportBatchId = 1, @SourceFile = 'export.xlsx'
*/

USE [RVToolsDW]
GO

IF OBJECT_ID('dbo.usp_ProcessImport', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_ProcessImport]
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE [dbo].[usp_ProcessImport]
    @ImportBatchId INT,
    @SourceFile NVARCHAR(500) = NULL,
    @RVToolsExportDate DATETIME2 = NULL  -- Override for ValidFrom (historical imports)
AS
BEGIN
    SET NOCOUNT ON;
    -- NOTE: Removed XACT_ABORT to allow per-table error handling

    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    DECLARE @SheetName NVARCHAR(100);
    DECLARE @RowCount INT;
    DECLARE @ErrorMessage NVARCHAR(MAX);
    DECLARE @TotalMerged INT = 0;
    DECLARE @SheetsProcessed INT = 0;
    DECLARE @SheetsSucceeded INT = 0;
    DECLARE @SheetsFailed INT = 0;
    DECLARE @SheetsSkipped INT = 0;

    -- Log start
    INSERT INTO [Audit].[ImportLog] (ImportBatchId, LogLevel, Message)
    VALUES (@ImportBatchId, 'Info', 'Starting import processing for batch ' + CAST(@ImportBatchId AS VARCHAR(10)));

    -- ================================================================
    -- Process all tables from Config.TableMapping
    -- Each table is processed independently - failures don't cascade
    -- ================================================================
    DECLARE @AllTables TABLE (
        TableName NVARCHAR(100),
        ProcessOrder INT IDENTITY(1,1)
    );

    -- Define processing order (most important first)
    INSERT INTO @AllTables (TableName)
    SELECT TableName FROM Config.TableMapping
    WHERE IsActive = 1
    ORDER BY
        CASE TableName
            WHEN 'vInfo' THEN 1
            WHEN 'vHost' THEN 2
            WHEN 'vCluster' THEN 3
            WHEN 'vDatastore' THEN 4
            WHEN 'vCPU' THEN 5
            WHEN 'vMemory' THEN 6
            WHEN 'vDisk' THEN 7
            WHEN 'vPartition' THEN 8
            WHEN 'vNetwork' THEN 9
            WHEN 'vSnapshot' THEN 10
            ELSE 100
        END,
        TableName;

    DECLARE table_cursor CURSOR FOR
        SELECT TableName FROM @AllTables ORDER BY ProcessOrder;

    OPEN table_cursor;
    FETCH NEXT FROM table_cursor INTO @SheetName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @RowCount = 0;

        BEGIN TRY
            EXEC [dbo].[usp_MergeTable]
                @ImportBatchId = @ImportBatchId,
                @TableName = @SheetName,
                @SourceFile = @SourceFile,
                @EffectiveDate = @RVToolsExportDate,
                @MergedCount = @RowCount OUTPUT;

            SET @TotalMerged = @TotalMerged + ISNULL(@RowCount, 0);
            SET @SheetsProcessed = @SheetsProcessed + 1;

            IF @RowCount > 0
                SET @SheetsSucceeded = @SheetsSucceeded + 1;
            ELSE
                SET @SheetsSkipped = @SheetsSkipped + 1;

        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE();
            SET @SheetsProcessed = @SheetsProcessed + 1;
            SET @SheetsFailed = @SheetsFailed + 1;

            -- Error is already logged by usp_MergeTable, just note it here
            INSERT INTO [Audit].[ImportLog] (ImportBatchId, LogLevel, Message)
            VALUES (@ImportBatchId, 'Warning',
                'Table ' + @SheetName + ' failed: ' + LEFT(@ErrorMessage, 500));
        END CATCH

        FETCH NEXT FROM table_cursor INTO @SheetName;
    END

    CLOSE table_cursor;
    DEALLOCATE table_cursor;

    -- ================================================================
    -- Determine final status
    -- ================================================================
    DECLARE @FinalStatus NVARCHAR(20);
    SET @FinalStatus = CASE
        WHEN @SheetsFailed = 0 THEN 'Success'
        WHEN @SheetsSucceeded = 0 THEN 'Failed'
        ELSE 'Partial'
    END;

    -- ================================================================
    -- Update Import Batch record
    -- ================================================================
    UPDATE [Audit].[ImportBatch]
    SET ImportEndTime = GETUTCDATE(),
        Status = @FinalStatus,
        SheetsProcessed = @SheetsProcessed,
        TotalRowsMerged = @TotalMerged,
        ErrorMessage = CASE
            WHEN @SheetsFailed > 0 THEN CAST(@SheetsFailed AS VARCHAR) + ' table(s) failed. Check Audit.ErrorLog for details.'
            ELSE NULL
        END
    WHERE ImportBatchId = @ImportBatchId;

    -- Log completion
    INSERT INTO [Audit].[ImportLog] (ImportBatchId, LogLevel, Message, DurationMs)
    VALUES (@ImportBatchId, 'Info',
        'Import completed: Status=' + @FinalStatus +
        ', Tables=' + CAST(@SheetsProcessed AS VARCHAR(10)) +
        ', Succeeded=' + CAST(@SheetsSucceeded AS VARCHAR(10)) +
        ', Failed=' + CAST(@SheetsFailed AS VARCHAR(10)) +
        ', Skipped=' + CAST(@SheetsSkipped AS VARCHAR(10)) +
        ', Rows=' + CAST(@TotalMerged AS VARCHAR(10)),
        DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE()));

    -- ================================================================
    -- Auto-sync vCenters to Config.ActiveVCenters
    -- This ensures new vCenters are automatically discovered
    -- ================================================================
    BEGIN TRY
        EXEC [dbo].[usp_SyncActiveVCenters];
    END TRY
    BEGIN CATCH
        -- Non-critical, just log and continue
        INSERT INTO [Audit].[ImportLog] (ImportBatchId, LogLevel, Message)
        VALUES (@ImportBatchId, 'Warning', 'Failed to sync active vCenters: ' + ERROR_MESSAGE());
    END CATCH

    -- Return summary
    SELECT
        @ImportBatchId AS ImportBatchId,
        @FinalStatus AS Status,
        @SheetsProcessed AS SheetsProcessed,
        @SheetsSucceeded AS SheetsSucceeded,
        @SheetsFailed AS SheetsFailed,
        @SheetsSkipped AS SheetsSkipped,
        @TotalMerged AS TotalRowsMerged,
        DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE()) AS DurationMs;

END
GO

PRINT 'Created [dbo].[usp_ProcessImport] with per-table error handling';
GO
