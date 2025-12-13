/*
    RVTools Data Warehouse - VM Lifecycle Analysis View

    Purpose: Track VM power state changes and uptime over time
    Source:  History.vInfo

    Usage:
        SELECT * FROM [Reporting].[vw_Trends_VM_Lifecycle]
        WHERE State_Start_Date >= DATEADD(DAY, -90, GETUTCDATE())
        ORDER BY VM, State_Start_Date DESC

    NOTE: Report query aggregates this data to calculate uptime %
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Trends_VM_Lifecycle]
AS
SELECT
    VM,
    VM_UUID,
    VI_SDK_Server,
    Datacenter,
    Cluster,
    Host,
    Resource_pool,

    -- State Information
    Powerstate,
    CAST(ValidFrom AS DATE) AS State_Start_Date,
    CAST(COALESCE(ValidTo, GETUTCDATE()) AS DATE) AS State_End_Date,

    -- Duration in this state (days)
    DATEDIFF(DAY,
        CAST(ValidFrom AS DATE),
        CAST(COALESCE(ValidTo, GETUTCDATE()) AS DATE)
    ) AS Days_In_State,

    -- PowerOn timestamp
    TRY_CAST(PowerOn AS DATETIME) AS Last_PowerOn_Time,

    -- Template flag
    Template,

    -- OS
    OS_according_to_the_VMware_Tools,

    -- Audit
    ImportBatchId,
    ValidFrom,
    ValidTo

FROM [History].[vInfo]
WHERE Template = 0  -- Exclude templates
  AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
GO

PRINT 'Created [Reporting].[vw_Trends_VM_Lifecycle]'
GO
