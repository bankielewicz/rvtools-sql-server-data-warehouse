# Installation Guide

> Complete installation and deployment instructions for RVTools SQL Server Data Warehouse.

**Navigation**: [Home](../README.md) | [Getting Started](./getting-started.md) | [Configuration](./configuration.md)

---

## System Requirements

### SQL Server

| Component | Requirement |
|-----------|-------------|
| Version | SQL Server 2016 or later |
| Edition | Any (Express, Standard, Enterprise) |
| Memory | 4 GB minimum recommended |
| Disk | 10 GB+ depending on data volume |

### PowerShell

| Component | Requirement |
|-----------|-------------|
| Version | PowerShell 5.1 or later |
| Modules | ImportExcel, SqlServer |
| Execution Policy | RemoteSigned or Unrestricted |

### .NET (Optional - for Web Dashboard)

| Component | Requirement |
|-----------|-------------|
| SDK | .NET 8.0 SDK |
| Runtime | ASP.NET Core 8.0 |

## Installation Steps

### 1. Install PowerShell Modules

```powershell
# Check current modules
Get-Module -ListAvailable ImportExcel, SqlServer

# Install if needed
Install-Module -Name ImportExcel -Scope CurrentUser -Force
Install-Module -Name SqlServer -Scope CurrentUser -Force

# Verify installation
Import-Module ImportExcel
Import-Module SqlServer
```

### 2. Create Database

Connect to SQL Server and run:

```sql
-- Run against master database
USE master;
GO

CREATE DATABASE RVToolsDW;
GO
```

Or execute the provided script:

```
src/tsql/Database/001_CreateDatabase.sql
```

### 3. Create Schemas

```sql
-- Run against RVToolsDW
USE RVToolsDW;
GO

-- Execute schema creation script
-- src/tsql/Database/002_CreateSchemas.sql
```

This creates the following schemas:

| Schema | Purpose |
|--------|---------|
| Staging | Raw import data (all NVARCHAR(MAX)) |
| Current | Latest typed snapshot |
| History | SCD Type 2 historical data |
| Audit | Import tracking and logging |
| Config | Settings and configuration |
| Web | Authentication and session tracking |
| Service | Windows Service job management |
| Reporting | Report-facing views |

### 4. Create Tables and Configuration

**IMPORTANT:** Execute SQL scripts in this exact order. Missing steps will break the web application.

1. `src/tsql/Tables/Staging/001_AllStagingTables.sql`
2. `src/tsql/Tables/Staging/002_AddDatacenter2Column.sql`
3. `src/tsql/Tables/Current/001_AllCurrentTables.sql`
4. `src/tsql/Tables/History/001_AllHistoryTables.sql`
5. `src/tsql/Tables/Audit/ErrorLog.sql` and `MergeProgress.sql`
6. `src/tsql/Tables/Web/001_ErrorLog.sql` (web application error logging)
7. `src/tsql/Tables/Web/002_Users.sql` (authentication - user accounts)
8. `src/tsql/Tables/Web/003_AuthSettings.sql` (authentication - provider config)
9. `src/tsql/Tables/Web/004_AuthSettings_LDAP.sql` (LDAP configuration columns)
10. `src/tsql/Tables/Web/005_AuthSettings_CertValidation.sql` (LDAP certificate validation)
11. `src/tsql/Tables/Web/006_Sessions.sql` (authentication audit trail)

### 5. Create Service Schema Tables (Optional - for Windows Service)

12. `src/tsql/Tables/Service/001_Jobs.sql` (creates Service schema + Jobs table)
13. `src/tsql/Tables/Service/002_JobRuns.sql` (execution history)
14. `src/tsql/Tables/Service/003_JobTriggers.sql` (manual trigger queue)
15. `src/tsql/Tables/Service/004_ServiceStatus.sql` (health monitoring)

### 6. Create Stored Procedures

16. `src/tsql/StoredProcedures/usp_RefreshColumnMapping.sql` (creates Config tables)
17. Execute: `EXEC dbo.usp_RefreshColumnMapping` (populates column mapping)
18. Execute remaining stored procedures in `src/tsql/StoredProcedures/`:
    - `usp_ProcessImport.sql`
    - `usp_MergeTable.sql`
    - `usp_PurgeOldHistory.sql`
    - `usp_CleanupStaleSessions.sql` (session cleanup - 90 day retention)

### 7. Create Views

19. Execute all view scripts in `src/tsql/Views/` subdirectories:
    - `Views/Inventory/*.sql`
    - `Views/Health/*.sql`
    - `Views/Capacity/*.sql`
    - `Views/Trends/*.sql`

### 8. Optional: Enable Transparent Data Encryption (TDE)

20. `src/tsql/Database/005_EnableTDE.sql` (SQL Server Enterprise Edition only)

**Note:** TDE encrypts database files at rest. Requires Enterprise Edition and DBA privileges. See script for detailed instructions and certificate backup requirements.

### 9. Optional: Enable Soft-Delete/Tombstone Support

Soft-delete tracks when records are removed from vCenter instead of immediately removing them from Current tables.

21. `src/tsql/Database/006_SoftDeleteSettings.sql` (Config.Settings for soft-delete)
22. `src/tsql/Tables/Current/002_AddSoftDeleteColumns.sql` (adds 6 columns to all 27 Current tables)
23. Execute: `EXEC dbo.usp_RefreshColumnMapping @DebugMode = 1` (register new columns)
24. Re-deploy `src/tsql/StoredProcedures/usp_MergeTable.sql` (SOFT_DELETE, UPDATE_LAST_SEEN steps)
25. `src/tsql/StoredProcedures/usp_ArchiveSoftDeletedRecords.sql` (archive procedure)
26. Re-deploy all views in `src/tsql/Views/` (updated with IsDeleted filter)

See [Soft-Delete Feature](./development/soft-delete.md) for detailed configuration.

### 10. Verify Installation

```sql
-- Check all schemas exist (should return 8 rows)
SELECT name FROM sys.schemas
WHERE name IN ('Staging', 'Current', 'History', 'Audit', 'Config', 'Web', 'Service', 'Reporting')
ORDER BY name;

-- Check table counts
SELECT
    s.name AS SchemaName,
    COUNT(*) AS TableCount
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name IN ('Staging', 'Current', 'History', 'Audit', 'Web', 'Config', 'Service')
GROUP BY s.name
ORDER BY s.name;
-- Expected: Staging=27, Current=27, History=27, Audit=4, Web=4, Config=3, Service=4

-- Verify Web schema tables exist
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'Web'
ORDER BY TABLE_NAME;
-- Expected: AuthSettings, ErrorLog, Sessions, Users

-- Verify Service schema tables exist (if Windows Service deployed)
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'Service'
ORDER BY TABLE_NAME;
-- Expected: Jobs, JobRuns, JobTriggers, ServiceStatus

-- Check stored procedures
SELECT name FROM sys.procedures WHERE name LIKE 'usp_%' ORDER BY name;

-- Check reporting views
SELECT name FROM sys.views WHERE SCHEMA_NAME(schema_id) = 'Reporting' ORDER BY name;
```

### 11. Deploy Web Application

**The web application is required** for authentication, user management, and admin settings.

See [Authentication Setup Guide](authentication-setup.md) for first-time configuration.

```bash
cd src/web/RVToolsWeb
dotnet restore
dotnet run
```

See [Web Reports](./web-reports.md) for detailed IIS deployment instructions.

### 12. Optional: Deploy Windows Service

For automated imports, deploy the Windows Service:

```powershell
# Build the service
cd src/service/RVToolsService
dotnet publish -c Release -o C:\Services\RVToolsService

# Install as Windows Service
sc create RVToolsService binPath="C:\Services\RVToolsService\RVToolsService.exe"
sc start RVToolsService
```

See [Windows Service Guide](./windows-service.md) for detailed configuration.

## Folder Setup

The import process uses these operational folders:

| Folder | Purpose |
|--------|---------|
| `incoming/` | Drop zone for xlsx files to import |
| `processed/` | Successfully imported files (timestamped) |
| `errors/` | Files that failed completely |
| `failed/` | Individual failed records (CSV) |
| `logs/` | Timestamped execution logs |

Create them if they don't exist:

```powershell
$folders = @('incoming', 'processed', 'errors', 'failed', 'logs')
foreach ($folder in $folders) {
    New-Item -ItemType Directory -Path $folder -Force
}
```

## SQL Server Authentication

### Windows Authentication (Default)

```powershell
.\Import-RVToolsData.ps1 -ServerInstance "localhost"
```

### SQL Server Authentication

```powershell
# Will prompt for credentials
.\Import-RVToolsData.ps1 -ServerInstance "localhost" -UseSqlAuth

# With pre-defined credential
$cred = Get-Credential
.\Import-RVToolsData.ps1 -ServerInstance "sqlserver.domain.com" -UseSqlAuth -Credential $cred
```

---

## Next Steps

- [Configuration](./configuration.md) - Customize settings
- [Importing Data](./usage/importing-data.md) - Run your first import
- [Web Reports](./web-reports.md) - Web dashboard setup and deployment

## Need Help?

See [Troubleshooting](./reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
