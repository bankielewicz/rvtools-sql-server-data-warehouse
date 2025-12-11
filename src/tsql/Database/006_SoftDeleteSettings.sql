/*
    RVTools Data Warehouse - Soft Delete Configuration Settings

    Purpose: Adds configuration settings for soft-delete/tombstone functionality

    Settings Added:
        - SoftDeleteEnabled: Enable/disable soft-delete detection during imports
        - SoftDeleteRetentionDays: Days to retain soft-deleted records in Current before archiving to History

    Usage: Execute against RVToolsDW database after 002_CreateSchemas.sql
           sqlcmd -S localhost -d RVToolsDW -i 006_SoftDeleteSettings.sql

    Related Files:
        - src/tsql/Tables/Current/002_AddSoftDeleteColumns.sql (schema changes)
        - src/tsql/StoredProcedures/usp_MergeTable.sql (soft-delete detection)
        - src/tsql/StoredProcedures/usp_ArchiveSoftDeletedRecords.sql (archive procedure)
*/

USE [RVToolsDW]
GO

-- ============================================================================
-- Add Soft Delete Settings to Config.Settings
-- ============================================================================
PRINT 'Adding soft-delete configuration settings...';

-- SoftDeleteEnabled: Master switch for soft-delete functionality
IF NOT EXISTS (SELECT 1 FROM [Config].[Settings] WHERE SettingName = 'SoftDeleteEnabled')
BEGIN
    INSERT INTO [Config].[Settings] (SettingName, SettingValue, Description, DataType)
    VALUES (
        'SoftDeleteEnabled',
        'true',
        'Enable soft-delete detection during imports. When enabled, records not present in the import will be marked as deleted.',
        'bool'
    );
    PRINT '  Added: SoftDeleteEnabled = true';
END
ELSE
BEGIN
    PRINT '  Skipped: SoftDeleteEnabled already exists';
END
GO

-- SoftDeleteRetentionDays: How long to keep deleted records in Current before archiving
IF NOT EXISTS (SELECT 1 FROM [Config].[Settings] WHERE SettingName = 'SoftDeleteRetentionDays')
BEGIN
    INSERT INTO [Config].[Settings] (SettingName, SettingValue, Description, DataType)
    VALUES (
        'SoftDeleteRetentionDays',
        '90',
        'Number of days to retain soft-deleted records in Current schema before archiving to History. After this period, records are moved to History tables and removed from Current.',
        'int'
    );
    PRINT '  Added: SoftDeleteRetentionDays = 90';
END
ELSE
BEGIN
    PRINT '  Skipped: SoftDeleteRetentionDays already exists';
END
GO

-- ============================================================================
-- Verification
-- ============================================================================
PRINT '';
PRINT 'Soft-delete settings verification:';
SELECT
    SettingName,
    SettingValue,
    DataType,
    LEFT(Description, 80) AS Description_Truncated
FROM [Config].[Settings]
WHERE SettingName IN ('SoftDeleteEnabled', 'SoftDeleteRetentionDays')
ORDER BY SettingName;
GO

PRINT '';
PRINT 'Soft-delete settings installation complete.';
GO
