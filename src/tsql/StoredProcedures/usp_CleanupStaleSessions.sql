/*
    RVTools Data Warehouse - Cleanup Stale Sessions

    Purpose: Removes session records older than retention period

    Parameters:
        @RetentionDays - Number of days to retain (default: 90)
        @DryRun - If 1, only reports what would be deleted

    Usage:
        -- Check what would be deleted
        EXEC [dbo].[usp_CleanupStaleSessions] @RetentionDays = 90, @DryRun = 1

        -- Actually purge (default 90 days)
        EXEC [dbo].[usp_CleanupStaleSessions]

        -- Custom retention
        EXEC [dbo].[usp_CleanupStaleSessions] @RetentionDays = 30

    Recommended Schedule:
        Create a SQL Agent Job to run weekly (e.g., Sunday 2:00 AM)
        Or use a PowerShell scheduled task if SQL Agent is unavailable
*/

USE [RVToolsDW]
GO

IF OBJECT_ID('dbo.usp_CleanupStaleSessions', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_CleanupStaleSessions]
GO

CREATE PROCEDURE [dbo].[usp_CleanupStaleSessions]
    @RetentionDays INT = 90,
    @DryRun BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@RetentionDays, GETUTCDATE());
    DECLARE @RowsToDelete INT;
    DECLARE @RowsDeleted INT = 0;

    -- Count rows that would be deleted
    SELECT @RowsToDelete = COUNT(*)
    FROM [Web].[Sessions]
    WHERE LoginTime < @CutoffDate;

    IF @DryRun = 0 AND @RowsToDelete > 0
    BEGIN
        -- Actually delete
        DELETE FROM [Web].[Sessions]
        WHERE LoginTime < @CutoffDate;

        SET @RowsDeleted = @@ROWCOUNT;

        -- Log the cleanup
        INSERT INTO [Audit].[ImportLog] (LogLevel, Message)
        VALUES ('Info', 'Cleanup: Deleted ' + CAST(@RowsDeleted AS VARCHAR(20)) +
                ' session records older than ' + CAST(@RetentionDays AS VARCHAR(10)) + ' days');
    END

    -- Return results
    SELECT
        @RowsToDelete AS RowsToDelete,
        @RowsDeleted AS RowsDeleted,
        @CutoffDate AS CutoffDate,
        @RetentionDays AS RetentionDays,
        @DryRun AS DryRun,
        (SELECT COUNT(*) FROM [Web].[Sessions]) AS RemainingRows;
END
GO

PRINT 'Created [dbo].[usp_CleanupStaleSessions]'
GO
