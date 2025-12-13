/*
    RVTools Data Warehouse - VM Count Trend View

    Purpose: Track VM count over time for growth analysis
    Source:  History.vInfo

    Usage:
        SELECT * FROM [Reporting].[vw_VM_Count_Trend]
        ORDER BY SnapshotDate DESC, VI_SDK_Server
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_VM_Count_Trend]
AS
SELECT
    CAST(ValidFrom AS DATE) AS SnapshotDate,
    VI_SDK_Server,
    COUNT(DISTINCT VM_UUID) AS VMCount,
    SUM(CASE WHEN Template = 'True' THEN 1 ELSE 0 END) AS TemplateCount,
    SUM(CASE WHEN Powerstate = 'poweredOn' THEN 1 ELSE 0 END) AS PoweredOnCount,
    SUM(CASE WHEN Powerstate = 'poweredOff' THEN 1 ELSE 0 END) AS PoweredOffCount,
    SUM(CASE WHEN Powerstate = 'suspended' THEN 1 ELSE 0 END) AS SuspendedCount,
    MIN(ImportBatchId) AS ImportBatchId

FROM [History].[vInfo]
WHERE (ValidTo IS NULL  -- Current records only (latest snapshot per day)
   OR ValidTo > ValidFrom)  -- Include historical records
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GROUP BY CAST(ValidFrom AS DATE), VI_SDK_Server
GO

PRINT 'Created [Reporting].[vw_VM_Count_Trend]'
GO
