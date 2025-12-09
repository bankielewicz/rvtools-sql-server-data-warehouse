/*
    RVTools Data Warehouse - Error Log Table

    Purpose: Captures detailed error information from stored procedures,
             especially during MERGE operations. Survives transaction rollbacks
             when logged properly.

    Usage:
        -- Query recent errors
        SELECT * FROM Audit.ErrorLog WHERE ImportBatchId = @BatchId ORDER BY LogTime;

        -- Find errors by table
        SELECT * FROM Audit.ErrorLog WHERE TableName = 'vHealth' ORDER BY LogTime DESC;
*/

USE [RVToolsDW]
GO

IF OBJECT_ID('Audit.ErrorLog', 'U') IS NOT NULL
    DROP TABLE Audit.ErrorLog;
GO

CREATE TABLE Audit.ErrorLog (
    ErrorLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    LogTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ImportBatchId INT NULL,
    ProcedureName NVARCHAR(200) NOT NULL,
    TableName NVARCHAR(100) NULL,
    Operation NVARCHAR(50) NULL,           -- 'MERGE', 'HISTORY_CLOSE', 'HISTORY_INSERT', 'VALIDATION'
    ErrorNumber INT NULL,
    ErrorSeverity INT NULL,
    ErrorState INT NULL,
    ErrorLine INT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    DynamicSQL NVARCHAR(MAX) NULL,         -- The SQL that was attempted (truncated if too long)
    ContextData NVARCHAR(MAX) NULL,        -- JSON with additional context (row counts, parameters, etc.)

    INDEX IX_ErrorLog_BatchId (ImportBatchId),
    INDEX IX_ErrorLog_LogTime (LogTime),
    INDEX IX_ErrorLog_TableName (TableName)
);
GO

PRINT 'Created Audit.ErrorLog table';
GO
