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

### 4. Create Tables

Execute in order:

```
src/tsql/Tables/Staging/001_AllStagingTables.sql
src/tsql/Tables/Current/001_AllCurrentTables.sql
src/tsql/Tables/History/001_AllHistoryTables.sql
```

### 5. Create Stored Procedures

```
src/tsql/StoredProcedures/usp_ProcessImport.sql
src/tsql/StoredProcedures/usp_MergeTable_vInfo.sql
src/tsql/StoredProcedures/usp_PurgeOldHistory.sql
```

### 6. Create Views

Execute all view scripts:

```
src/tsql/Views/Inventory/*.sql
src/tsql/Views/Health/*.sql
src/tsql/Views/Capacity/*.sql
src/tsql/Views/Trends/*.sql
```

### 7. Verify Installation

```sql
-- Check all schemas exist
SELECT name FROM sys.schemas
WHERE name IN ('Staging', 'Current', 'History', 'Audit', 'Config', 'Reporting');

-- Check table counts (should be 27 each)
SELECT
    s.name AS SchemaName,
    COUNT(*) AS TableCount
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name IN ('Staging', 'Current', 'History')
GROUP BY s.name;

-- Check stored procedures
SELECT name FROM sys.procedures WHERE name LIKE 'usp_%';

-- Check views
SELECT name FROM sys.views WHERE SCHEMA_NAME(schema_id) = 'Reporting';
```

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

## Need Help?

See [Troubleshooting](./reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
