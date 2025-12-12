/*
    001_Jobs.sql
    Creates the Service.Jobs table for import job configuration.

    Execute against: RVToolsDW database
    Requires: Service schema (006_CreateServiceSchema.sql)
    Part of: Phase 1 - Foundation
*/

USE [RVToolsDW];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Service].[Jobs]'))
BEGIN
    CREATE TABLE [Service].[Jobs] (
        JobId INT IDENTITY(1,1) PRIMARY KEY,
        JobName NVARCHAR(100) NOT NULL,
        JobType NVARCHAR(50) NOT NULL DEFAULT 'Scheduled',  -- 'Scheduled', 'FileWatcher', 'Manual'
        IsEnabled BIT NOT NULL DEFAULT 1,

        -- Folder configuration
        IncomingFolder NVARCHAR(500) NOT NULL,
        ProcessedFolder NVARCHAR(500) NULL,
        ErrorsFolder NVARCHAR(500) NULL,

        -- Schedule (cron expression for Quartz.NET)
        CronSchedule NVARCHAR(100) NULL,         -- e.g., "0 0 2 * * ?" (daily at 2 AM)
        TimeZone NVARCHAR(100) DEFAULT 'UTC',

        -- Database connection
        ServerInstance NVARCHAR(200) NOT NULL,
        DatabaseName NVARCHAR(100) NOT NULL DEFAULT 'RVToolsDW',
        UseWindowsAuth BIT NOT NULL DEFAULT 1,
        EncryptedCredential NVARCHAR(MAX) NULL,  -- Encrypted via Data Protection API

        -- vCenter mapping
        VIServer NVARCHAR(100) NULL,

        -- Metadata
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME2 NULL,

        -- Constraints
        CONSTRAINT UQ_Jobs_JobName UNIQUE (JobName),
        CONSTRAINT CK_Jobs_JobType CHECK (JobType IN ('Scheduled', 'FileWatcher', 'Manual'))
    );

    CREATE INDEX IX_Jobs_IsEnabled ON [Service].[Jobs](IsEnabled);
    PRINT 'Service.Jobs table created successfully';
END
ELSE
BEGIN
    PRINT 'Service.Jobs table already exists';
END
GO
