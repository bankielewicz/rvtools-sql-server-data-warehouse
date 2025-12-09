/*
    Add Datacenter_2 column to staging tables

    RVTools exports have "Datacenter" appearing twice in some sheets.
    The import process renames the second occurrence to "Datacenter_2".

    Affected tables: vInfo, vCPU, vMemory, vDisk, vPartition, vNetwork, vCD, vUSB, vSnapshot, vTools
*/

USE [RVToolsDW]
GO

-- vInfo
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vInfo' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vInfo] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vInfo';
END
GO

-- vCPU
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vCPU' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vCPU] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vCPU';
END
GO

-- vMemory
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vMemory' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vMemory] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vMemory';
END
GO

-- vDisk
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vDisk' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vDisk] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vDisk';
END
GO

-- vPartition
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vPartition' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vPartition] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vPartition';
END
GO

-- vNetwork
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vNetwork' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vNetwork] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vNetwork';
END
GO

-- vCD
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vCD' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vCD] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vCD';
END
GO

-- vUSB
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vUSB' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vUSB] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vUSB';
END
GO

-- vSnapshot
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vSnapshot' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vSnapshot] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vSnapshot';
END
GO

-- vTools
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = 'vTools' AND COLUMN_NAME = 'Datacenter_2')
BEGIN
    ALTER TABLE [Staging].[vTools] ADD [Datacenter_2] NVARCHAR(MAX) NULL;
    PRINT 'Added Datacenter_2 to Staging.vTools';
END
GO

PRINT 'Datacenter_2 column additions complete.';
GO
