# Getting Started

> Get RVTools SQL Server Data Warehouse running in 5 minutes.

**Navigation**: [Home](../README.md) | [Installation](./installation.md) | [Configuration](./configuration.md)

---

## Prerequisites

Before you begin, ensure you have:

- [ ] SQL Server 2016 or later (Express edition works)
- [ ] PowerShell 5.1 or later
- [ ] RVTools export file (.xlsx format)

## Step 1: Install PowerShell Modules

```powershell
Install-Module -Name ImportExcel -Scope CurrentUser
Install-Module -Name SqlServer -Scope CurrentUser
```

## Step 2: Deploy Database

Run the SQL scripts in order against your SQL Server:

```sql
-- Against master database
src/tsql/Database/001_CreateDatabase.sql

-- Against RVToolsDW database
src/tsql/Database/002_CreateSchemas.sql
src/tsql/Tables/Staging/001_AllStagingTables.sql
src/tsql/Tables/Current/001_AllCurrentTables.sql
src/tsql/Tables/History/001_AllHistoryTables.sql
src/tsql/StoredProcedures/*.sql
src/tsql/Views/*/*.sql
```

## Step 3: Run Your First Import

```powershell
# Place your RVTools export in the incoming folder
Copy-Item "path\to\export.xlsx" -Destination "incoming\"

# Run the import (Windows Authentication)
cd src/powershell
.\Import-RVToolsData.ps1 -ServerInstance "localhost" -LogLevel Verbose

# Or with SQL Authentication (will prompt for credentials)
.\Import-RVToolsData.ps1 -ServerInstance "localhost" -UseSqlAuth -LogLevel Verbose
```

## Step 4: Verify

```sql
-- Check imported data
SELECT COUNT(*) AS VMCount FROM Current.vInfo;
SELECT COUNT(*) AS HostCount FROM Current.vHost;

-- View import history
SELECT * FROM Audit.ImportBatch ORDER BY StartTime DESC;
```

## What Happens During Import

1. **Read** - PowerShell reads all sheets from the xlsx file
2. **Stage** - Data loaded into Staging schema (all text columns)
3. **Merge** - Stored procedures validate and merge into Current schema
4. **History** - Changes tracked in History schema with SCD Type 2
5. **Archive** - Processed file moved to `processed/` folder

---

## Next Steps

- [Installation Guide](./installation.md) - Detailed setup instructions
- [Configuration](./configuration.md) - Customize settings
- [Importing Data](./usage/importing-data.md) - Advanced import options

## Need Help?

See [Troubleshooting](./reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
