/*
    003_JobTriggers.sql
    Creates the Service.JobTriggers table for manual trigger queue.
    Used for communication between web app and Windows service via database polling.

    Execute against: RVToolsDW database
    Requires: Service.Jobs table (001_Jobs.sql)
    Part of: Phase 1 - Foundation
*/

USE [RVToolsDW];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Service].[JobTriggers]'))
BEGIN
    CREATE TABLE [Service].[JobTriggers] (
        TriggerId BIGINT IDENTITY(1,1) PRIMARY KEY,
        JobId INT NOT NULL,
        TriggerType NVARCHAR(50) NOT NULL DEFAULT 'Manual',  -- 'Manual', 'Reschedule'
        TriggerUser NVARCHAR(100) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ProcessedDate DATETIME2 NULL,

        -- Constraints
        CONSTRAINT FK_JobTriggers_Jobs FOREIGN KEY (JobId)
            REFERENCES [Service].[Jobs](JobId) ON DELETE CASCADE,
        CONSTRAINT CK_JobTriggers_TriggerType CHECK (TriggerType IN ('Manual', 'Reschedule'))
    );

    CREATE INDEX IX_JobTriggers_Processed ON [Service].[JobTriggers](ProcessedDate)
        WHERE ProcessedDate IS NULL;  -- Filtered index for unprocessed triggers
    CREATE INDEX IX_JobTriggers_JobId ON [Service].[JobTriggers](JobId);
    PRINT 'Service.JobTriggers table created successfully';
END
ELSE
BEGIN
    PRINT 'Service.JobTriggers table already exists';
END
GO
