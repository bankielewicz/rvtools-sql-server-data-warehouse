/*
    RVTools Data Warehouse - Host Utilization Trend View

    Purpose: Track host CPU/memory utilization over time
    Source:  History.vHost

    Usage:
        SELECT * FROM [Reporting].[vw_Trends_Host_Utilization]
        WHERE HostName = 'esxi01.example.com'
          AND SnapshotDate >= DATEADD(DAY, -30, GETUTCDATE())
        ORDER BY SnapshotDate DESC
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Trends_Host_Utilization]
AS
SELECT
    CAST(ValidFrom AS DATE) AS SnapshotDate,
    Host AS HostName,
    VI_SDK_Server,
    Datacenter,
    Cluster,

    -- CPU Metrics
    TRY_CAST(CPU_usage_Percent AS DECIMAL(5,2)) AS CPU_Usage_Percent,
    TRY_CAST(Num_CPU AS INT) AS Physical_CPUs,
    TRY_CAST(Cores_per_CPU AS INT) AS Cores_per_CPU,
    TRY_CAST(Num_Cores AS INT) AS Total_Cores,
    TRY_CAST(Num_vCPUs AS INT) AS Total_vCPUs,
    TRY_CAST(vCPUs_per_Core AS DECIMAL(5,2)) AS vCPU_to_Core_Ratio,

    -- Memory Metrics
    TRY_CAST(Memory_usage_Percent AS DECIMAL(5,2)) AS Memory_Usage_Percent,
    TRY_CAST(Num_Memory AS BIGINT) AS Physical_Memory_MiB,
    TRY_CAST(vRAM AS BIGINT) AS Allocated_vMemory_MiB,

    -- VM Counts
    TRY_CAST(Num_VMs AS INT) AS VM_Count,
    TRY_CAST(VMs_per_Core AS DECIMAL(5,2)) AS VMs_per_Core,

    -- Maintenance Mode
    in_Maintenance_Mode,

    -- ESX Version
    ESX_Version,

    -- Audit
    ImportBatchId

FROM [History].[vHost]
WHERE (ValidTo IS NULL  -- Current record for each snapshot
   OR ValidTo > ValidFrom)
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Trends_Host_Utilization]'
GO
