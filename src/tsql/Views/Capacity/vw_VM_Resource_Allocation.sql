/*
    RVTools Data Warehouse - VM Resource Allocation View

    Purpose: VM CPU and Memory allocation with usage metrics for right-sizing
    Source:  Current.vInfo, Current.vCPU, Current.vMemory

    Usage:
        SELECT * FROM [Reporting].[vw_VM_Resource_Allocation]
        WHERE Powerstate = 'poweredOn'
        ORDER BY Memory_Size_MiB DESC
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_VM_Resource_Allocation]
AS
SELECT
    -- VM Identity
    i.VM,
    i.VM_UUID,
    i.VI_SDK_Server,
    i.Powerstate,
    i.Template,

    -- Location
    i.Datacenter,
    i.Cluster,
    i.Host,

    -- CPU Allocation (from vCPU)
    TRY_CAST(c.CPUs AS INT) AS CPU_Count,
    TRY_CAST(c.Sockets AS INT) AS CPU_Sockets,
    TRY_CAST(c.Cores_per_socket AS INT) AS Cores_Per_Socket,
    TRY_CAST(c.Overall AS INT) AS CPU_Overall_MHz,
    TRY_CAST(c.Reservation AS INT) AS CPU_Reservation_MHz,
    TRY_CAST(c.Limit AS INT) AS CPU_Limit_MHz,
    c.Level AS CPU_Shares_Level,
    c.Hot_Add AS CPU_Hot_Add,

    -- Memory Allocation (from vMemory)
    TRY_CAST(m.Size_MiB AS BIGINT) AS Memory_Size_MiB,
    TRY_CAST(m.Consumed AS BIGINT) AS Memory_Consumed_MiB,
    TRY_CAST(m.Active AS BIGINT) AS Memory_Active_MiB,
    TRY_CAST(m.Ballooned AS BIGINT) AS Memory_Ballooned_MiB,
    TRY_CAST(m.Swapped AS BIGINT) AS Memory_Swapped_MiB,
    TRY_CAST(m.Reservation AS BIGINT) AS Memory_Reservation_MiB,
    TRY_CAST(m.Limit AS BIGINT) AS Memory_Limit_MiB,
    m.Level AS Memory_Shares_Level,
    m.Hot_Add AS Memory_Hot_Add,

    -- OS Info
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
WHERE ISNULL(i.IsDeleted, 0) = 0  -- Exclude soft-deleted records
  AND i.VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_VM_Resource_Allocation]'
GO
