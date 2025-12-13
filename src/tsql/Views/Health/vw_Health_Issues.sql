/*
    RVTools Data Warehouse - Health Issues View

    Purpose: Active health problems requiring attention
    Source:  Current.vHealth

    Usage:
        SELECT * FROM [Reporting].[vw_Health_Issues]
        ORDER BY Message_type, Name
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Health_Issues]
AS
SELECT
    -- Issue Details
    Name AS ObjectName,
    Message,
    Message_type AS IssueType,

    -- Source
    VI_SDK_Server,

    -- Audit
    ImportBatchId,
    LastModifiedDate AS DetectedDate

FROM [Current].[vHealth]
WHERE ISNULL(IsDeleted, 0) = 0  -- Exclude soft-deleted records
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Health_Issues]'
GO
