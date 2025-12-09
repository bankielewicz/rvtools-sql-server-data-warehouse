/*
    RVTools Data Warehouse - Datastore Capacity View

    Purpose: Storage capacity with over-provisioning and status indicators
    Source:  Current.vDatastore

    Usage:
        SELECT * FROM [Reporting].[vw_Datastore_Capacity]
        WHERE CapacityStatus IN ('Warning', 'Critical')
        ORDER BY Free_Percent ASC
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Datastore_Capacity]
AS
SELECT
    -- Identity
    Name AS DatastoreName,
    VI_SDK_Server,

    -- Type
    Type,
    Cluster_name,

    -- Capacity (MiB)
    TRY_CAST(Capacity_MiB AS BIGINT) AS Capacity_MiB,
    TRY_CAST(Provisioned_MiB AS BIGINT) AS Provisioned_MiB,
    TRY_CAST(In_Use_MiB AS BIGINT) AS In_Use_MiB,
    TRY_CAST(Free_MiB AS BIGINT) AS Free_MiB,
    TRY_CAST(Free_Percent AS DECIMAL(5,2)) AS Free_Percent,

    -- Calculated Metrics
    CASE
        WHEN TRY_CAST(Capacity_MiB AS BIGINT) > 0
        THEN CAST(TRY_CAST(Provisioned_MiB AS DECIMAL(18,2)) / TRY_CAST(Capacity_MiB AS DECIMAL(18,2)) * 100 AS DECIMAL(5,2))
        ELSE 0
    END AS OverProvisioningPercent,

    -- Status
    CASE
        WHEN TRY_CAST(Free_Percent AS DECIMAL(5,2)) < 10 THEN 'Critical'
        WHEN TRY_CAST(Free_Percent AS DECIMAL(5,2)) < 20 THEN 'Warning'
        ELSE 'Normal'
    END AS CapacityStatus,

    -- Usage
    Num_VMs,
    Num_Hosts,

    -- Accessibility
    Accessible,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vDatastore]
GO

PRINT 'Created [Reporting].[vw_Datastore_Capacity]'
GO
