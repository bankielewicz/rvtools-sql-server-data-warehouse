/*
    RVTools Data Warehouse - VM Right-Sizing View

    Purpose: Identify over-provisioned VMs for cost optimization
    Source:  Current.vInfo, Current.vCPU, Current.vMemory

    Usage:
        SELECT * FROM [Reporting].[vw_Capacity_VM_RightSizing]
        WHERE Memory_Active_Percent < 50  -- Under-utilized memory
        ORDER BY Memory_Allocated_MiB DESC
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Capacity_VM_RightSizing]
AS
SELECT
    -- VM Identity
    i.VM,
    i.VM_UUID,
    i.VI_SDK_Server,
    i.Powerstate,

    -- Location
    i.Datacenter,
    i.Cluster,
    i.Host,
    i.Resource_pool,

    -- CPU Metrics
    TRY_CAST(c.CPUs AS INT) AS CPU_Allocated,
    TRY_CAST(i.Overall_Cpu_Readiness AS DECIMAL(5,2)) AS CPU_Readiness_Percent,
    TRY_CAST(c.Reservation AS INT) AS CPU_Reservation_MHz,

    -- Memory Metrics
    TRY_CAST(m.Size_MiB AS BIGINT) AS Memory_Allocated_MiB,
    TRY_CAST(m.Active AS BIGINT) AS Memory_Active_MiB,
    TRY_CAST(m.Consumed AS BIGINT) AS Memory_Consumed_MiB,
    TRY_CAST(m.Ballooned AS BIGINT) AS Memory_Ballooned_MiB,
    TRY_CAST(m.Reservation AS BIGINT) AS Memory_Reservation_MiB,

    -- Calculated Ratios
    CASE
        WHEN TRY_CAST(m.Size_MiB AS BIGINT) > 0 THEN
            TRY_CAST((TRY_CAST(m.Active AS DECIMAL(18,2)) * 100.0 / TRY_CAST(m.Size_MiB AS DECIMAL(18,2))) AS DECIMAL(5,2))
        ELSE NULL
    END AS Memory_Active_Percent,

    CASE
        WHEN TRY_CAST(m.Size_MiB AS BIGINT) > 0 THEN
            TRY_CAST((TRY_CAST(m.Reservation AS DECIMAL(18,2)) * 100.0 / TRY_CAST(m.Size_MiB AS DECIMAL(18,2))) AS DECIMAL(5,2))
        ELSE NULL
    END AS Memory_Reservation_Percent,

    -- OS
    i.OS_according_to_the_VMware_Tools,

    -- Audit
    i.ImportBatchId,
    i.LastModifiedDate

FROM [Current].[vInfo] i
LEFT JOIN [Current].[vCPU] c
    ON i.VM_UUID = c.VM_UUID
    AND i.VI_SDK_Server = c.VI_SDK_Server
LEFT JOIN [Current].[vMemory] m
    ON i.VM_UUID = m.VM_UUID
    AND i.VI_SDK_Server = m.VI_SDK_Server
WHERE i.Template = 0  -- Exclude templates
  AND i.Powerstate = 'poweredOn'  -- Only running VMs
  AND ISNULL(i.IsDeleted, 0) = 0  -- Exclude soft-deleted records
GO

PRINT 'Created [Reporting].[vw_Capacity_VM_RightSizing]'
GO
