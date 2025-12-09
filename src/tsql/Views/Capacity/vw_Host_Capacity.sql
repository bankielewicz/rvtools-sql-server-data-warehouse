/*
    RVTools Data Warehouse - Host Capacity View

    Purpose: Host resource utilization with status indicators
    Source:  Current.vHost

    Usage:
        SELECT * FROM [Reporting].[vw_Host_Capacity]
        WHERE CPUStatus = 'Critical' OR MemoryStatus = 'Critical'
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Host_Capacity]
AS
SELECT
    -- Identity
    Host,
    VI_SDK_Server,

    -- Location
    Datacenter,
    Cluster,

    -- CPU Capacity
    Num_Cores,
    TRY_CAST(CPU_usage_Percent AS DECIMAL(5,2)) AS CPU_Usage_Percent,
    CASE
        WHEN TRY_CAST(CPU_usage_Percent AS DECIMAL(5,2)) >= 85 THEN 'Critical'
        WHEN TRY_CAST(CPU_usage_Percent AS DECIMAL(5,2)) >= 70 THEN 'Warning'
        ELSE 'Normal'
    END AS CPUStatus,

    -- Memory Capacity
    Num_Memory AS Memory_MB,
    TRY_CAST(Memory_usage_Percent AS DECIMAL(5,2)) AS Memory_Usage_Percent,
    CASE
        WHEN TRY_CAST(Memory_usage_Percent AS DECIMAL(5,2)) >= 85 THEN 'Critical'
        WHEN TRY_CAST(Memory_usage_Percent AS DECIMAL(5,2)) >= 70 THEN 'Warning'
        ELSE 'Normal'
    END AS MemoryStatus,

    -- VM Load
    Num_VMs,
    Num_vCPUs,
    TRY_CAST(vCPUs_per_Core AS DECIMAL(5,2)) AS vCPUs_per_Core,
    TRY_CAST(vRAM AS BIGINT) AS vRAM_MB,

    -- Status
    in_Maintenance_Mode,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vHost]
GO

PRINT 'Created [Reporting].[vw_Host_Capacity]'
GO
