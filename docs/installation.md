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

### 5. Create Stored Procedures

11. `src/tsql/StoredProcedures/usp_RefreshColumnMapping.sql` (creates Config tables)
12. Execute: `EXEC dbo.usp_RefreshColumnMapping` (populates column mapping)
13. Execute remaining stored procedures in `src/tsql/StoredProcedures/`:
    - `usp_ProcessImport.sql`
    - `usp_MergeTable.sql`
    - `usp_PurgeOldHistory.sql`

### 6. Create Views

14. Execute all view scripts in `src/tsql/Views/` subdirectories:
    - `Views/Inventory/*.sql`
    - `Views/Health/*.sql`
    - `Views/Capacity/*.sql`
    - `Views/Trends/*.sql`

### 7. Optional: Enable Transparent Data Encryption (TDE)

15. `src/tsql/Database/005_EnableTDE.sql` (SQL Server Enterprise Edition only)

**Note:** TDE encrypts database files at rest. Requires Enterprise Edition and DBA privileges. See script for detailed instructions and certificate backup requirements.

### 8. Verify Installation

```sql
-- Check all schemas exist (should return 7 rows)
SELECT name FROM sys.schemas
WHERE name IN ('Staging', 'Current', 'History', 'Audit', 'Config', 'Web', 'Reporting')
ORDER BY name;

-- Check table counts
SELECT
    s.name AS SchemaName,
    COUNT(*) AS TableCount
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name IN ('Staging', 'Current', 'History', 'Audit', 'Web', 'Config')
GROUP BY s.name
ORDER BY s.name;
-- Expected: Staging=27, Current=27, History=27, Audit=4, Web=3, Config=3

-- Verify authentication tables exist
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'Web'
ORDER BY TABLE_NAME;
-- Expected: AuthSettings, ErrorLog, Users

-- Check stored procedures
SELECT name FROM sys.procedures WHERE name LIKE 'usp_%' ORDER BY name;

-- Check reporting views
SELECT name FROM sys.views WHERE SCHEMA_NAME(schema_id) = 'Reporting' ORDER BY name;
```

### 9. Deploy Web Application

**The web application is required** for authentication, user management, and admin settings.

See [Authentication Setup Guide](authentication-setup.md) for first-time configuration.

```bash
cd src/web/RVToolsWeb
dotnet restore
dotnet run
```

See [Web Reports](./web-reports.md) for detailed IIS deployment instructions.

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
