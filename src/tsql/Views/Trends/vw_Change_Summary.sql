/*
    RVTools Data Warehouse - Change Summary View

    Purpose: Track VMs created and deleted over time periods
    Source:  History.vInfo (SCD Type 2)

    Usage:
        -- Get changes for last 30 days
        SELECT * FROM [Reporting].[vw_Change_Summary]
        WHERE ChangeDate >= DATEADD(DAY, -30, GETDATE())
        ORDER BY ChangeDate DESC

    Notes:
        - 'Created' = First appearance in History (no prior ValidFrom)
        - 'Deleted' = ValidTo is set (record superseded and not current)
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Change_Summary]
AS
WITH VMCreations AS (
    -- VMs that first appeared (created)
    SELECT
        h.VM,
        h.VM_UUID,
        h.Powerstate,
        h.Host,
        h.Cluster,
        h.Datacenter,
        h.VI_SDK_Server,
        CAST(h.ValidFrom AS DATE) AS ChangeDate,
        'Created' AS ChangeType,
        h.CPUs,
        h.Memory,
        h.Provisioned_MiB
    FROM [History].[vInfo] h
    WHERE NOT EXISTS (
        SELECT 1 FROM [History].[vInfo] h2
        WHERE h2.VM_UUID = h.VM_UUID
          AND h2.VI_SDK_Server = h.VI_SDK_Server
          AND h2.ValidFrom < h.ValidFrom
    )
    AND h.Template = 0  -- Exclude templates
    AND h.VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
),
VMDeletions AS (
    -- VMs that were deleted (have ValidTo and no newer record)
    SELECT
        h.VM,
        h.VM_UUID,
        h.Powerstate,
        h.Host,
        h.Cluster,
        h.Datacenter,
        h.VI_SDK_Server,
        CAST(h.ValidTo AS DATE) AS ChangeDate,
        'Deleted' AS ChangeType,
        h.CPUs,
        h.Memory,
        h.Provisioned_MiB
    FROM [History].[vInfo] h
    WHERE h.ValidTo IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM [History].[vInfo] h2
          WHERE h2.VM_UUID = h.VM_UUID
            AND h2.VI_SDK_Server = h.VI_SDK_Server
            AND h2.ValidFrom > h.ValidTo
      )
      AND h.Template = 0  -- Exclude templates
      AND h.VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
)
SELECT * FROM VMCreations
UNION ALL
SELECT * FROM VMDeletions;
GO

PRINT 'Created [Reporting].[vw_Change_Summary]'
GO
