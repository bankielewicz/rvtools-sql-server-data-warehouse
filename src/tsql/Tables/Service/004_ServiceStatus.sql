/*
    004_ServiceStatus.sql
    Creates the Service.ServiceStatus table for service health monitoring.
    The Windows service updates this table on each heartbeat.

    Execute against: RVToolsDW database
    Requires: Service schema (006_CreateServiceSchema.sql)
    Part of: Phase 1 - Foundation
*/

USE [RVToolsDW];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Service].[ServiceStatus]'))
BEGIN
    CREATE TABLE [Service].[ServiceStatus] (
        ServiceStatusId INT IDENTITY(1,1) PRIMARY KEY,
        ServiceName NVARCHAR(100) NOT NULL,
        MachineName NVARCHAR(100) NOT NULL,

        Status NVARCHAR(50) NOT NULL DEFAULT 'Unknown',  -- 'Running', 'Stopped', 'Error', 'Unknown'
        LastHeartbeat DATETIME2 NOT NULL,
        ServiceVersion NVARCHAR(50) NULL,

        ActiveJobs INT DEFAULT 0,
        QueuedJobs INT DEFAULT 0,

        -- Constraints
        CONSTRAINT UQ_ServiceStatus_ServiceMachine UNIQUE (ServiceName, MachineName),
        CONSTRAINT CK_ServiceStatus_Status CHECK (Status IN ('Running', 'Stopped', 'Error', 'Unknown'))
    );

    CREATE INDEX IX_ServiceStatus_LastHeartbeat ON [Service].[ServiceStatus](LastHeartbeat DESC);
    PRINT 'Service.ServiceStatus table created successfully';
END
ELSE
BEGIN
    PRINT 'Service.ServiceStatus table already exists';
END
GO
