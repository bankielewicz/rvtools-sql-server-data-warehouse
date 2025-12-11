/*
    RVTools Data Warehouse - LDAP Certificate Validation Settings
    Purpose: Add certificate validation columns to Web.AuthSettings

    Security Fix: SEC-001 - LDAP Certificate Validation Bypass

    Usage: Execute against RVToolsDW database after 004_AuthSettings_LDAP.sql

    New Columns:
    - LdapValidateCertificate: Whether to validate SSL certificates (default: true for security)
    - LdapCertificateThumbprint: Optional thumbprint for self-signed cert pinning
*/

USE [RVToolsDW]
GO

-- Add certificate validation flag
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapValidateCertificate')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapValidateCertificate BIT NOT NULL DEFAULT 1;
    PRINT 'Added LdapValidateCertificate column (default: enabled for security).'
END
GO

-- Add certificate thumbprint for pinning (optional, for self-signed certs)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Web.AuthSettings') AND name = 'LdapCertificateThumbprint')
BEGIN
    ALTER TABLE Web.AuthSettings ADD LdapCertificateThumbprint NVARCHAR(128) NULL;
    PRINT 'Added LdapCertificateThumbprint column for certificate pinning.'
END
GO

PRINT 'Certificate validation columns added to Web.AuthSettings.'
GO
