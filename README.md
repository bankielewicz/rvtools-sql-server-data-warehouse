# RVTools Data Warehouse

[![SQL Server](https://img.shields.io/badge/SQL%20Server-2016+-blue.svg)](https://www.microsoft.com/sql-server)
[![PowerShell](https://img.shields.io/badge/PowerShell-5.1+-blue.svg)](https://docs.microsoft.com/powershell)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> Import and warehouse RVTools VMware inventory exports for historical tracking, auditing, and reporting.

## Features

- **Full RVTools Support** - Import all 27 RVTools tabs (~850 columns)
- **SCD Type 2 History** - Track all changes with ValidFrom/ValidTo timestamps
- **Pre-built Views** - 13 reporting views across Inventory, Health, Capacity, and Trends
- **Audit Trail** - Complete import logging with batch tracking and failed record capture

## Quick Start

```powershell
# 1. Install required modules
Install-Module -Name ImportExcel -Scope CurrentUser
Install-Module -Name SqlServer -Scope CurrentUser

# 2. Deploy database (run scripts in order)
# See docs/installation.md for details

# 3. Import RVTools exports
cd src/powershell
.\Import-RVToolsData.ps1 -ServerInstance "localhost" -LogLevel Verbose
```

See [Getting Started](docs/getting-started.md) for detailed setup instructions.

## Documentation

| Section | Description |
|---------|-------------|
| [Getting Started](docs/getting-started.md) | 5-minute quick start guide |
| [Installation](docs/installation.md) | Detailed installation and deployment |
| [Configuration](docs/configuration.md) | Parameters and settings reference |
| **Architecture** | |
| [Overview](docs/architecture/overview.md) | System architecture and design |
| [Database Schema](docs/architecture/database-schema.md) | Schema documentation |
| [Data Flow](docs/architecture/data-flow.md) | ETL process details |
| **Usage** | |
| [Importing Data](docs/usage/importing-data.md) | Running imports |
| [Reports](docs/usage/reports.md) | Available reports and views |
| [Querying Data](docs/usage/querying-data.md) | SQL query examples |
| **Development** | |
| [Contributing](docs/development/contributing.md) | How to contribute |
| [Extending Tables](docs/development/extending-tables.md) | Adding new RVTools tabs |
| [Code Standards](docs/development/code-standards.md) | Naming conventions |
| **Reference** | |
| [RVTools Tabs](docs/reference/rvtools-tabs.md) | All 27 tabs reference |
| [Stored Procedures](docs/reference/stored-procedures.md) | SP documentation |
| [Troubleshooting](docs/reference/troubleshooting.md) | Common issues |

## Requirements

- **SQL Server** 2016 or later
- **PowerShell** 5.1 or later
- **ImportExcel** module
- **SqlServer** module

## Project Structure

```
src/
├── powershell/          # Import scripts and modules
├── tsql/
│   ├── Database/        # Database and schema creation
│   ├── Tables/          # Staging, Current, History tables
│   ├── StoredProcedures/# Import processing logic
│   └── Views/           # Reporting views
└── reports/             # Report definitions
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
