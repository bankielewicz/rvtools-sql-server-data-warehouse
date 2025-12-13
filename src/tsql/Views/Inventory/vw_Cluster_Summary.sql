/*
    RVTools Data Warehouse - Cluster Summary View

    Purpose: High-level cluster configuration and capacity overview
    Source:  Current.vCluster

    Usage:
        SELECT * FROM [Reporting].[vw_Cluster_Summary]
        WHERE HA_enabled = 'true'
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Cluster_Summary]
AS
SELECT
    -- Identity
    Name AS ClusterName,
    VI_SDK_Server,

    -- Status
    Config_status,
    OverallStatus,

    -- Hosts
    NumHosts,
    numEffectiveHosts,

    -- CPU Resources
    TotalCpu,
    NumCpuCores,
    NumCpuThreads,
    Effective_Cpu,

    -- Memory Resources
    TotalMemory,
    Effective_Memory,

    -- HA Configuration
    HA_enabled,
    Failover_Level,
    AdmissionControlEnabled,
    Host_monitoring,
    Isolation_Response,
    Restart_Priority,

    -- VM Monitoring
    VM_Monitoring,
    Max_Failures,
    Max_Failure_Window,
    Failure_Interval,
    Min_Up_Time,

    -- DRS Configuration
    DRS_enabled,
    DRS_default_VM_behavior,
    DRS_vmotion_rate,

    -- DPM Configuration
    DPM_enabled,
    DPM_default_behavior,

    -- Activity
    Num_VMotions,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vCluster]
WHERE ISNULL(IsDeleted, 0) = 0  -- Exclude soft-deleted records
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Cluster_Summary]'
GO
