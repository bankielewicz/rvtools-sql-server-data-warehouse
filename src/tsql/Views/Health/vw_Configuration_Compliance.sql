/*
    RVTools Data Warehouse - Configuration Compliance View

    Purpose: Validate VMs against 4 compliance standards:
             1. vCPU:Core ratio <= 4:1
             2. Memory reservation >= 50%
             3. Boot delay >= 10 seconds
             4. VMware Tools current
    Source:  Current.vInfo, Current.vCPU, Current.vMemory, Current.vHost, Current.vTools

    Usage:
        SELECT * FROM [Reporting].[vw_Health_Configuration_Compliance]
        WHERE Overall_Compliance_Status = 'Non-Compliant'
        ORDER BY VM
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Health_Configuration_Compliance]
AS
WITH HostCores AS (
    SELECT
        VI_SDK_Server,
        Host,
        TRY_CAST(Num_Cores AS INT) AS Num_Cores
    FROM [Current].[vHost]
)
SELECT
    -- VM Identity
    i.VM,
    i.VM_UUID,
    i.VI_SDK_Server,
    i.Powerstate,
    i.Datacenter,
    i.Cluster,
    i.Host,
    i.Resource_pool,

    -- OS (for production determination - filter in report)
    i.OS_according_to_the_VMware_Tools,
    i.Annotation,

    -- CPU Metrics
    TRY_CAST(c.CPUs AS INT) AS CPU_Count,
    h.Num_Cores AS Host_Physical_Cores,
    CASE
        WHEN h.Num_Cores > 0 THEN
            TRY_CAST(TRY_CAST(c.CPUs AS DECIMAL(10,2)) / h.Num_Cores AS DECIMAL(5,2))
        ELSE NULL
    END AS vCPU_to_Core_Ratio,

    -- Memory Metrics
    TRY_CAST(m.Size_MiB AS BIGINT) AS Memory_Allocated_MiB,
    TRY_CAST(m.Reservation AS BIGINT) AS Memory_Reservation_MiB,
    CASE
        WHEN TRY_CAST(m.Size_MiB AS BIGINT) > 0 THEN
            TRY_CAST(TRY_CAST(m.Reservation AS DECIMAL(18,2)) * 100.0 / TRY_CAST(m.Size_MiB AS DECIMAL(18,2)) AS DECIMAL(5,2))
        ELSE NULL
    END AS Memory_Reservation_Percent,

    -- Boot Settings
    TRY_CAST(i.Boot_delay AS INT) AS Boot_Delay_Seconds,

    -- Tools Status
    t.Tools AS Tools_Status,
    t.Tools_Version,
    t.Required_Version,
    t.Upgradeable AS Tools_Upgradeable,

    -- Compliance Checks
    CASE
        WHEN h.Num_Cores > 0 AND TRY_CAST(c.CPUs AS DECIMAL(10,2)) / h.Num_Cores <= 4 THEN 1
        ELSE 0
    END AS vCPU_Ratio_Compliant,

    CASE
        WHEN TRY_CAST(m.Size_MiB AS BIGINT) > 0 AND (TRY_CAST(m.Reservation AS DECIMAL(18,2)) * 100.0 / TRY_CAST(m.Size_MiB AS DECIMAL(18,2))) >= 50 THEN 1
        ELSE 0
    END AS Memory_Reservation_Compliant,

    CASE
        WHEN TRY_CAST(i.Boot_delay AS INT) >= 10 THEN 1
        ELSE 0
    END AS Boot_Delay_Compliant,

    CASE
        WHEN t.Tools IN ('toolsOk', 'toolsOld') AND t.Upgradeable = '0' THEN 1
        WHEN t.Tools = 'toolsNotRunning' THEN 0
        WHEN t.Upgradeable = '1' THEN 0
        ELSE 0
    END AS Tools_Compliant,

    -- Overall Compliance
    CASE
        WHEN (
            (h.Num_Cores > 0 AND TRY_CAST(c.CPUs AS DECIMAL(10,2)) / h.Num_Cores <= 4) AND
            (TRY_CAST(m.Size_MiB AS BIGINT) > 0 AND (TRY_CAST(m.Reservation AS DECIMAL(18,2)) * 100.0 / TRY_CAST(m.Size_MiB AS DECIMAL(18,2))) >= 50) AND
            (TRY_CAST(i.Boot_delay AS INT) >= 10) AND
            (t.Tools IN ('toolsOk', 'toolsOld') AND t.Upgradeable = '0')
        ) THEN 'Compliant'
        ELSE 'Non-Compliant'
    END AS Overall_Compliance_Status,

    -- Audit
    i.ImportBatchId,
    i.LastModifiedDate

FROM [Current].[vInfo] i
LEFT JOIN [Current].[vCPU] c
    ON i.VM_UUID = c.VM_UUID
    AND i.VI_SDK_Server = c.VI_SDK_Server
LEFT JOIN [Current].[vMemory] m
    ON i.VM_UUID = m.VM_UUID
    AND i.VI_SDK_Server = m.VI_SDK_Server
LEFT JOIN HostCores h
    ON i.Host = h.Host
    AND i.VI_SDK_Server = h.VI_SDK_Server
LEFT JOIN [Current].[vTools] t
    ON i.VM_UUID = t.VM_UUID
    AND i.VI_SDK_Server = t.VI_SDK_Server
WHERE i.Template = 0
GO

PRINT 'Created [Reporting].[vw_Health_Configuration_Compliance]'
GO
