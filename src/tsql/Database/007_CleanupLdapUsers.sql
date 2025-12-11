/*
    RVTools Data Warehouse - Cleanup LDAP Users Migration

    Purpose: Removes LDAP users from Web.Users table after new transient auth is deployed

    IMPORTANT: Run this script AFTER deploying the new code that handles LDAP users
               without database records. LDAP users must be able to authenticate
               transiently before this migration is executed.

    Pre-requisites:
        1. Web.Sessions table must exist (006_Sessions.sql deployed)
        2. New authentication code deployed and tested
        3. LDAP users can login without Web.Users records
        4. Backup of Web.Users table taken

    Usage:
        -- Step 1: Review what will be deleted (DRY RUN)
        -- Execute the SELECT statements below first

        -- Step 2: Run the full script if satisfied with the preview
*/

USE [RVToolsDW]
GO

PRINT '=== LDAP Users Cleanup Migration ==='
PRINT ''

-- Step 1: Show current LDAP user count
PRINT 'Current LDAP users in Web.Users:'
SELECT
    UserId,
    Username,
    Email,
    Role,
    IsActive,
    LastLoginDate,
    CreatedDate
FROM [Web].[Users]
WHERE PasswordHash = 'LDAP_USER_NO_PASSWORD'
ORDER BY Username;

DECLARE @LdapUserCount INT;
SELECT @LdapUserCount = COUNT(*)
FROM [Web].[Users]
WHERE PasswordHash = 'LDAP_USER_NO_PASSWORD';

PRINT ''
PRINT 'Total LDAP users to remove: ' + CAST(@LdapUserCount AS VARCHAR(10))
PRINT ''

-- Step 2: Create backup of LDAP users before deletion
IF OBJECT_ID('tempdb..#LdapUsersBackup') IS NOT NULL
    DROP TABLE #LdapUsersBackup;

SELECT
    UserId,
    Username,
    Email,
    Role,
    IsActive,
    LastLoginDate,
    CreatedDate,
    ModifiedDate
INTO #LdapUsersBackup
FROM [Web].[Users]
WHERE PasswordHash = 'LDAP_USER_NO_PASSWORD';

PRINT 'Backup created in #LdapUsersBackup'
PRINT ''

-- Step 3: Delete LDAP users from Web.Users
PRINT 'Deleting LDAP users...'

DELETE FROM [Web].[Users]
WHERE PasswordHash = 'LDAP_USER_NO_PASSWORD';

DECLARE @DeletedCount INT = @@ROWCOUNT;
PRINT 'Deleted ' + CAST(@DeletedCount AS VARCHAR(10)) + ' LDAP users'
PRINT ''

-- Step 4: Show remaining LocalDB users
PRINT 'Remaining LocalDB users in Web.Users:'
SELECT
    UserId,
    Username,
    Email,
    Role,
    IsActive,
    CreatedDate
FROM [Web].[Users]
ORDER BY Username;

DECLARE @RemainingCount INT;
SELECT @RemainingCount = COUNT(*) FROM [Web].[Users];

PRINT ''
PRINT 'Remaining LocalDB users: ' + CAST(@RemainingCount AS VARCHAR(10))
PRINT ''

-- Step 5: Log the migration
INSERT INTO [Audit].[ImportLog] (LogLevel, Message)
VALUES ('Info', 'Migration: Removed ' + CAST(@DeletedCount AS VARCHAR(10)) +
        ' LDAP users from Web.Users. ' + CAST(@RemainingCount AS VARCHAR(10)) +
        ' LocalDB users remain.');

-- Step 6: Summary
PRINT '=== Migration Complete ==='
PRINT 'LDAP users removed: ' + CAST(@DeletedCount AS VARCHAR(10))
PRINT 'LocalDB users remaining: ' + CAST(@RemainingCount AS VARCHAR(10))
PRINT ''
PRINT 'LDAP users will now authenticate transiently without database records.'
PRINT 'All authentication events are tracked in Web.Sessions table.'
PRINT ''

-- Cleanup temp table
DROP TABLE #LdapUsersBackup;
GO
