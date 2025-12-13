-- ============================================================================
-- SAFE TEST DATA CLEANUP SCRIPT
-- Purpose: Clear all imported/processed data while preserving configuration
-- Use this to reset a test environment for fresh import testing
-- ============================================================================

USE RVToolsDW;
GO

SET NOCOUNT ON;

PRINT '============================================================';
PRINT 'RVTools Data Warehouse - Test Data Cleanup';
PRINT '============================================================';
PRINT '';
PRINT 'This script will:';
PRINT '  - Clear all imported RVTools data (Staging/Current/History)';
PRINT '  - Clear audit/logging tables';
PRINT '  - Clear service job run history';
PRINT '  - Clear web session logs';
PRINT '';
PRINT 'This script will PRESERVE:';
PRINT '  - Config schema (Settings, TableMapping, ColumnMapping, etc.)';
PRINT '  - Web.Users and Web.AuthSettings (user accounts)';
PRINT '  - Service.Jobs (job configurations)';
PRINT '';
PRINT 'Starting cleanup...';
PRINT '';

-- ============================================================================
-- Step 1: Clear Service schema (job runs, triggers, status)
-- Preserves: Service.Jobs (job configuration)
-- ============================================================================
IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Service')
BEGIN
    PRINT 'Clearing Service tables (preserving job configurations)...';

    -- Clear job triggers (manual trigger queue)
    IF OBJECT_ID('[Service].[JobTriggers]', 'U') IS NOT NULL
        TRUNCATE TABLE [Service].[JobTriggers];

    -- Clear service status (heartbeat)
    IF OBJECT_ID('[Service].[ServiceStatus]', 'U') IS NOT NULL
        TRUNCATE TABLE [Service].[ServiceStatus];

    -- Clear job runs (execution history) - must use DELETE due to FK from ImportBatch
    IF OBJECT_ID('[Service].[JobRuns]', 'U') IS NOT NULL
        DELETE FROM [Service].[JobRuns];

    PRINT '  Cleared: JobRuns, JobTriggers, ServiceStatus';
    PRINT '  Preserved: Jobs (configurations)';
    PRINT '';
END

-- ============================================================================
-- Step 2: Clear Audit tables (respecting FK dependencies)
-- ============================================================================
PRINT 'Clearing Audit tables...';

-- Child tables first (they reference ImportBatch)
IF OBJECT_ID('[Audit].[ImportLog]', 'U') IS NOT NULL
    TRUNCATE TABLE [Audit].[ImportLog];

IF OBJECT_ID('[Audit].[FailedRecords]', 'U') IS NOT NULL
    TRUNCATE TABLE [Audit].[FailedRecords];

IF OBJECT_ID('[Audit].[ImportBatchDetail]', 'U') IS NOT NULL
    TRUNCATE TABLE [Audit].[ImportBatchDetail];

-- Independent tables
IF OBJECT_ID('[Audit].[MergeProgress]', 'U') IS NOT NULL
    TRUNCATE TABLE [Audit].[MergeProgress];

IF OBJECT_ID('[Audit].[ErrorLog]', 'U') IS NOT NULL
    TRUNCATE TABLE [Audit].[ErrorLog];

-- Parent table: DELETE because it may have FK from Service.JobRuns
IF OBJECT_ID('[Audit].[ImportBatch]', 'U') IS NOT NULL
    DELETE FROM [Audit].[ImportBatch];

PRINT '  Cleared: ImportBatch, ImportBatchDetail, ImportLog, FailedRecords, MergeProgress, ErrorLog';
PRINT '';

-- ============================================================================
-- Step 3: Clear Web schema logs (preserving users and auth settings)
-- ============================================================================
IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Web')
BEGIN
    PRINT 'Clearing Web schema logs (preserving users and auth settings)...';

    -- Clear error log
    IF OBJECT_ID('[Web].[ErrorLog]', 'U') IS NOT NULL
        TRUNCATE TABLE [Web].[ErrorLog];

    -- Clear sessions (login audit trail)
    IF OBJECT_ID('[Web].[Sessions]', 'U') IS NOT NULL
        TRUNCATE TABLE [Web].[Sessions];

    PRINT '  Cleared: ErrorLog, Sessions';
    PRINT '  Preserved: Users, AuthSettings';
    PRINT '';
END

-- ============================================================================
-- Step 4: Clear Config.ActiveVCenters (optional - uncomment if desired)
-- This resets the vCenter active/inactive status
-- ============================================================================
-- PRINT 'Clearing Config.ActiveVCenters...';
-- IF OBJECT_ID('[Config].[ActiveVCenters]', 'U') IS NOT NULL
--     TRUNCATE TABLE [Config].[ActiveVCenters];
-- PRINT '  Cleared: ActiveVCenters';
-- PRINT '';

-- ============================================================================
-- Step 5: Clear History tables (27 tables)
-- ============================================================================
PRINT 'Clearing History tables (27 tables)...';

TRUNCATE TABLE [History].[vInfo];
TRUNCATE TABLE [History].[vCPU];
TRUNCATE TABLE [History].[vMemory];
TRUNCATE TABLE [History].[vDisk];
TRUNCATE TABLE [History].[vPartition];
TRUNCATE TABLE [History].[vNetwork];
TRUNCATE TABLE [History].[vSnapshot];
TRUNCATE TABLE [History].[vTools];
TRUNCATE TABLE [History].[vHost];
TRUNCATE TABLE [History].[vCluster];
TRUNCATE TABLE [History].[vDatastore];
TRUNCATE TABLE [History].[vHealth];
TRUNCATE TABLE [History].[vCD];
TRUNCATE TABLE [History].[vUSB];
TRUNCATE TABLE [History].[vSource];
TRUNCATE TABLE [History].[vRP];
TRUNCATE TABLE [History].[vHBA];
TRUNCATE TABLE [History].[vNIC];
TRUNCATE TABLE [History].[vSwitch];
TRUNCATE TABLE [History].[vPort];
TRUNCATE TABLE [History].[dvSwitch];
TRUNCATE TABLE [History].[dvPort];
TRUNCATE TABLE [History].[vSC_VMK];
TRUNCATE TABLE [History].[vMultiPath];
TRUNCATE TABLE [History].[vLicense];
TRUNCATE TABLE [History].[vFileInfo];
TRUNCATE TABLE [History].[vMetaData];

PRINT '  Cleared 27 History tables';
PRINT '';

-- ============================================================================
-- Step 6: Clear Current tables (27 tables)
-- ============================================================================
PRINT 'Clearing Current tables (27 tables)...';

TRUNCATE TABLE [Current].[vInfo];
TRUNCATE TABLE [Current].[vCPU];
TRUNCATE TABLE [Current].[vMemory];
TRUNCATE TABLE [Current].[vDisk];
TRUNCATE TABLE [Current].[vPartition];
TRUNCATE TABLE [Current].[vNetwork];
TRUNCATE TABLE [Current].[vSnapshot];
TRUNCATE TABLE [Current].[vTools];
TRUNCATE TABLE [Current].[vHost];
TRUNCATE TABLE [Current].[vCluster];
TRUNCATE TABLE [Current].[vDatastore];
TRUNCATE TABLE [Current].[vHealth];
TRUNCATE TABLE [Current].[vCD];
TRUNCATE TABLE [Current].[vUSB];
TRUNCATE TABLE [Current].[vSource];
TRUNCATE TABLE [Current].[vRP];
TRUNCATE TABLE [Current].[vHBA];
TRUNCATE TABLE [Current].[vNIC];
TRUNCATE TABLE [Current].[vSwitch];
TRUNCATE TABLE [Current].[vPort];
TRUNCATE TABLE [Current].[dvSwitch];
TRUNCATE TABLE [Current].[dvPort];
TRUNCATE TABLE [Current].[vSC_VMK];
TRUNCATE TABLE [Current].[vMultiPath];
TRUNCATE TABLE [Current].[vLicense];
TRUNCATE TABLE [Current].[vFileInfo];
TRUNCATE TABLE [Current].[vMetaData];

PRINT '  Cleared 27 Current tables';
PRINT '';

-- ============================================================================
-- Step 7: Clear Staging tables (27 tables)
-- ============================================================================
PRINT 'Clearing Staging tables (27 tables)...';

TRUNCATE TABLE [Staging].[vInfo];
TRUNCATE TABLE [Staging].[vCPU];
TRUNCATE TABLE [Staging].[vMemory];
TRUNCATE TABLE [Staging].[vDisk];
TRUNCATE TABLE [Staging].[vPartition];
TRUNCATE TABLE [Staging].[vNetwork];
TRUNCATE TABLE [Staging].[vSnapshot];
TRUNCATE TABLE [Staging].[vTools];
TRUNCATE TABLE [Staging].[vHost];
TRUNCATE TABLE [Staging].[vCluster];
TRUNCATE TABLE [Staging].[vDatastore];
TRUNCATE TABLE [Staging].[vHealth];
TRUNCATE TABLE [Staging].[vCD];
TRUNCATE TABLE [Staging].[vUSB];
TRUNCATE TABLE [Staging].[vSource];
TRUNCATE TABLE [Staging].[vRP];
TRUNCATE TABLE [Staging].[vHBA];
TRUNCATE TABLE [Staging].[vNIC];
TRUNCATE TABLE [Staging].[vSwitch];
TRUNCATE TABLE [Staging].[vPort];
TRUNCATE TABLE [Staging].[dvSwitch];
TRUNCATE TABLE [Staging].[dvPort];
TRUNCATE TABLE [Staging].[vSC_VMK];
TRUNCATE TABLE [Staging].[vMultiPath];
TRUNCATE TABLE [Staging].[vLicense];
TRUNCATE TABLE [Staging].[vFileInfo];
TRUNCATE TABLE [Staging].[vMetaData];

PRINT '  Cleared 27 Staging tables';
PRINT '';

-- ============================================================================
-- Summary
-- ============================================================================
PRINT '============================================================';
PRINT 'Cleanup Complete!';
PRINT '============================================================';
PRINT '';
PRINT 'CLEARED:';
PRINT '  - Staging tables:  27 (raw import data)';
PRINT '  - Current tables:  27 (current snapshot)';
PRINT '  - History tables:  27 (SCD Type 2 history)';
PRINT '  - Audit tables:    6  (import tracking)';
PRINT '  - Service tables:  3  (job runs, triggers, status)';
PRINT '  - Web tables:      2  (error log, sessions)';
PRINT '  ----------------------------------------';
PRINT '  Total cleared:     92 tables';
PRINT '';
PRINT 'PRESERVED:';
PRINT '  - Config.Settings';
PRINT '  - Config.TableMapping';
PRINT '  - Config.ColumnMapping';
PRINT '  - Config.TableRetention';
PRINT '  - Config.ActiveVCenters (uncomment section above to clear)';
PRINT '  - Service.Jobs (job configurations)';
PRINT '  - Web.Users (user accounts)';
PRINT '  - Web.AuthSettings (auth provider config)';
PRINT '';
PRINT 'Database is ready for fresh import testing.';
PRINT '';

-- ============================================================================
-- Verification: Show table counts by schema
-- ============================================================================
PRINT 'Verification - Tables per schema:';

SELECT
    s.name AS SchemaName,
    COUNT(*) AS TableCount
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name IN ('Audit', 'Config', 'Staging', 'Current', 'History', 'Web', 'Service', 'Reporting')
GROUP BY s.name
ORDER BY s.name;

-- Show row counts for key tables to verify cleanup
PRINT '';
PRINT 'Row counts after cleanup:';

SELECT 'Audit.ImportBatch' AS TableName, COUNT(*) AS RowCount FROM [Audit].[ImportBatch]
UNION ALL SELECT 'Current.vInfo', COUNT(*) FROM [Current].[vInfo]
UNION ALL SELECT 'History.vInfo', COUNT(*) FROM [History].[vInfo]
UNION ALL SELECT 'Staging.vInfo', COUNT(*) FROM [Staging].[vInfo];
GO
