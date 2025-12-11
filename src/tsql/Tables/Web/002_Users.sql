/*
    RVTools Data Warehouse - Web Users Table
    Purpose: Stores local user accounts for web application authentication

    Usage: Execute against RVToolsDW database after 001_ErrorLog.sql

    Columns:
    - UserId: Primary key
    - Username: Unique login name
    - PasswordHash: PBKDF2-SHA256 hash (base64)
    - Salt: Random 32-byte salt (base64)
    - Role: 'Admin' or 'User'
    - ForcePasswordChange: True for default admin account
    - FailedLoginAttempts: Counter for lockout
    - LockoutEnd: UTC time when lockout expires
*/

USE [RVToolsDW]
GO

-- Drop existing table if it exists
IF OBJECT_ID('Web.Users', 'U') IS NOT NULL
BEGIN
    DROP TABLE Web.Users;
    PRINT 'Dropped existing Web.Users table.'
END
GO

-- Create Users table
CREATE TABLE Web.Users (
    UserId              INT IDENTITY(1,1) PRIMARY KEY,
    Username            NVARCHAR(100) NOT NULL,
    PasswordHash        NVARCHAR(256) NOT NULL,
    Salt                NVARCHAR(64) NOT NULL,
    Email               NVARCHAR(256) NULL,
    Role                NVARCHAR(50) NOT NULL DEFAULT 'User',
    IsActive            BIT NOT NULL DEFAULT 1,
    ForcePasswordChange BIT NOT NULL DEFAULT 0,
    LastLoginDate       DATETIME2 NULL,
    FailedLoginAttempts INT NOT NULL DEFAULT 0,
    LockoutEnd          DATETIME2 NULL,
    CreatedDate         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'User'))
);
GO

-- Create indexes for common queries
CREATE NONCLUSTERED INDEX IX_Users_Username ON Web.Users (Username);
CREATE NONCLUSTERED INDEX IX_Users_Role ON Web.Users (Role);
CREATE NONCLUSTERED INDEX IX_Users_IsActive ON Web.Users (IsActive);
GO

PRINT 'Created Web.Users table with indexes.'
GO
