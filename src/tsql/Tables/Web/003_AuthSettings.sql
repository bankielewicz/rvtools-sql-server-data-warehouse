/*
    RVTools Data Warehouse - Authentication Settings Table
    Purpose: Stores authentication provider configuration (LocalDB or LDAP)

    Usage: Execute against RVToolsDW database after 002_Users.sql

    Behavior:
    - IsConfigured = 0 triggers first-time setup wizard
    - Only one row exists (singleton pattern)
    - LDAP fields reserved for future implementation
*/

USE [RVToolsDW]
GO

-- Drop existing table if it exists
IF OBJECT_ID('Web.AuthSettings', 'U') IS NOT NULL
BEGIN
    DROP TABLE Web.AuthSettings;
    PRINT 'Dropped existing Web.AuthSettings table.'
END
GO

-- Create AuthSettings table
CREATE TABLE Web.AuthSettings (
    AuthSettingsId      INT IDENTITY(1,1) PRIMARY KEY,
    AuthProvider        NVARCHAR(50) NOT NULL DEFAULT 'LocalDB',
    LdapServer          NVARCHAR(256) NULL,
    LdapDomain          NVARCHAR(256) NULL,
    LdapBaseDN          NVARCHAR(500) NULL,
    LdapSearchFilter    NVARCHAR(500) NULL,
    IsConfigured        BIT NOT NULL DEFAULT 0,
    CreatedDate         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT CK_AuthSettings_Provider CHECK (AuthProvider IN ('LocalDB', 'LDAP'))
);
GO

-- Insert default row (unconfigured state triggers first-time setup)
INSERT INTO Web.AuthSettings (AuthProvider, IsConfigured)
VALUES ('LocalDB', 0);
GO

PRINT 'Created Web.AuthSettings table with default row.'
GO
