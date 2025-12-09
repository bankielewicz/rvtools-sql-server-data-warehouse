/*
    RVTools Data Warehouse - VM Configuration Changes View

    Purpose: Track configuration drift and changes over time
    Source:  History.vInfo

    Usage:
        SELECT * FROM [Reporting].[vw_VM_Config_Changes]
        WHERE ChangedDate >= DATEADD(DAY, -30, GETUTCDATE())
        ORDER BY ChangedDate DESC
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_VM_Config_Changes]
AS
SELECT
    -- VM Identity
    VM,
    VM_UUID,
    VI_SDK_Server,

    -- Change Timestamps
    ValidFrom AS EffectiveFrom,
    ValidTo AS EffectiveUntil,
    ValidTo AS ChangedDate,

    -- Key Configuration (at time of change)
    Powerstate,
    TRY_CAST(CPUs AS INT) AS CPUs,
    TRY_CAST(Memory AS BIGINT) AS Memory_MB,
    TRY_CAST(NICs AS INT) AS NICs,
    TRY_CAST(Disks AS INT) AS Disks,

    -- Location (at time of change)
    Datacenter,
    Cluster,
    Host,

    -- Hardware
    HW_version,

    -- OS
    OS_according_to_the_VMware_Tools,

    -- Source
    SourceFile,
    ImportBatchId

FROM [History].[vInfo]
WHERE ValidTo IS NOT NULL  -- Only superseded records (changed or deleted)
GO

PRINT 'Created [Reporting].[vw_VM_Config_Changes]'
GO
