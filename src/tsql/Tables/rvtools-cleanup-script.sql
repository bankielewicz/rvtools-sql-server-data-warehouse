  -- ============================================================================
  -- SAFE TEST DATA CLEANUP SCRIPT
  -- Purpose: Clear all imported/processed data while preserving configuration
  -- ============================================================================

  USE RVToolsDW;
  GO

  SET NOCOUNT ON;

  PRINT 'Starting test data cleanup...';
  PRINT '';

  -- ============================================================================
  -- Step 1: Clear Audit tables (respecting FK dependencies)
  -- ============================================================================
  PRINT 'Clearing Audit tables...';

  -- Child tables can be truncated (they reference others but aren't referenced)
  TRUNCATE TABLE [Audit].ImportLog;
  TRUNCATE TABLE [Audit].FailedRecords;
  TRUNCATE TABLE [Audit].ImportBatchDetail;

  -- Independent tables with no FK relationships
  TRUNCATE TABLE [Audit].MergeProgress;
  TRUNCATE TABLE [Audit].ErrorLog;

  -- Parent table: Use DELETE because it's referenced by FK constraints
  -- SQL Server doesn't allow TRUNCATE on tables with incoming FK references
  DELETE FROM [Audit].ImportBatch;

  PRINT '  Cleared 6 Audit tables';
  PRINT '';

  -- ============================================================================
  -- Step 2: Clear History tables (27 tables)
  -- ============================================================================
  PRINT 'Clearing History tables...';

  TRUNCATE TABLE [History].vInfo;
  TRUNCATE TABLE [History].vCPU;
  TRUNCATE TABLE [History].vMemory;
  TRUNCATE TABLE [History].vDisk;
  TRUNCATE TABLE [History].vPartition;
  TRUNCATE TABLE [History].vNetwork;
  TRUNCATE TABLE [History].vSnapshot;
  TRUNCATE TABLE [History].vTools;
  TRUNCATE TABLE [History].vHost;
  TRUNCATE TABLE [History].vCluster;
  TRUNCATE TABLE [History].vDatastore;
  TRUNCATE TABLE [History].vHealth;
  TRUNCATE TABLE [History].vCD;
  TRUNCATE TABLE [History].vUSB;
  TRUNCATE TABLE [History].vSource;
  TRUNCATE TABLE [History].vRP;
  TRUNCATE TABLE [History].vHBA;
  TRUNCATE TABLE [History].vNIC;
  TRUNCATE TABLE [History].vSwitch;
  TRUNCATE TABLE [History].vPort;
  TRUNCATE TABLE [History].dvSwitch;
  TRUNCATE TABLE [History].dvPort;
  TRUNCATE TABLE [History].vSC_VMK;
  TRUNCATE TABLE [History].vMultiPath;
  TRUNCATE TABLE [History].vLicense;
  TRUNCATE TABLE [History].vFileInfo;
  TRUNCATE TABLE [History].vMetaData;

  PRINT '  Cleared 27 History tables';
  PRINT '';

  -- ============================================================================
  -- Step 3: Clear Current tables (27 tables)
  -- ============================================================================
  PRINT 'Clearing Current tables...';

  TRUNCATE TABLE [Current].vInfo;
  TRUNCATE TABLE [Current].vCPU;
  TRUNCATE TABLE [Current].vMemory;
  TRUNCATE TABLE [Current].vDisk;
  TRUNCATE TABLE [Current].vPartition;
  TRUNCATE TABLE [Current].vNetwork;
  TRUNCATE TABLE [Current].vSnapshot;
  TRUNCATE TABLE [Current].vTools;
  TRUNCATE TABLE [Current].vHost;
  TRUNCATE TABLE [Current].vCluster;
  TRUNCATE TABLE [Current].vDatastore;
  TRUNCATE TABLE [Current].vHealth;
  TRUNCATE TABLE [Current].vCD;
  TRUNCATE TABLE [Current].vUSB;
  TRUNCATE TABLE [Current].vSource;
  TRUNCATE TABLE [Current].vRP;
  TRUNCATE TABLE [Current].vHBA;
  TRUNCATE TABLE [Current].vNIC;
  TRUNCATE TABLE [Current].vSwitch;
  TRUNCATE TABLE [Current].vPort;
  TRUNCATE TABLE [Current].dvSwitch;
  TRUNCATE TABLE [Current].dvPort;
  TRUNCATE TABLE [Current].vSC_VMK;
  TRUNCATE TABLE [Current].vMultiPath;
  TRUNCATE TABLE [Current].vLicense;
  TRUNCATE TABLE [Current].vFileInfo;
  TRUNCATE TABLE [Current].vMetaData;

  PRINT '  Cleared 27 Current tables';
  PRINT '';

  -- ============================================================================
  -- Step 4: Clear Staging tables (27 tables)
  -- ============================================================================
  PRINT 'Clearing Staging tables...';

  TRUNCATE TABLE [Staging].vInfo;
  TRUNCATE TABLE [Staging].vCPU;
  TRUNCATE TABLE [Staging].vMemory;
  TRUNCATE TABLE [Staging].vDisk;
  TRUNCATE TABLE [Staging].vPartition;
  TRUNCATE TABLE [Staging].vNetwork;
  TRUNCATE TABLE [Staging].vSnapshot;
  TRUNCATE TABLE [Staging].vTools;
  TRUNCATE TABLE [Staging].vHost;
  TRUNCATE TABLE [Staging].vCluster;
  TRUNCATE TABLE [Staging].vDatastore;
  TRUNCATE TABLE [Staging].vHealth;
  TRUNCATE TABLE [Staging].vCD;
  TRUNCATE TABLE [Staging].vUSB;
  TRUNCATE TABLE [Staging].vSource;
  TRUNCATE TABLE [Staging].vRP;
  TRUNCATE TABLE [Staging].vHBA;
  TRUNCATE TABLE [Staging].vNIC;
  TRUNCATE TABLE [Staging].vSwitch;
  TRUNCATE TABLE [Staging].vPort;
  TRUNCATE TABLE [Staging].dvSwitch;
  TRUNCATE TABLE [Staging].dvPort;
  TRUNCATE TABLE [Staging].vSC_VMK;
  TRUNCATE TABLE [Staging].vMultiPath;
  TRUNCATE TABLE [Staging].vLicense;
  TRUNCATE TABLE [Staging].vFileInfo;
  TRUNCATE TABLE [Staging].vMetaData;

  PRINT '  Cleared 27 Staging tables';
  PRINT '';

  -- ============================================================================
  -- Summary
  -- ============================================================================
  PRINT '============================================================';
  PRINT 'Cleanup Complete!';
  PRINT '============================================================';
  PRINT 'Tables cleared:    87 (all data tables)';
  PRINT 'Tables preserved:  4 (Config schema)';
  PRINT '';
  PRINT 'Preserved configuration tables:';
  PRINT '  - Config.Settings';
  PRINT '  - Config.TableMapping';
  PRINT '  - Config.ColumnMapping';
  PRINT '  - Config.TableRetention';
  PRINT '';
  PRINT 'Database is ready for fresh import testing.';

  -- Verification query
  SELECT
      'Audit' AS SchemaName, COUNT(*) AS TableCount
  FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = 'Audit'
  UNION ALL
  SELECT 'Config', COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Config'
  UNION ALL
  SELECT 'Staging', COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Staging'
  UNION ALL
  SELECT 'Current', COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Current'
  UNION ALL
  SELECT 'History', COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'History'
  ORDER BY SchemaName;
  GO