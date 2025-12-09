/*
    RVTools Data Warehouse - Schema Creation Script

    Purpose: Creates database schemas for logical separation of tables

    Schemas:
    - Staging:  Temporary tables for xlsx data import (all NVARCHAR MAX)
    - Current:  Latest snapshot of all entities (typed columns)
    - History:  Historical records with ValidFrom/ValidTo (SCD Type 2)
    - Audit:    Import tracking, failed records, logging
    - Config:   Configuration settings (retention, thresholds, etc.)

    Usage: Execute against RVToolsDW database
           sqlcmd -S localhost -d RVToolsDW -i 002_CreateSchemas.sql
*/

USE [RVToolsDW]
GO

-- Create Staging schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Staging')
BEGIN
    EXEC('CREATE SCHEMA [Staging]')
    PRINT 'Schema [Staging] created.'
END
ELSE
    PRINT 'Schema [Staging] already exists.'
GO

-- Create Current schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Current')
BEGIN
    EXEC('CREATE SCHEMA [Current]')
    PRINT 'Schema [Current] created.'
END
ELSE
    PRINT 'Schema [Current] already exists.'
GO

-- Create History schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'History')
BEGIN
    EXEC('CREATE SCHEMA [History]')
    PRINT 'Schema [History] created.'
END
ELSE
    PRINT 'Schema [History] already exists.'
GO

-- Create Audit schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Audit')
BEGIN
    EXEC('CREATE SCHEMA [Audit]')
    PRINT 'Schema [Audit] created.'
END
ELSE
    PRINT 'Schema [Audit] already exists.'
GO

-- Create Config schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Config')
BEGIN
    EXEC('CREATE SCHEMA [Config]')
    PRINT 'Schema [Config] created.'
END
ELSE
    PRINT 'Schema [Config] already exists.'
GO

-- Create Reporting schema (for views)
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Reporting')
BEGIN
    EXEC('CREATE SCHEMA [Reporting]')
    PRINT 'Schema [Reporting] created.'
END
ELSE
    PRINT 'Schema [Reporting] already exists.'
GO

-- ============================================================================
-- Create Audit Tables
-- ============================================================================

-- Import Batch tracking table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID('Audit') AND name = 'ImportBatch')
BEGIN
    CREATE TABLE [Audit].[ImportBatch] (
        ImportBatchId       INT IDENTITY(1,1) PRIMARY KEY,
        SourceFile          NVARCHAR(500) NOT NULL,
        VIServer            NVARCHAR(255) NULL,
        ImportStartTime     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ImportEndTime       DATETIME2 NULL,
        Status              NVARCHAR(20) NOT NULL DEFAULT 'Running',  -- Running, Success, Partial, Failed
        TotalSheets         INT NULL,
        SheetsProcessed     INT NULL,
        TotalRowsSource     INT NULL,
        TotalRowsStaged     INT NULL,
        TotalRowsMerged     INT NULL,
        TotalRowsFailed     INT NULL,
        ErrorMessage        NVARCHAR(MAX) NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    )
    PRINT 'Table [Audit].[ImportBatch] created.'
END
GO

-- Import Batch Detail (per sheet)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID('Audit') AND name = 'ImportBatchDetail')
BEGIN
    CREATE TABLE [Audit].[ImportBatchDetail] (
        ImportBatchDetailId BIGINT IDENTITY(1,1) PRIMARY KEY,
        ImportBatchId       INT NOT NULL,
        SheetName           NVARCHAR(100) NOT NULL,
        SourceRowCount      INT NULL,
        StagedRowCount      INT NULL,
        MergedRowCount      INT NULL,
        ArchivedRowCount    INT NULL,
        FailedRowCount      INT NULL,
        StartTime           DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        EndTime             DATETIME2 NULL,
        Status              NVARCHAR(20) NOT NULL DEFAULT 'Running',
        ErrorMessage        NVARCHAR(MAX) NULL,
        CONSTRAINT FK_ImportBatchDetail_ImportBatch
            FOREIGN KEY (ImportBatchId) REFERENCES [Audit].[ImportBatch](ImportBatchId)
    )
    PRINT 'Table [Audit].[ImportBatchDetail] created.'
END
GO

-- Failed Records table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID('Audit') AND name = 'FailedRecords')
BEGIN
    CREATE TABLE [Audit].[FailedRecords] (
        FailedRecordId      BIGINT IDENTITY(1,1) PRIMARY KEY,
        ImportBatchId       INT NOT NULL,
        SheetName           NVARCHAR(100) NOT NULL,
        RowNumber           INT NOT NULL,
        ErrorType           NVARCHAR(50) NOT NULL,    -- DataType, FieldLength, Required, Validation, Duplicate
        ErrorMessage        NVARCHAR(MAX) NOT NULL,
        ColumnName          NVARCHAR(100) NULL,
        OriginalValue       NVARCHAR(MAX) NULL,
        RowDataJson         NVARCHAR(MAX) NULL,       -- Full row as JSON for debugging
        CreatedDate         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_FailedRecords_ImportBatch
            FOREIGN KEY (ImportBatchId) REFERENCES [Audit].[ImportBatch](ImportBatchId)
    )

    -- Index for querying by batch
    CREATE NONCLUSTERED INDEX IX_FailedRecords_ImportBatchId
        ON [Audit].[FailedRecords](ImportBatchId)

    PRINT 'Table [Audit].[FailedRecords] created.'
END
GO

-- Import Log table (detailed logging)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID('Audit') AND name = 'ImportLog')
BEGIN
    CREATE TABLE [Audit].[ImportLog] (
        ImportLogId         BIGINT IDENTITY(1,1) PRIMARY KEY,
        ImportBatchId       INT NULL,
        LogTime             DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LogLevel            NVARCHAR(20) NOT NULL,    -- Verbose, Info, Warning, Error
        Message             NVARCHAR(MAX) NOT NULL,
        SheetName           NVARCHAR(100) NULL,
        RowsAffected        INT NULL,
        DurationMs          INT NULL,
        CONSTRAINT FK_ImportLog_ImportBatch
            FOREIGN KEY (ImportBatchId) REFERENCES [Audit].[ImportBatch](ImportBatchId)
    )

    -- Index for querying by batch and level
    CREATE NONCLUSTERED INDEX IX_ImportLog_ImportBatchId_LogLevel
        ON [Audit].[ImportLog](ImportBatchId, LogLevel)

    PRINT 'Table [Audit].[ImportLog] created.'
END
GO

-- ============================================================================
-- Create Config Tables
-- ============================================================================

-- Configuration settings table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID('Config') AND name = 'Settings')
BEGIN
    CREATE TABLE [Config].[Settings] (
        SettingId           INT IDENTITY(1,1) PRIMARY KEY,
        SettingName         NVARCHAR(100) NOT NULL UNIQUE,
        SettingValue        NVARCHAR(500) NOT NULL,
        Description         NVARCHAR(500) NULL,
        DataType            NVARCHAR(20) NOT NULL DEFAULT 'string',  -- string, int, bool, datetime
        CreatedDate         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedDate        DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    )

    -- Insert default settings
    INSERT INTO [Config].[Settings] (SettingName, SettingValue, Description, DataType)
    VALUES
        ('RetentionDays', '365', 'Number of days to retain history records', 'int'),
        ('FailureThresholdPercent', '50', 'Percentage of failed rows that marks import as Failed', 'int'),
        ('DefaultLogLevel', 'Info', 'Default logging level (Verbose, Info, Warning, Error)', 'string'),
        ('ArchiveBeforePurge', 'false', 'Export history to file before purging', 'bool'),
        ('EnableEmailNotification', 'false', 'Send email on import failure', 'bool'),
        ('NotificationEmail', '', 'Email address for notifications', 'string')

    PRINT 'Table [Config].[Settings] created with default values.'
END
GO

-- Table-specific retention settings (optional override)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID('Config') AND name = 'TableRetention')
BEGIN
    CREATE TABLE [Config].[TableRetention] (
        TableRetentionId    INT IDENTITY(1,1) PRIMARY KEY,
        SchemaName          NVARCHAR(100) NOT NULL,
        TableName           NVARCHAR(100) NOT NULL,
        RetentionDays       INT NOT NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedDate        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_TableRetention_SchemaTable UNIQUE (SchemaName, TableName)
    )

    PRINT 'Table [Config].[TableRetention] created.'
END
GO

PRINT 'Schema creation complete.'
GO
