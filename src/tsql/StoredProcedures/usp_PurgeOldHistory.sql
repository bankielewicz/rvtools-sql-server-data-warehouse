/*
    RVTools Data Warehouse - Purge Old History

    Purpose: Removes history records older than retention period

    Parameters:
        @RetentionDays - Number of days to retain (default from Config.Settings)
        @DryRun - If 1, only reports what would be deleted

    Usage:
        -- Check what would be deleted
        EXEC [dbo].[usp_PurgeOldHistory] @DryRun = 1

        -- Actually purge
        EXEC [dbo].[usp_PurgeOldHistory] @RetentionDays = 365
*/

USE [RVToolsDW]
GO

IF OBJECT_ID('dbo.usp_PurgeOldHistory', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_PurgeOldHistory]
GO

CREATE PROCEDURE [dbo].[usp_PurgeOldHistory]
    @RetentionDays INT = NULL,
    @DryRun BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Get retention days from config if not specified
    IF @RetentionDays IS NULL
    BEGIN
        SELECT @RetentionDays = TRY_CAST(SettingValue AS INT)
        FROM [Config].[Settings]
        WHERE SettingName = 'RetentionDays'

        IF @RetentionDays IS NULL
            SET @RetentionDays = 365  -- Default
    END

    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@RetentionDays, GETUTCDATE())
    DECLARE @TableName NVARCHAR(100)
    DECLARE @RowCount INT
    DECLARE @TotalDeleted INT = 0
    DECLARE @SQL NVARCHAR(MAX)

    -- Table list
    DECLARE @Tables TABLE (TableName NVARCHAR(100))
    INSERT INTO @Tables VALUES
        ('vInfo'), ('vCPU'), ('vMemory'), ('vDisk'), ('vPartition'),
        ('vNetwork'), ('vCD'), ('vUSB'), ('vSnapshot'), ('vTools'),
        ('vSource'), ('vRP'), ('vCluster'), ('vHost'), ('vHBA'),
        ('vNIC'), ('vSwitch'), ('vPort'), ('dvSwitch'), ('dvPort'),
        ('vSC_VMK'), ('vDatastore'), ('vMultiPath'), ('vLicense'),
        ('vFileInfo'), ('vHealth'), ('vMetaData')

    -- Results table
    CREATE TABLE #PurgeResults (
        TableName NVARCHAR(100),
        RowsToDelete INT,
        RowsDeleted INT
    )

    DECLARE table_cursor CURSOR FOR
        SELECT TableName FROM @Tables

    OPEN table_cursor
    FETCH NEXT FROM table_cursor INTO @TableName

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Count rows to delete
        SET @SQL = N'SELECT @cnt = COUNT(*) FROM [History].[' + @TableName + N']
                     WHERE ValidTo IS NOT NULL AND ValidTo < @cutoff'

        EXEC sp_executesql @SQL,
            N'@cutoff DATETIME2, @cnt INT OUTPUT',
            @cutoff = @CutoffDate,
            @cnt = @RowCount OUTPUT

        IF @DryRun = 0 AND @RowCount > 0
        BEGIN
            -- Actually delete
            SET @SQL = N'DELETE FROM [History].[' + @TableName + N']
                         WHERE ValidTo IS NOT NULL AND ValidTo < @cutoff'

            EXEC sp_executesql @SQL, N'@cutoff DATETIME2', @cutoff = @CutoffDate

            INSERT INTO #PurgeResults VALUES (@TableName, @RowCount, @@ROWCOUNT)
            SET @TotalDeleted = @TotalDeleted + @@ROWCOUNT
        END
        ELSE
        BEGIN
            INSERT INTO #PurgeResults VALUES (@TableName, @RowCount, 0)
        END

        FETCH NEXT FROM table_cursor INTO @TableName
    END

    CLOSE table_cursor
    DEALLOCATE table_cursor

    -- Log the purge
    IF @DryRun = 0
    BEGIN
        INSERT INTO [Audit].[ImportLog] (LogLevel, Message)
        VALUES ('Info', 'Purged ' + CAST(@TotalDeleted AS VARCHAR(20)) +
                ' history records older than ' + CAST(@RetentionDays AS VARCHAR(10)) + ' days')
    END

    -- Return results
    SELECT
        TableName,
        RowsToDelete,
        CASE WHEN @DryRun = 1 THEN 0 ELSE RowsDeleted END AS RowsDeleted,
        @CutoffDate AS CutoffDate,
        @RetentionDays AS RetentionDays,
        @DryRun AS DryRun
    FROM #PurgeResults
    WHERE RowsToDelete > 0
    ORDER BY RowsToDelete DESC

    DROP TABLE #PurgeResults
END
GO

PRINT 'Created [dbo].[usp_PurgeOldHistory]'
GO
