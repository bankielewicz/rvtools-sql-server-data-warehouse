/*
    RVTools Data Warehouse - Certificate Expiration View

    Purpose: Track ESXi host SSL certificate expiration dates
    Source:  Current.vHost

    Usage:
        SELECT * FROM [Reporting].[vw_Health_Certificate_Expiration]
        WHERE Expiration_Status = 'Expiring Soon'
        ORDER BY Days_Until_Expiration
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Health_Certificate_Expiration]
AS
SELECT
    -- Host Identity
    Host AS HostName,
    VI_SDK_Server,
    Datacenter,
    Cluster,

    -- Certificate Details
    Certificate_Issuer,
    Certificate_Subject,
    Certificate_Status,
    Certificate_Start_Date,
    Certificate_Expiry_Date,

    -- Expiration Calculation
    CASE
        WHEN Certificate_Expiry_Date IS NOT NULL THEN
            DATEDIFF(DAY, GETUTCDATE(), Certificate_Expiry_Date)
        ELSE NULL
    END AS Days_Until_Expiration,

    -- Health Status
    CASE
        WHEN Certificate_Expiry_Date IS NULL THEN 'Unknown'
        WHEN Certificate_Expiry_Date < GETUTCDATE() THEN 'Expired'
        WHEN Certificate_Expiry_Date < DATEADD(DAY, 30, GETUTCDATE()) THEN 'Expiring Soon'
        WHEN Certificate_Expiry_Date < DATEADD(DAY, 90, GETUTCDATE()) THEN 'Expiring (90 days)'
        ELSE 'Valid'
    END AS Expiration_Status,

    -- ESX Version (for context)
    ESX_Version,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vHost]
GO

PRINT 'Created [Reporting].[vw_Health_Certificate_Expiration]'
GO
