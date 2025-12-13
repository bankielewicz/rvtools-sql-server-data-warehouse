/*
    RVTools Data Warehouse - Orphaned Files View

    Purpose: Identify VMDK files not linked to registered VMs
    Source:  Current.vFileInfo, Current.vInfo

    Usage:
        SELECT * FROM [Reporting].[vw_Health_Orphaned_Files]
        WHERE IsOrphaned = 1
        ORDER BY File_Size_GiB DESC

    NOTE: Requires RVTools to be run with -GetFileInfo flag
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Health_Orphaned_Files]
AS
SELECT
    -- File Details
    f.File_Name,
    f.Friendly_Path_Name,
    f.Path,
    f.File_Type,
    TRY_CAST(f.File_Size_in_bytes AS BIGINT) AS File_Size_Bytes,
    TRY_CAST(TRY_CAST(f.File_Size_in_bytes AS BIGINT) / 1024.0 / 1024.0 / 1024.0 AS DECIMAL(10,2)) AS File_Size_GiB,

    -- Datastore extracted from Path (format: [DatastoreName] folder/file.vmdk)
    CASE
        WHEN f.Path LIKE '[%]%' THEN
            SUBSTRING(f.Path, 2, CHARINDEX(']', f.Path) - 2)
        ELSE NULL
    END AS Datastore,

    -- Orphan Detection (VMDK not in any registered VM's path)
    CASE
        WHEN f.File_Type = 'VMDK' AND NOT EXISTS (
            SELECT 1 FROM [Current].[vInfo] i
            WHERE f.Path LIKE i.Path + '%'
              AND f.VI_SDK_Server = i.VI_SDK_Server
        ) THEN 1
        ELSE 0
    END AS IsOrphaned,

    -- Source
    f.VI_SDK_Server,

    -- Audit
    f.ImportBatchId,
    f.LastModifiedDate

FROM [Current].[vFileInfo] f
WHERE f.File_Type IN ('VMDK', 'VMSD', 'VMSN')  -- Disk, snapshot descriptor, snapshot data
  AND ISNULL(f.IsDeleted, 0) = 0  -- Exclude soft-deleted records
  AND f.VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Health_Orphaned_Files]'
GO
