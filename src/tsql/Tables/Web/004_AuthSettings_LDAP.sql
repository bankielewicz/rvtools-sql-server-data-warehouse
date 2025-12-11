/*
    RVTools Data Warehouse - LDAP Authentication Settings
    Purpose: Add LDAP/Active Directory configuration columns to Web.AuthSettings

    Usage: Execute against RVToolsDW database after 003_AuthSettings.sql

    New Columns:
    - LdapPort: LDAP server port (389 or 636 for LDAPS)
    - LdapUseSsl: Use SSL/TLS for LDAP connection
    - LdapBindDN: Service account DN for searching (optional)
    - LdapBindPassword: Service account password (encrypted)
    - LdapAdminGroup: AD group DN for Admin role
    - LdapUserGroup: AD group DN for User role
    - LdapFallbackToLocal: Try local DB auth if LDAP fails
*/

USE [RVToolsDW]
GO

-- Add LDAP columns if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapPort')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapPort INT NOT NULL DEFAULT 389;
    PRINT 'Added LdapPort column.'
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapUseSsl')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapUseSsl BIT NOT NULL DEFAULT 0;
    PRINT 'Added LdapUseSsl column.'
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapBindDN')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapBindDN NVARCHAR(500) NULL;
    PRINT 'Added LdapBindDN column.'
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapBindPassword')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapBindPassword NVARCHAR(256) NULL;
    PRINT 'Added LdapBindPassword column.'
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapAdminGroup')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapAdminGroup NVARCHAR(500) NULL;
    PRINT 'Added LdapAdminGroup column.'
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapUserGroup')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapUserGroup NVARCHAR(500) NULL;
    PRINT 'Added LdapUserGroup column.'
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapFallbackToLocal')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapFallbackToLocal BIT NOT NULL DEFAULT 1;
    PRINT 'Added LdapFallbackToLocal column.'
END
GO

PRINT 'LDAP columns added to Web.AuthSettings.'
GO
