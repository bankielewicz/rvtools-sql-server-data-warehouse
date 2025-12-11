/*
    RVTools Data Warehouse - Refresh Column Mapping

    Purpose: Auto-populates Config.TableMapping and Config.ColumnMapping
             from sys.columns metadata. Run this after schema changes.

    Usage:
        EXEC dbo.usp_RefreshColumnMapping;

    Notes:
        - Truncates existing mapping data before repopulating
        - Derives data types from Current schema tables
        - Marks natural keys based on known patterns per table
        - Identifies boolean fields (BIT type in Current)
        - Can be called from PowerShell as a troubleshooting step
*/

USE [RVToolsDW]
GO

-- ============================================================================
-- First, create the mapping tables if they don't exist
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Config')
BEGIN
    EXEC('CREATE SCHEMA Config');
END
GO

IF OBJECT_ID('Config.ColumnMapping', 'U') IS NOT NULL
    DROP TABLE Config.ColumnMapping;
GO

IF OBJECT_ID('Config.TableMapping', 'U') IS NOT NULL
    DROP TABLE Config.TableMapping;
GO

CREATE TABLE Config.TableMapping (
    TableName NVARCHAR(100) NOT NULL PRIMARY KEY,
    NaturalKeyColumns NVARCHAR(500) NOT NULL,       -- Comma-separated column names for MERGE ON clause
    HistoryTrackingColumns NVARCHAR(MAX) NULL,      -- Columns that trigger history when changed (NULL = all)
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Config.ColumnMapping (
    MappingId INT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(100) NOT NULL,
    StagingColumnName NVARCHAR(200) NOT NULL,       -- Column name in Staging schema
    CurrentColumnName NVARCHAR(200) NOT NULL,       -- Column name in Current schema
    TargetDataType NVARCHAR(100) NOT NULL,          -- SQL data type for TRY_CAST
    MaxLength INT NULL,                              -- For NVARCHAR columns
    IsBooleanField BIT NOT NULL DEFAULT 0,          -- Needs True/1/Yes conversion
    IsNaturalKey BIT NOT NULL DEFAULT 0,            -- Part of MERGE ON clause
    IsRequired BIT NOT NULL DEFAULT 0,              -- Must have non-NULL value
    IsIdentity BIT NOT NULL DEFAULT 0,              -- Skip in INSERT (auto-generated)
    IsComputed BIT NOT NULL DEFAULT 0,              -- Skip in INSERT (computed column)
    IsSystemColumn BIT NOT NULL DEFAULT 0,          -- ImportBatchId, dates, etc.
    IncludeInMerge BIT NOT NULL DEFAULT 1,          -- Include in dynamic MERGE
    DefaultValue NVARCHAR(100) NULL,                -- Default if NULL
    OrdinalPosition INT NOT NULL,

    CONSTRAINT FK_ColumnMapping_TableMapping
        FOREIGN KEY (TableName) REFERENCES Config.TableMapping(TableName),
    CONSTRAINT UQ_ColumnMapping_Table_Column
        UNIQUE (TableName, CurrentColumnName)
);
GO

CREATE INDEX IX_ColumnMapping_TableName ON Config.ColumnMapping(TableName);
GO

-- ============================================================================
-- Create the refresh procedure
-- ============================================================================

IF OBJECT_ID('dbo.usp_RefreshColumnMapping', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_RefreshColumnMapping;
GO

CREATE PROCEDURE dbo.usp_RefreshColumnMapping
    @DebugMode BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RowsInserted INT;
    DECLARE @TablesProcessed INT;

    -- ========================================================================
    -- Step 1: Clear existing mapping data
    -- ========================================================================
    IF @DebugMode = 1 PRINT 'Truncating existing mapping tables...';

    DELETE FROM Config.ColumnMapping;
    DELETE FROM Config.TableMapping;

    -- ========================================================================
    -- Step 2: Populate TableMapping with natural keys
    -- ========================================================================
    IF @DebugMode = 1 PRINT 'Populating Config.TableMapping...';

    -- Insert all tables from Current schema with their natural keys
    -- Natural keys are based on RVTools data model
    INSERT INTO Config.TableMapping (TableName, NaturalKeyColumns)
    VALUES
        -- VM-related tables: keyed by VM identifier + vCenter
        ('vInfo',       'VM_UUID,VI_SDK_Server'),
        ('vCPU',        'VM,VI_SDK_Server'),
        ('vMemory',     'VM,VI_SDK_Server'),
        ('vDisk',       'VM,Disk,VI_SDK_Server'),
        ('vPartition',  'VM,Disk,VI_SDK_Server'),
        ('vNetwork',    'VM,Network,Adapter,VI_SDK_Server'),
        ('vCD',         'VM,Device_Node,VI_SDK_Server'),
        ('vUSB',        'VM,Device_Node,VI_SDK_Server'),
        ('vSnapshot',   'VM,Name,VI_SDK_Server'),
        ('vTools',      'VM,VI_SDK_Server'),

        -- Host-related tables
        ('vHost',       'Host,VI_SDK_Server'),
        ('vHBA',        'Host,Device,VI_SDK_Server'),
        ('vNIC',        'Host,Network_Device,VI_SDK_Server'),

        -- Networking tables
        ('vSwitch',     'Host,Name,VI_SDK_Server'),
        ('vPort',       'Host,Port_Group,VI_SDK_Server'),
        ('dvSwitch',    'Name,VI_SDK_Server'),
        ('dvPort',      'Port,Switch,VI_SDK_Server'),
        ('vSC_VMK',     'Host,Device,VI_SDK_Server'),

        -- Storage tables
        ('vDatastore',  'Name,VI_SDK_Server'),
        ('vMultiPath',  'Host,Disk,VI_SDK_Server'),
        ('vFileInfo',   'Path,File_Name,VI_SDK_Server'),

        -- Cluster/Resource tables
        ('vCluster',    'Name,VI_SDK_Server'),
        ('vRP',         'Resource_Pool_name,VI_SDK_Server'),

        -- Other tables
        ('vSource',     'Name,VI_SDK_Server'),
        ('vLicense',    'Name,VI_SDK_Server'),
        ('vHealth',     'Name,Message_type,VI_SDK_Server'),
        ('vMetaData',   'Server');

    SET @TablesProcessed = @@ROWCOUNT;
    IF @DebugMode = 1 PRINT '  Inserted ' + CAST(@TablesProcessed AS VARCHAR) + ' table mappings';

    -- ========================================================================
    -- Step 3: Populate ColumnMapping from sys.columns
    -- ========================================================================
    IF @DebugMode = 1 PRINT 'Populating Config.ColumnMapping from Current schema...';

    INSERT INTO Config.ColumnMapping (
        TableName,
        StagingColumnName,
        CurrentColumnName,
        TargetDataType,
        MaxLength,
        IsBooleanField,
        IsNaturalKey,
        IsRequired,
        IsIdentity,
        IsComputed,
        IsSystemColumn,
        IncludeInMerge,
        OrdinalPosition
    )
    SELECT
        t.name AS TableName,

        -- Staging column name (same as Current for now; adjust if different)
        c.name AS StagingColumnName,

        -- Current column name
        c.name AS CurrentColumnName,

        -- Build full data type string for TRY_CAST
        CASE
            WHEN tp.name IN ('nvarchar', 'varchar', 'nchar', 'char') THEN
                tp.name + '(' +
                CASE WHEN c.max_length = -1 THEN 'MAX'
                     WHEN tp.name LIKE 'n%' THEN CAST(c.max_length/2 AS VARCHAR)
                     ELSE CAST(c.max_length AS VARCHAR)
                END + ')'
            WHEN tp.name IN ('decimal', 'numeric') THEN
                tp.name + '(' + CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR) + ')'
            ELSE tp.name
        END AS TargetDataType,

        -- Max length for validation
        CASE WHEN c.max_length = -1 THEN NULL ELSE c.max_length END AS MaxLength,

        -- Boolean fields (BIT type or common boolean column names)
        CASE WHEN tp.name = 'bit' THEN 1 ELSE 0 END AS IsBooleanField,

        -- Natural key (will be updated in Step 4)
        0 AS IsNaturalKey,

        -- Required (not nullable and no default)
        CASE WHEN c.is_nullable = 0 AND c.default_object_id = 0 THEN 1 ELSE 0 END AS IsRequired,

        -- Identity columns
        c.is_identity AS IsIdentity,

        -- Computed columns
        c.is_computed AS IsComputed,

        -- System columns (metadata we add, not from RVTools)
        -- Includes soft-delete tracking columns added in 002_AddSoftDeleteColumns.sql
        CASE WHEN c.name IN ('RowId', 'ImportBatchId', 'CreatedDate', 'LastModifiedDate',
                             'ModifiedDate', 'ValidFrom', 'ValidTo', 'IsCurrent', 'SourceFile',
                             'StagingId', 'ImportRowNum', 'HistoryId',
                             -- Soft-delete columns (not from RVTools, managed by usp_MergeTable)
                             'LastSeenBatchId', 'LastSeenDate', 'IsDeleted',
                             'DeletedBatchId', 'DeletedDate', 'DeletedReason')
             THEN 1 ELSE 0 END AS IsSystemColumn,

        -- Include in merge (exclude system columns, identity, computed)
        -- System columns exist only in Current/History, not in Staging
        -- Note: LastModifiedDate/ModifiedDate ARE included - they get special handling in usp_MergeTable
        --       to use @Now (effective date for historical imports, or current time for regular imports)
        -- Soft-delete columns are managed directly by usp_MergeTable, not from Staging data
        CASE WHEN c.name IN ('RowId', 'CreatedDate',
                             'StagingId', 'ImportRowNum', 'HistoryId',
                             'ValidFrom', 'ValidTo', 'IsCurrent', 'SourceFile',
                             -- Soft-delete columns (managed by SOFT_DELETE/UPDATE_LAST_SEEN steps)
                             'LastSeenBatchId', 'LastSeenDate', 'IsDeleted',
                             'DeletedBatchId', 'DeletedDate', 'DeletedReason')
                  OR c.is_identity = 1
                  OR c.is_computed = 1
             THEN 0 ELSE 1 END AS IncludeInMerge,

        -- Ordinal position
        c.column_id AS OrdinalPosition

    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    INNER JOIN sys.columns c ON t.object_id = c.object_id
    INNER JOIN sys.types tp ON c.user_type_id = tp.user_type_id
    INNER JOIN Config.TableMapping tm ON t.name = tm.TableName
    WHERE s.name = 'Current'
    ORDER BY t.name, c.column_id;

    SET @RowsInserted = @@ROWCOUNT;
    IF @DebugMode = 1 PRINT '  Inserted ' + CAST(@RowsInserted AS VARCHAR) + ' column mappings';

    -- ========================================================================
    -- Step 4: Mark natural key columns
    -- ========================================================================
    IF @DebugMode = 1 PRINT 'Marking natural key columns...';

    -- Use proper comma-delimited matching to avoid substring false positives
    -- (e.g., 'VM' matching within 'VM_UUID')
    UPDATE cm
    SET IsNaturalKey = 1, IsRequired = 1
    FROM Config.ColumnMapping cm
    INNER JOIN Config.TableMapping tm ON cm.TableName = tm.TableName
    WHERE ',' + tm.NaturalKeyColumns + ',' LIKE '%,' + cm.CurrentColumnName + ',%';

    IF @DebugMode = 1 PRINT '  Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' natural key columns';

    -- ========================================================================
    -- Step 5: Verify staging columns exist (log mismatches)
    -- ========================================================================
    IF @DebugMode = 1
    BEGIN
        PRINT 'Checking for staging/current column mismatches...';

        -- Find columns in Current that don't exist in Staging
        SELECT
            cm.TableName,
            cm.CurrentColumnName,
            'Missing in Staging' AS Issue
        FROM Config.ColumnMapping cm
        WHERE cm.IsSystemColumn = 0
          AND cm.IncludeInMerge = 1
          AND NOT EXISTS (
              SELECT 1
              FROM sys.tables t
              INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
              INNER JOIN sys.columns c ON t.object_id = c.object_id
              WHERE s.name = 'Staging'
                AND t.name = cm.TableName
                AND c.name = cm.StagingColumnName
          );
    END

    -- ========================================================================
    -- Summary
    -- ========================================================================
    SELECT
        @TablesProcessed AS TablesConfigured,
        @RowsInserted AS ColumnsConfigured,
        (SELECT COUNT(*) FROM Config.ColumnMapping WHERE IsNaturalKey = 1) AS NaturalKeyColumns,
        (SELECT COUNT(*) FROM Config.ColumnMapping WHERE IsBooleanField = 1) AS BooleanColumns,
        (SELECT COUNT(*) FROM Config.ColumnMapping WHERE IncludeInMerge = 1) AS MergeableColumns;

    IF @DebugMode = 1 PRINT 'Column mapping refresh complete.';
END
GO

PRINT 'Created Config.TableMapping table';
PRINT 'Created Config.ColumnMapping table';
PRINT 'Created dbo.usp_RefreshColumnMapping procedure';
GO
