/*
    006_AddJobRunIdColumn.sql
    Adds JobRunId column to Audit.ImportBatch to link imports with job runs.

    Execute against: RVToolsDW database
    Requires: Service.JobRuns table (002_JobRuns.sql)
    Part of: Phase 1 - Foundation
*/

USE [RVToolsDW];
GO

-- Add JobRunId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Audit].[ImportBatch]') AND name = 'JobRunId')
BEGIN
    ALTER TABLE [Audit].[ImportBatch]
    ADD JobRunId BIGINT NULL;
    PRINT 'JobRunId column added to Audit.ImportBatch';
END
ELSE
BEGIN
    PRINT 'JobRunId column already exists in Audit.ImportBatch';
END
GO

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ImportBatch_JobRuns')
BEGIN
    ALTER TABLE [Audit].[ImportBatch]
    ADD CONSTRAINT FK_ImportBatch_JobRuns
    FOREIGN KEY (JobRunId) REFERENCES [Service].[JobRuns](JobRunId);
    PRINT 'Foreign key FK_ImportBatch_JobRuns added';
END
ELSE
BEGIN
    PRINT 'Foreign key FK_ImportBatch_JobRuns already exists';
END
GO

-- Add index if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ImportBatch_JobRunId' AND object_id = OBJECT_ID(N'[Audit].[ImportBatch]'))
BEGIN
    CREATE INDEX IX_ImportBatch_JobRunId ON [Audit].[ImportBatch](JobRunId);
    PRINT 'Index IX_ImportBatch_JobRunId created';
END
ELSE
BEGIN
    PRINT 'Index IX_ImportBatch_JobRunId already exists';
END
GO
