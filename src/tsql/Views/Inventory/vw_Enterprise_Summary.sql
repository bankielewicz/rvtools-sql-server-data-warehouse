/*
    RVTools Data Warehouse - Enterprise Multi-vCenter Summary View

    Purpose: Aggregate VM counts and resources across all vCenters
    Source:  Current.vInfo

    Usage:
        SELECT * FROM [Reporting].[vw_MultiVCenter_Enterprise_Summary]
        ORDER BY VI_SDK_Server

    NOTE: Report query will join this with Host and Datastore summaries
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_MultiVCenter_Enterprise_Summary]
AS
SELECT
    -- vCenter Identity
    COALESCE(VI_SDK_Server, 'Unknown') AS VI_SDK_Server,

    -- VM Counts
    SUM(CASE WHEN Template = 0 AND Powerstate = 'poweredOn' THEN 1 ELSE 0 END) AS VMs_PoweredOn,
    SUM(CASE WHEN Template = 0 AND Powerstate = 'poweredOff' THEN 1 ELSE 0 END) AS VMs_PoweredOff,
    SUM(CASE WHEN Template = 1 THEN 1 ELSE 0 END) AS Templates,
    SUM(CASE WHEN Template = 0 THEN 1 ELSE 0 END) AS Total_VMs,

    -- CPU Allocation (only powered-on VMs)
    SUM(CASE WHEN Template = 0 AND Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(CPUs AS INT), 0) ELSE 0 END) AS Total_vCPUs,

    -- Memory Allocation (only powered-on VMs)
    SUM(CASE WHEN Template = 0 AND Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(Memory AS BIGINT), 0) ELSE 0 END) AS Total_vMemory_MiB,

    -- Storage Allocation
    SUM(COALESCE(TRY_CAST(Provisioned_MiB AS BIGINT), 0)) AS Total_Provisioned_MiB,
    SUM(COALESCE(TRY_CAST(In_Use_MiB AS BIGINT), 0)) AS Total_InUse_MiB,

    -- Cluster Counts
    COUNT(DISTINCT Cluster) AS Cluster_Count,

    -- Host Counts
    COUNT(DISTINCT Host) AS Host_Count,

    -- Datacenter Counts
    COUNT(DISTINCT Datacenter) AS Datacenter_Count,

    -- Audit
    MAX(ImportBatchId) AS Latest_ImportBatchId,
    MAX(LastModifiedDate) AS Latest_Import_Date

FROM [Current].[vInfo]
WHERE ISNULL(IsDeleted, 0) = 0  -- Exclude soft-deleted records
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GROUP BY VI_SDK_Server
GO

PRINT 'Created [Reporting].[vw_MultiVCenter_Enterprise_Summary]'
GO
