/*
    RVTools Data Warehouse - VMware Tools Status View

    Purpose: VMware Tools version compliance tracking
    Source:  Current.vTools

    Usage:
        SELECT * FROM [Reporting].[vw_Tools_Status]
        WHERE Upgradeable = 'true'
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Tools_Status]
AS
SELECT
    -- VM Identity
    VM,
    VM_UUID,
    Powerstate,
    Template,

    -- Tools Status
    Tools AS ToolsStatus,
    Tools_Version,
    Required_Version,
    Upgradeable,
    Upgrade_Policy,

    -- Additional Status
    App_status,
    Heartbeat_status,
    Operation_Ready,
    State_change_support,
    Interactive_Guest,

    -- Location
    Datacenter,
    Cluster,
    Host,
    Folder,

    -- OS
    OS_according_to_the_VMware_Tools,
    OS_according_to_the_configuration_file,

    -- Source
    VI_SDK_Server,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vTools]
WHERE ISNULL(IsDeleted, 0) = 0  -- Exclude soft-deleted records
GO

PRINT 'Created [Reporting].[vw_Tools_Status]'
GO
