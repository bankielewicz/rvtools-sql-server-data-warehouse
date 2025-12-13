/*
    RVTools Data Warehouse - Storage Growth Projection View

    Purpose: Provide data for linear regression on storage capacity
    Source:  History.vDatastore

    Usage:
        SELECT * FROM [Reporting].[vw_Trends_Storage_Growth]
        WHERE DatastoreName = 'VMFS-Prod-01'
          AND SnapshotDate >= DATEADD(DAY, -90, GETUTCDATE())
        ORDER BY SnapshotDate

    NOTE: Report query performs linear regression calculation
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Trends_Storage_Growth]
AS
SELECT
    CAST(ValidFrom AS DATE) AS SnapshotDate,
    Name AS DatastoreName,
    VI_SDK_Server,
    Type,

    -- Capacity Metrics
    TRY_CAST(Capacity_MiB AS BIGINT) AS Capacity_MiB,
    TRY_CAST(In_Use_MiB AS BIGINT) AS In_Use_MiB,
    TRY_CAST(Free_MiB AS BIGINT) AS Free_MiB,
    TRY_CAST(Free_Percent AS DECIMAL(5,2)) AS Free_Percent,

    -- Day number for regression (days since earliest snapshot)
    DATEDIFF(DAY,
        MIN(CAST(ValidFrom AS DATE)) OVER (PARTITION BY Name, VI_SDK_Server),
        CAST(ValidFrom AS DATE)
    ) AS DayNumber,

    -- Audit
    ImportBatchId

FROM [History].[vDatastore]
WHERE (ValidTo IS NULL  -- Current record for each snapshot
   OR ValidTo > ValidFrom)
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Trends_Storage_Growth]'
GO
