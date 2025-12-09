/*
    RVTools Data Warehouse - VM Inventory View

    Purpose: Complete listing of all virtual machines with key specifications
    Source:  Current.vInfo

    Usage:
        SELECT * FROM [Reporting].[vw_VM_Inventory]
        WHERE Datacenter = 'Production'
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_VM_Inventory]
AS
SELECT
    -- Identity
    VM,
    VM_UUID,
    VI_SDK_Server,

    -- State
    Powerstate,
    Template,
    Config_status,
    Guest_state,

    -- Resources
    CPUs,
    Memory,
    NICs,
    Disks,

    -- Storage (MiB)
    Total_disk_capacity_MiB,
    Provisioned_MiB,
    In_Use_MiB,

    -- Network
    Primary_IP_Address,
    DNS_Name,

    -- Location
    Datacenter,
    Cluster,
    Host,
    Folder,
    Resource_pool,

    -- OS
    OS_according_to_the_VMware_Tools,
    OS_according_to_the_configuration_file,

    -- Hardware
    HW_version,
    Firmware,

    -- Dates
    Creation_date,
    PowerOn,

    -- Metadata
    Annotation,
    Path,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vInfo]
GO

PRINT 'Created [Reporting].[vw_VM_Inventory]'
GO
