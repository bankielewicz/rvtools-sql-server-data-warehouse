/*
    RVTools Data Warehouse - License Compliance View

    Purpose: Track license usage vs allocation with expiration warnings
    Source:  Current.vLicense

    Usage:
        SELECT * FROM [Reporting].[vw_Inventory_License_Compliance]
        WHERE Compliance_Status != 'Compliant'
        ORDER BY Days_Until_Expiration
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Inventory_License_Compliance]
AS
SELECT
    -- License Details
    Name AS LicenseName,
    [Key] AS LicenseKey,
    Labels,
    Cost_Unit,

    -- Usage
    Total AS Total_Licenses,
    Used AS Used_Licenses,
    Total - Used AS Available_Licenses,
    CASE
        WHEN Total > 0 THEN
            TRY_CAST((Used * 100.0 / Total) AS DECIMAL(5,2))
        ELSE NULL
    END AS Usage_Percent,

    -- Expiration
    Expiration_Date,
    CASE
        WHEN Expiration_Date IS NOT NULL THEN
            DATEDIFF(DAY, GETUTCDATE(), Expiration_Date)
        ELSE NULL
    END AS Days_Until_Expiration,

    -- Compliance Status
    CASE
        WHEN Used > Total THEN 'Over-Allocated'
        WHEN Expiration_Date < GETUTCDATE() THEN 'Expired'
        WHEN Expiration_Date < DATEADD(DAY, 30, GETUTCDATE()) THEN 'Expiring Soon'
        WHEN TRY_CAST((Used * 100.0 / NULLIF(Total, 0)) AS DECIMAL(5,2)) > 90 THEN 'Near Capacity'
        ELSE 'Compliant'
    END AS Compliance_Status,

    -- Features
    Features,

    -- Source
    VI_SDK_Server,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vLicense]
GO

PRINT 'Created [Reporting].[vw_Inventory_License_Compliance]'
GO
