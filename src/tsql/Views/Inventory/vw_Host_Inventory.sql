/*
    RVTools Data Warehouse - Host Inventory View

    Purpose: Complete listing of ESXi hosts with specifications
    Source:  Current.vHost

    Usage:
        SELECT * FROM [Reporting].[vw_Host_Inventory]
        WHERE Cluster = 'Production-Cluster'
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Host_Inventory]
AS
SELECT
    -- Identity
    Host,
    UUID,
    VI_SDK_Server,

    -- Location
    Datacenter,
    Cluster,

    -- Status
    Config_status,
    in_Maintenance_Mode,
    in_Quarantine_Mode,

    -- CPU
    CPU_Model,
    Speed,
    Num_CPU,
    Cores_per_CPU,
    Num_Cores,
    HT_Available,
    HT_Active,

    -- Memory
    Num_Memory,

    -- Adapters
    Num_NICs,
    Num_HBAs,

    -- VMs
    Num_VMs,
    Num_vCPUs,
    vCPUs_per_Core,
    vRAM,

    -- Software
    ESX_Version,
    Current_EVC,
    Max_EVC,

    -- Hardware
    Vendor,
    Model,
    Serial_number,
    Service_tag,
    BIOS_Version,
    -- Handle Excel OLE Automation dates (serial numbers like 43929.8333333333)
    CASE
        WHEN BIOS_Date IS NULL OR BIOS_Date = '' THEN NULL
        WHEN TRY_CAST(BIOS_Date AS FLOAT) IS NOT NULL
            THEN DATEADD(DAY, TRY_CAST(BIOS_Date AS FLOAT), '1899-12-30')
        ELSE TRY_CAST(BIOS_Date AS DATETIME2)
    END AS BIOS_Date,

    -- Time
    Boot_time,
    Time_Zone_Name,

    -- Certificate
    Certificate_Expiry_Date,
    Certificate_Status,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vHost]
WHERE ISNULL(IsDeleted, 0) = 0  -- Exclude soft-deleted records
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Host_Inventory]'
GO
