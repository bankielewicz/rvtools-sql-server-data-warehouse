/*
    Sync Active vCenters from Import History

    Purpose: Auto-populate Config.ActiveVCenters from Audit.ImportBatch
    Run after imports or manually to discover new vCenters
*/

USE [RVToolsDW]
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_SyncActiveVCenters]
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert new vCenters discovered in imports
    -- Status can be 'Success', 'Completed', or 'Partial' depending on import method
    INSERT INTO [Config].[ActiveVCenters] (VIServer, IsActive, LastImportDate, TotalImports)
    SELECT
        VIServer,
        1,  -- Default to active
        MAX(ImportStartTime),
        COUNT(*)
    FROM [Audit].[ImportBatch]
    WHERE Status IN ('Success', 'Completed', 'Partial', 'PartialSuccess')
      AND VIServer IS NOT NULL
      AND VIServer NOT IN (SELECT VIServer FROM [Config].[ActiveVCenters])
    GROUP BY VIServer;

    -- Update existing vCenters with latest import info
    UPDATE a
    SET
        a.LastImportDate = b.LastImportDate,
        a.TotalImports = b.TotalImports,
        a.ModifiedDate = SYSUTCDATETIME()
    FROM [Config].[ActiveVCenters] a
    INNER JOIN (
        SELECT VIServer, MAX(ImportStartTime) AS LastImportDate, COUNT(*) AS TotalImports
        FROM [Audit].[ImportBatch]
        WHERE Status IN ('Success', 'Completed', 'Partial', 'PartialSuccess')
        GROUP BY VIServer
    ) b ON a.VIServer = b.VIServer;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'Created [dbo].[usp_SyncActiveVCenters]'
GO
