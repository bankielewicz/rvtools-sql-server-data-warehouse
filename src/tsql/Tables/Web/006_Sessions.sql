/*
    RVTools Data Warehouse - Web Sessions Table
    Purpose: Tracks authentication events for audit trail (both LocalDB and LDAP users)

    Usage: Execute against RVToolsDW database after 005_AuthSettings_CertValidation.sql

    Columns:
    - SessionId: Primary key (BIGINT for high volume)
    - Username: Login name (from either LocalDB or LDAP)
    - AuthSource: 'LocalDB' or 'LDAP'
    - UserId: FK to Web.Users (NULL for LDAP users who are transient)
    - Role: User's role at login time
    - Email: User's email (if available)
    - LoginTime: UTC timestamp of login
    - LogoutTime: UTC timestamp of logout (NULL if session still active or not tracked)
    - IPAddress: Client IP address
    - UserAgent: Browser/client user agent string
    - SessionToken: Optional tracking for cookie/session ID
*/

USE [RVToolsDW]
GO

SET QUOTED_IDENTIFIER ON
GO

-- Drop existing table if it exists
IF OBJECT_ID('Web.Sessions', 'U') IS NOT NULL
BEGIN
    DROP TABLE Web.Sessions;
    PRINT 'Dropped existing Web.Sessions table.'
END
GO

-- Create Sessions table
CREATE TABLE Web.Sessions (
    SessionId           BIGINT IDENTITY(1,1) PRIMARY KEY,
    Username            NVARCHAR(100) NOT NULL,
    AuthSource          NVARCHAR(20) NOT NULL,
    UserId              INT NULL,
    Role                NVARCHAR(50) NOT NULL,
    Email               NVARCHAR(255) NULL,
    LoginTime           DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LogoutTime          DATETIME2 NULL,
    IPAddress           NVARCHAR(50) NULL,
    UserAgent           NVARCHAR(500) NULL,
    SessionToken        NVARCHAR(100) NULL,

    CONSTRAINT CK_Sessions_AuthSource CHECK (AuthSource IN ('LocalDB', 'LDAP')),
    CONSTRAINT FK_Sessions_UserId FOREIGN KEY (UserId) REFERENCES Web.Users(UserId) ON DELETE SET NULL
);
GO

-- Create indexes for common queries
CREATE NONCLUSTERED INDEX IX_Sessions_Username_LoginTime ON Web.Sessions (Username, LoginTime DESC);
CREATE NONCLUSTERED INDEX IX_Sessions_AuthSource_LoginTime ON Web.Sessions (AuthSource, LoginTime DESC);
CREATE NONCLUSTERED INDEX IX_Sessions_LoginTime ON Web.Sessions (LoginTime DESC);
CREATE NONCLUSTERED INDEX IX_Sessions_UserId ON Web.Sessions (UserId) WHERE UserId IS NOT NULL;
GO

PRINT 'Created Web.Sessions table with indexes.'
GO
