/*
    RVTools Data Warehouse - vCenter Status View

    Purpose: Show status and last import date for each vCenter
    Source:  Audit.ImportBatch + Config.ActiveVCenters

    Usage:
        SELECT * FROM [Reporting].[vw_VCenter_Status]
        ORDER BY LastImportDate DESC
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_VCenter_Status]
AS
SELECT
    COALESCE(a.VIServer, b.VIServer) AS VI_SDK_Server,
    COALESCE(a.IsActive, 1) AS IsActive,
    b.LastImportDate,
    b.TotalImports,
    b.FirstImportDate,
    DATEDIFF(DAY, b.LastImportDate, GETDATE()) AS DaysSinceLastImport,
    CASE
        WHEN b.LastImportDate IS NULL THEN 'Never'
        WHEN DATEDIFF(DAY, b.LastImportDate, GETDATE()) <= 1 THEN 'Current'
        WHEN DATEDIFF(DAY, b.LastImportDate, GETDATE()) <= 7 THEN 'Recent'
        WHEN DATEDIFF(DAY, b.LastImportDate, GETDATE()) <= 30 THEN 'Stale'
        ELSE 'Inactive'
    END AS ImportStatus,
    a.Notes,
    -- VM counts from Current schema
    (SELECT COUNT(*) FROM [Current].[vInfo] v
     WHERE v.VI_SDK_Server = COALESCE(a.VIServer, b.VIServer)
       AND ISNULL(v.IsDeleted, 0) = 0) AS VMCount,
    (SELECT COUNT(*) FROM [Current].[vHost] h
     WHERE h.VI_SDK_Server = COALESCE(a.VIServer, b.VIServer)
       AND ISNULL(h.IsDeleted, 0) = 0) AS HostCount
FROM [Config].[ActiveVCenters] a
FULL OUTER JOIN (
    SELECT
        VIServer,
        MAX(ImportStartTime) AS LastImportDate,
        MIN(ImportStartTime) AS FirstImportDate,
        COUNT(*) AS TotalImports
    FROM [Audit].[ImportBatch]
    WHERE Status = 'Completed'
    GROUP BY VIServer
) b ON a.VIServer = b.VIServer;
GO

PRINT 'Created [Reporting].[vw_VCenter_Status]'
GO
