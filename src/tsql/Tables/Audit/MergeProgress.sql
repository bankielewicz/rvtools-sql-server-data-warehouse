/*
    RVTools Data Warehouse - Merge Progress Tracking Table

    Purpose: Tracks in-progress merge operations. When a merge fails mid-transaction,
             records with Status='InProgress' show exactly where the failure occurred.

    Key Feature: Insert happens BEFORE the operation, so even if the transaction
                 rolls back, we know which table was being processed.

    Usage:
        -- Find where an import stopped
        SELECT TableName, Status, StartTime, ErrorMessage
        FROM Audit.MergeProgress
        WHERE ImportBatchId = @BatchId
        ORDER BY StartTime;

        -- Find tables that frequently fail
        SELECT TableName, COUNT(*) AS FailureCount
        FROM Audit.MergeProgress
        WHERE Status = 'Failed'
        GROUP BY TableName
        ORDER BY FailureCount DESC;
*/

USE [RVToolsDW]
GO

IF OBJECT_ID('Audit.MergeProgress', 'U') IS NOT NULL
    DROP TABLE Audit.MergeProgress;
GO

CREATE TABLE Audit.MergeProgress (
    ProgressId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    TableName NVARCHAR(100) NOT NULL,
    Operation NVARCHAR(50) NOT NULL DEFAULT 'MERGE',  -- 'MERGE', 'HISTORY_CLOSE', 'HISTORY_INSERT'
    StartTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndTime DATETIME2 NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'InProgress',  -- InProgress, Success, Failed, Skipped
    RowsInStaging INT NULL,
    RowsProcessed INT NULL,
    RowsInserted INT NULL,
    RowsUpdated INT NULL,
    RowsDeleted INT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    DurationMs INT NULL,

    INDEX IX_MergeProgress_BatchId (ImportBatchId),
    INDEX IX_MergeProgress_Status (Status),
    INDEX IX_MergeProgress_TableName (TableName)
);
GO

PRINT 'Created Audit.MergeProgress table';
GO
