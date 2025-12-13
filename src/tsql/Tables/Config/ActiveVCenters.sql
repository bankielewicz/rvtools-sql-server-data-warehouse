/*
    Active vCenters Configuration Table

    Purpose: Track which vCenters are actively being imported
    Admin UI allows marking vCenters as active/inactive

    Usage:
        -- Get active vCenters only
        SELECT VIServer FROM Config.ActiveVCenters WHERE IsActive = 1

        -- Auto-populate from Audit.ImportBatch
        INSERT INTO Config.ActiveVCenters (VIServer, IsActive, LastImportDate)
        SELECT DISTINCT VIServer, 1, MAX(ImportStartTime)
        FROM Audit.ImportBatch
        GROUP BY VIServer
*/

USE [RVToolsDW]
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActiveVCenters' AND schema_id = SCHEMA_ID('Config'))
BEGIN
    CREATE TABLE [Config].[ActiveVCenters]
    (
        ActiveVCenterId     INT IDENTITY(1,1) PRIMARY KEY,
        VIServer            NVARCHAR(255) NOT NULL UNIQUE,
        IsActive            BIT NOT NULL DEFAULT 1,
        LastImportDate      DATETIME2 NULL,
        TotalImports        INT NOT NULL DEFAULT 0,
        Notes               NVARCHAR(500) NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ModifiedDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_ActiveVCenters_IsActive ON [Config].[ActiveVCenters](IsActive);

    PRINT 'Created [Config].[ActiveVCenters]'
END
ELSE
BEGIN
    PRINT '[Config].[ActiveVCenters] already exists'
END
GO
