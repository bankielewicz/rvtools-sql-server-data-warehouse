/*
    RVTools Data Warehouse - Datastore Capacity Trend View

    Purpose: Track storage capacity over time for planning
    Source:  History.vDatastore

    Usage:
        SELECT * FROM [Reporting].[vw_Datastore_Capacity_Trend]
        WHERE DatastoreName = 'VMFS-Prod-01'
        ORDER BY SnapshotDate DESC
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Datastore_Capacity_Trend]
AS
SELECT
    CAST(ValidFrom AS DATE) AS SnapshotDate,
    Name AS DatastoreName,
    VI_SDK_Server,
    Type,

    -- Capacity Metrics
    TRY_CAST(Capacity_MiB AS BIGINT) AS Capacity_MiB,
    TRY_CAST(Provisioned_MiB AS BIGINT) AS Provisioned_MiB,
    TRY_CAST(In_Use_MiB AS BIGINT) AS In_Use_MiB,
    TRY_CAST(Free_MiB AS BIGINT) AS Free_MiB,
    TRY_CAST(Free_Percent AS DECIMAL(5,2)) AS Free_Percent,

    -- VM Count
    TRY_CAST(Num_VMs AS INT) AS Num_VMs,

    -- Audit
    ImportBatchId

FROM [History].[vDatastore]
WHERE (ValidTo IS NULL  -- Current record for each snapshot
   OR ValidTo > ValidFrom)
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Datastore_Capacity_Trend]'
GO
