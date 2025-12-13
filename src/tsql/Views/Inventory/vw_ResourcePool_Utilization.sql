/*
    RVTools Data Warehouse - Resource Pool Utilization View

    Purpose: Aggregate VM resources by resource pool across vCenters
    Source:  Current.vInfo, Current.vCPU, Current.vMemory

    Usage:
        SELECT * FROM [Reporting].[vw_MultiVCenter_ResourcePool_Utilization]
        WHERE VI_SDK_Server = 'vcenter1.example.com'
        ORDER BY Total_Memory_MiB DESC

    NOTE: No vResourcePool table exists - uses Current.vInfo.Resource_pool column
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_MultiVCenter_ResourcePool_Utilization]
AS
SELECT
    -- Resource Pool Identity
    COALESCE(i.Resource_pool, 'Resources') AS ResourcePool,
    i.VI_SDK_Server,
    i.Datacenter,
    i.Cluster,

    -- VM Counts
    COUNT(CASE WHEN i.Template = 0 AND i.Powerstate = 'poweredOn' THEN 1 END) AS VMs_PoweredOn,
    COUNT(CASE WHEN i.Template = 0 THEN 1 END) AS Total_VMs,

    -- CPU Allocation
    SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(c.CPUs AS INT), 0) ELSE 0 END) AS Total_vCPUs,
    SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(c.Reservation AS BIGINT), 0) ELSE 0 END) AS Total_CPU_Reservation_MHz,
    SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(c.[Limit] AS BIGINT), 0) ELSE 0 END) AS Total_CPU_Limit_MHz,

    -- Memory Allocation
    SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(m.Size_MiB AS BIGINT), 0) ELSE 0 END) AS Total_Memory_MiB,
    SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(m.Active AS BIGINT), 0) ELSE 0 END) AS Total_Active_Memory_MiB,
    SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(m.Reservation AS BIGINT), 0) ELSE 0 END) AS Total_Memory_Reservation_MiB,
    SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(m.[Limit] AS BIGINT), 0) ELSE 0 END) AS Total_Memory_Limit_MiB,

    -- Average Active Memory Percent
    CASE
        WHEN SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(m.Size_MiB AS BIGINT), 0) ELSE 0 END) > 0 THEN
            TRY_CAST(
                SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(m.Active AS BIGINT), 0) ELSE 0 END) * 100.0 /
                SUM(CASE WHEN i.Powerstate = 'poweredOn' THEN COALESCE(TRY_CAST(m.Size_MiB AS BIGINT), 0) ELSE 0 END)
                AS DECIMAL(5,2)
            )
        ELSE NULL
    END AS Avg_Memory_Active_Percent,

    -- Audit
    MAX(i.ImportBatchId) AS ImportBatchId,
    MAX(i.LastModifiedDate) AS LastModifiedDate

FROM [Current].[vInfo] i
LEFT JOIN [Current].[vCPU] c
    ON i.VM_UUID = c.VM_UUID
    AND i.VI_SDK_Server = c.VI_SDK_Server
LEFT JOIN [Current].[vMemory] m
    ON i.VM_UUID = m.VM_UUID
    AND i.VI_SDK_Server = m.VI_SDK_Server
WHERE i.Template = 0
  AND ISNULL(i.IsDeleted, 0) = 0  -- Exclude soft-deleted records
  AND i.VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GROUP BY i.Resource_pool, i.VI_SDK_Server, i.Datacenter, i.Cluster
GO

PRINT 'Created [Reporting].[vw_MultiVCenter_ResourcePool_Utilization]'
GO
