/*
    RVTools Data Warehouse - Web Error Log Table

    Purpose: Captures detailed error information from the web application,
             including request context, user information, and timing data.

    Usage:
        -- Query recent web errors
        SELECT TOP 100 * FROM Web.ErrorLog ORDER BY LogTime DESC;

        -- Find errors by user
        SELECT * FROM Web.ErrorLog WHERE UserName = 'domain\user' ORDER BY LogTime DESC;

        -- Find errors by request path
        SELECT * FROM Web.ErrorLog WHERE RequestPath LIKE '/VMInventory%' ORDER BY LogTime DESC;

        -- Count errors by level
        SELECT LogLevel, COUNT(*) AS Count FROM Web.ErrorLog GROUP BY LogLevel;

    Dependencies: Requires Web schema (003_CreateWebSchema.sql)
*/

USE [RVToolsDW]
GO

-- Drop existing table if it exists (for clean recreate during development)
IF OBJECT_ID('Web.ErrorLog', 'U') IS NOT NULL
BEGIN
    DROP TABLE Web.ErrorLog;
    PRINT 'Dropped existing Web.ErrorLog table.'
END
GO

CREATE TABLE Web.ErrorLog (
    -- Primary Key
    ErrorLogId          BIGINT IDENTITY(1,1) PRIMARY KEY,

    -- Timestamp and Level
    LogTime             DATETIME2(3) NOT NULL DEFAULT GETUTCDATE(),
    LogLevel            NVARCHAR(20) NOT NULL,          -- Verbose, Info, Warning, Error

    -- Message Content
    Message             NVARCHAR(MAX) NULL,

    -- Exception Details
    ExceptionType       NVARCHAR(500) NULL,             -- e.g., SqlException, NullReferenceException
    ExceptionMessage    NVARCHAR(MAX) NULL,
    StackTrace          NVARCHAR(MAX) NULL,
    InnerException      NVARCHAR(MAX) NULL,             -- JSON serialized inner exception chain

    -- Request Context
    RequestId           NVARCHAR(100) NULL,             -- HttpContext.TraceIdentifier
    RequestPath         NVARCHAR(2000) NULL,            -- Request URL path
    RequestMethod       NVARCHAR(10) NULL,              -- GET, POST, etc.
    QueryString         NVARCHAR(MAX) NULL,
    RequestHeaders      NVARCHAR(MAX) NULL,             -- JSON of selected headers (sensitive excluded)

    -- User Context
    UserName            NVARCHAR(256) NULL,             -- User.Identity.Name

    -- Client Context
    ClientIP            NVARCHAR(50) NULL,              -- Remote IP address
    UserAgent           NVARCHAR(1000) NULL,            -- Browser user agent string

    -- Performance
    DurationMs          INT NULL,                       -- Request duration in milliseconds

    -- Server Context
    MachineName         NVARCHAR(100) NULL,             -- Server machine name

    -- Additional Context
    ContextData         NVARCHAR(MAX) NULL              -- JSON for any additional context data
);
GO

-- Indexes for common query patterns
CREATE NONCLUSTERED INDEX IX_WebErrorLog_LogTime
    ON Web.ErrorLog (LogTime DESC);

CREATE NONCLUSTERED INDEX IX_WebErrorLog_LogLevel
    ON Web.ErrorLog (LogLevel);

CREATE NONCLUSTERED INDEX IX_WebErrorLog_RequestPath
    ON Web.ErrorLog (RequestPath);

CREATE NONCLUSTERED INDEX IX_WebErrorLog_UserName
    ON Web.ErrorLog (UserName);

CREATE NONCLUSTERED INDEX IX_WebErrorLog_RequestId
    ON Web.ErrorLog (RequestId);

GO

PRINT 'Created Web.ErrorLog table with indexes.'
GO
