/*
    Performance Indexes for Global Time Filter Feature

    Purpose: Optimize queries that filter by VI_SDK_Server for active vCenter filtering

    Deploy after tables exist. Safe to run multiple times (IF NOT EXISTS checks).
*/

USE [RVToolsDW]
GO

-- Index on Current.vInfo for active vCenter filtering
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_vInfo_VI_SDK_Server' AND object_id = OBJECT_ID('[Current].[vInfo]'))
BEGIN
    CREATE INDEX IX_vInfo_VI_SDK_Server ON [Current].[vInfo](VI_SDK_Server);
    PRINT 'Created IX_vInfo_VI_SDK_Server on [Current].[vInfo]';
END
GO

-- Index on History.vInfo for active vCenter filtering
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_History_vInfo_VI_SDK_Server' AND object_id = OBJECT_ID('[History].[vInfo]'))
BEGIN
    CREATE INDEX IX_History_vInfo_VI_SDK_Server ON [History].[vInfo](VI_SDK_Server);
    PRINT 'Created IX_History_vInfo_VI_SDK_Server on [History].[vInfo]';
END
GO

-- Composite index on History.vInfo for Change Summary view (VM creations/deletions)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_History_vInfo_VM_UUID_ValidFrom' AND object_id = OBJECT_ID('[History].[vInfo]'))
BEGIN
    CREATE INDEX IX_History_vInfo_VM_UUID_ValidFrom ON [History].[vInfo](VM_UUID, VI_SDK_Server, ValidFrom);
    PRINT 'Created IX_History_vInfo_VM_UUID_ValidFrom on [History].[vInfo]';
END
GO

-- Index on Audit.ImportBatch for vCenter Status view
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ImportBatch_VIServer_Status' AND object_id = OBJECT_ID('[Audit].[ImportBatch]'))
BEGIN
    CREATE INDEX IX_ImportBatch_VIServer_Status ON [Audit].[ImportBatch](VIServer, Status) INCLUDE (ImportStartTime);
    PRINT 'Created IX_ImportBatch_VIServer_Status on [Audit].[ImportBatch]';
END
GO

PRINT 'Global Time Filter indexes complete';
GO
