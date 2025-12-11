/*
    RVTools Data Warehouse - Transparent Data Encryption (TDE)
    Purpose: Enable encryption at rest for the RVToolsDW database

    Security Fix: SEC-007 - No encryption at rest for VMware infrastructure data

    REQUIREMENTS:
    - SQL Server Enterprise Edition (TDE not available in Standard/Express)
    - sysadmin or dbcreator privileges
    - Backup storage for master key and certificate

    WHAT TDE PROTECTS:
    - Physical database files (.mdf, .ldf)
    - Backup files
    - Storage media theft

    WHAT TDE DOES NOT PROTECT:
    - Data in transit (use TLS for that)
    - Data accessed through SQL queries
    - Application-level attacks

    APPLICATION IMPACT:
    - None - TDE is transparent to applications
    - No connection string changes required
    - No code changes required
    - Slight CPU overhead for encryption/decryption (~3-5%)

    USAGE:
    1. Review and modify paths below for your environment
    2. Execute against master database as sysadmin
    3. Store backup files securely - they are required for recovery!

    RECOVERY:
    If the database is restored to another server, you must first restore:
    1. The Database Master Key
    2. The TDE Certificate
*/

USE [master]
GO

-- ============================================================================
-- Step 1: Create Database Master Key (if not exists)
-- ============================================================================
-- This key protects the TDE certificate
-- Use a strong password and store it securely!

IF NOT EXISTS (SELECT * FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
BEGIN
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'CHANGE_ME_StrongP@ssw0rd_CHANGE_ME';
    PRINT 'Created Database Master Key in master database.'
END
ELSE
BEGIN
    PRINT 'Database Master Key already exists in master database.'
END
GO

-- ============================================================================
-- Step 2: Create TDE Certificate
-- ============================================================================
-- This certificate encrypts the Database Encryption Key

IF NOT EXISTS (SELECT * FROM sys.certificates WHERE name = 'RVToolsDW_TDE_Cert')
BEGIN
    CREATE CERTIFICATE RVToolsDW_TDE_Cert
    WITH SUBJECT = 'RVTools Data Warehouse TDE Certificate';
    PRINT 'Created TDE certificate: RVToolsDW_TDE_Cert'
END
ELSE
BEGIN
    PRINT 'TDE certificate RVToolsDW_TDE_Cert already exists.'
END
GO

-- ============================================================================
-- Step 3: CRITICAL - Backup the Certificate and Master Key!
-- ============================================================================
-- Without these backups, you CANNOT restore the database to another server!
-- Store these files securely, separate from the database backups.

-- MODIFY THESE PATHS for your environment:
-- Change C:\SQLBackups\TDE\ to your secure backup location

/*
-- Uncomment and execute these commands with appropriate paths:

BACKUP CERTIFICATE RVToolsDW_TDE_Cert
TO FILE = 'C:\SQLBackups\TDE\RVToolsDW_TDE_Cert.cer'
WITH PRIVATE KEY (
    FILE = 'C:\SQLBackups\TDE\RVToolsDW_TDE_Cert_PrivateKey.pvk',
    ENCRYPTION BY PASSWORD = 'CHANGE_ME_CertBackupP@ss_CHANGE_ME'
);

BACKUP MASTER KEY
TO FILE = 'C:\SQLBackups\TDE\MasterKey.key'
ENCRYPTION BY PASSWORD = 'CHANGE_ME_MasterKeyBackupP@ss_CHANGE_ME';

PRINT 'CRITICAL: Certificate and Master Key backed up.'
PRINT 'Store these files securely - they are required for disaster recovery!'
*/

-- ============================================================================
-- Step 4: Create Database Encryption Key
-- ============================================================================

USE [RVToolsDW]
GO

IF NOT EXISTS (SELECT * FROM sys.dm_database_encryption_keys WHERE database_id = DB_ID())
BEGIN
    CREATE DATABASE ENCRYPTION KEY
    WITH ALGORITHM = AES_256
    ENCRYPTION BY SERVER CERTIFICATE RVToolsDW_TDE_Cert;
    PRINT 'Created Database Encryption Key with AES-256 encryption.'
END
ELSE
BEGIN
    PRINT 'Database Encryption Key already exists.'
END
GO

-- ============================================================================
-- Step 5: Enable TDE
-- ============================================================================

ALTER DATABASE [RVToolsDW]
SET ENCRYPTION ON;

PRINT 'TDE enabled for RVToolsDW database.'
PRINT 'Initial encryption will happen in the background.'
PRINT 'Monitor progress with: SELECT * FROM sys.dm_database_encryption_keys'
GO

-- ============================================================================
-- Verify TDE Status
-- ============================================================================

SELECT
    db.name AS [Database],
    dek.encryption_state AS [State],
    CASE dek.encryption_state
        WHEN 0 THEN 'No encryption key'
        WHEN 1 THEN 'Unencrypted'
        WHEN 2 THEN 'Encryption in progress'
        WHEN 3 THEN 'Encrypted'
        WHEN 4 THEN 'Key change in progress'
        WHEN 5 THEN 'Decryption in progress'
        WHEN 6 THEN 'Protection change in progress'
    END AS [State Description],
    dek.percent_complete AS [Percent Complete],
    dek.key_algorithm AS [Algorithm],
    dek.key_length AS [Key Length],
    c.name AS [Certificate]
FROM sys.dm_database_encryption_keys dek
JOIN sys.databases db ON dek.database_id = db.database_id
LEFT JOIN sys.certificates c ON dek.encryptor_thumbprint = c.thumbprint
WHERE db.name = 'RVToolsDW';
GO

PRINT ''
PRINT '============================================================'
PRINT 'TDE SETUP COMPLETE'
PRINT '============================================================'
PRINT 'IMPORTANT: Before continuing, ensure you have:'
PRINT '1. Backed up the TDE certificate and private key'
PRINT '2. Backed up the master key'
PRINT '3. Stored backup files in a secure location separate from DB backups'
PRINT '4. Documented the passwords used'
PRINT ''
PRINT 'Without these backups, database recovery to another server'
PRINT 'will be IMPOSSIBLE.'
PRINT '============================================================'
GO
