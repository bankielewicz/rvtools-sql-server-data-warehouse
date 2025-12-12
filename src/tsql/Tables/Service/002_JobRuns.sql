/*
    002_JobRuns.sql
    Creates the Service.JobRuns table for job execution audit trail.

    Execute against: RVToolsDW database
    Requires: Service.Jobs table (001_Jobs.sql)
    Part of: Phase 1 - Foundation
*/

USE [RVToolsDW];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Service].[JobRuns]'))
BEGIN
    CREATE TABLE [Service].[JobRuns] (
        JobRunId BIGINT IDENTITY(1,1) PRIMARY KEY,
        JobId INT NOT NULL,
        ImportBatchId INT NULL,              -- FK to Audit.ImportBatch (can be NULL for failed runs)

        TriggerType NVARCHAR(50) NOT NULL,   -- 'Scheduled', 'Manual', 'FileWatcher'
        TriggerUser NVARCHAR(100) NULL,      -- Username if manually triggered

        StartTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        EndTime DATETIME2 NULL,
        DurationMs INT NULL,

        Status NVARCHAR(50) NOT NULL DEFAULT 'Running',
        FilesProcessed INT DEFAULT 0,
        FilesFailed INT DEFAULT 0,

        ErrorMessage NVARCHAR(MAX) NULL,

        -- Constraints
        CONSTRAINT FK_JobRuns_Jobs FOREIGN KEY (JobId)
            REFERENCES [Service].[Jobs](JobId) ON DELETE CASCADE,
        CONSTRAINT CK_JobRuns_TriggerType CHECK (TriggerType IN ('Scheduled', 'Manual', 'FileWatcher', 'Reschedule')),
        CONSTRAINT CK_JobRuns_Status CHECK (Status IN ('Running', 'Success', 'Failed', 'Cancelled', 'PartialSuccess'))
    );

    CREATE INDEX IX_JobRuns_JobId ON [Service].[JobRuns](JobId);
    CREATE INDEX IX_JobRuns_StartTime ON [Service].[JobRuns](StartTime DESC);
    CREATE INDEX IX_JobRuns_Status ON [Service].[JobRuns](Status);
    PRINT 'Service.JobRuns table created successfully';
END
ELSE
BEGIN
    PRINT 'Service.JobRuns table already exists';
END
GO
