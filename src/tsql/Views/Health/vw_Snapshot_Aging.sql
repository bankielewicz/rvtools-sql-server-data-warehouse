/*
    RVTools Data Warehouse - Snapshot Aging View

    Purpose: Identify old snapshots consuming storage
    Source:  Current.vSnapshot

    Usage:
        SELECT * FROM [Reporting].[vw_Snapshot_Aging]
        WHERE AgeDays > 7
        ORDER BY AgeDays DESC
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Snapshot_Aging]
AS
SELECT
    -- VM Identity
    VM,
    VM_UUID,
    Powerstate,

    -- Snapshot Details
    Name AS SnapshotName,
    Description,
    Date_time AS SnapshotDate,
    Filename,

    -- Size (MiB)
    Size_MiB_vmsn,
    Size_MiB_total,

    -- Calculated Age
    DATEDIFF(DAY, TRY_CAST(Date_time AS DATETIME2), GETUTCDATE()) AS AgeDays,

    -- State
    Quiesced,
    State,

    -- Location
    Datacenter,
    Cluster,
    Host,
    Folder,

    -- OS
    OS_according_to_the_VMware_Tools,

    -- Source
    VI_SDK_Server,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vSnapshot]
WHERE ISNULL(IsDeleted, 0) = 0  -- Exclude soft-deleted records
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Snapshot_Aging]'
GO
