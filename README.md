# RVTools SQL Server Data Warehouse

[![SQL Server](https://img.shields.io/badge/SQL%20Server-2016+-CC2927.svg?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![PowerShell](https://img.shields.io/badge/PowerShell-5.1+-5391FE.svg?logo=powershell)](https://docs.microsoft.com/powershell)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4.svg?logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/bankielewicz/rvtools-sql-server-data-warehouse?style=social)](https://github.com/bankielewicz/rvtools-sql-server-data-warehouse/stargazers)

> **SQL Server ETL solution that imports RVTools VMware inventory exports into a relational data warehouse with SCD Type 2 historical tracking, automated PowerShell ETL pipeline, and 24 pre-built reports via web dashboard or SSRS.**

![Web Dashboard](docs/images/web-dashboard.png)

**Keywords**: RVTools | SQL Server Data Warehouse | VMware Inventory | vCenter | ESXi | PowerShell ETL | ASP.NET Core | Web Dashboard | SSRS Reports | Historical Tracking | VMware Analytics | Infrastructure Monitoring

---

⭐ **If you find this project useful, please star this repository!** It helps others discover this solution.

## Features

- **Full RVTools Support** - Import all 27 RVTools tabs (~850 columns)
- **SCD Type 2 History** - Track all changes with ValidFrom/ValidTo timestamps
- **Web Dashboard** - Modern ASP.NET Core 8.0 browser-based reporting with interactive charts and real-time data
- **Pre-built Reports** - 24 reports across Inventory, Health, Capacity, and Trends via web interface or SSRS
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

# Windows Authentication
.\Import-RVToolsData.ps1 -ServerInstance "localhost" -LogLevel Verbose

# SQL Authentication (will prompt for credentials)
.\Import-RVToolsData.ps1 -ServerInstance "localhost" -UseSqlAuth -LogLevel Verbose
```

### Historical Import (for bulk historical data)

If you have historical RVTools exports with dates in filenames:

```powershell
# Preview files without importing
.\Import-RVToolsHistoricalData.ps1 -WhatIf

# Import historical files (processes oldest first)
.\Import-RVToolsHistoricalData.ps1 -ServerInstance "localhost"
```

Filename pattern: `vCenter{xx}_{d_mm_yyyy}.domain.com.xlsx`

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
| [Historical Import](docs/usage/importing-data.md#historical-import) | Import bulk historical data |
| [Web Reports](docs/web-reports.md) | Web dashboard and browser-based reports |
| [Reports](docs/usage/reports.md) | Available reports and views |
| [Querying Data](docs/usage/querying-data.md) | SQL query examples |
| [SSRS Reports Guide](docs/SSRS_Reports_User_Guide.md) | Complete SSRS report reference |
| **Development** | |
| [Contributing](docs/development/contributing.md) | How to contribute |
| [Extending Tables](docs/development/extending-tables.md) | Adding new RVTools tabs |
| [Code Standards](docs/development/code-standards.md) | Naming conventions |
| **Reference** | |
| [RVTools Tabs](docs/reference/rvtools-tabs.md) | All 27 tabs reference |
| [Stored Procedures](docs/reference/stored-procedures.md) | SP documentation |
| [Troubleshooting](docs/reference/troubleshooting.md) | Common issues |

## Requirements

- **SQL Server** 2016 or later (Express, Standard, or Enterprise)
- **PowerShell** 5.1 or later
- **ImportExcel** PowerShell module
- **SqlServer** PowerShell module
- **RVTools** 4.x (for generating VMware exports)
- **.NET 8.0 SDK** (for web application, optional)

## Project Structure

```
src/
├── powershell/          # Import scripts and modules
├── tsql/
│   ├── Database/        # Database and schema creation
│   ├── Tables/          # Staging, Current, History tables
│   ├── StoredProcedures/# Import processing logic
│   └── Views/           # Reporting views
├── reports/             # SSRS report definitions (.rdl)
└── web/                 # ASP.NET Core web application
    └── RVToolsWeb/      # Web dashboard and reports
```

## Use Cases

This data warehouse solution is ideal for:

- **VMware Infrastructure Auditing** - Track all changes to VMs, hosts, and datastores
- **Capacity Planning** - Analyze historical trends in CPU, memory, and storage usage
- **Compliance Reporting** - Generate audit-ready reports with complete change history
- **Cost Optimization** - Identify oversized VMs and unused resources
- **Disaster Recovery Planning** - Maintain historical snapshots of your VMware environment
- **Multi-vCenter Management** - Consolidate data from multiple vCenter servers

## Technology Stack

- **Database**: SQL Server 2016+ with three-schema pattern (Staging, Current, History)
- **ETL**: PowerShell 5.1+ with ImportExcel module for xlsx parsing
- **Web Reporting**: ASP.NET Core 8.0 with Bootstrap 5, Chart.js, and DataTables
- **SSRS Reporting**: SQL Server Reporting Services with RDL report definitions
- **Architecture**: SCD Type 2 for historical tracking with ValidFrom/ValidTo timestamps

## Contributing

Contributions are welcome! Please see [Contributing Guide](docs/development/contributing.md) for details on:
- Extending tables for new RVTools tabs
- Adding custom views and reports
- Code standards and naming conventions

## Support

- **Issues**: [Report bugs or request features](https://github.com/bankielewicz/rvtools-sql-server-data-warehouse/issues)
- **Discussions**: [Ask questions and share ideas](https://github.com/bankielewicz/rvtools-sql-server-data-warehouse/discussions)
- **Documentation**: Comprehensive guides in [/docs](docs/)

## Related Projects

- [RVTools](https://www.robware.net/rvtools/) - Official RVTools by Robware
- See also: VMware PowerCLI, vRealize Operations, vCenter Server

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**⭐ Star this repository** | **🔀 Fork it** | **📢 Share it**

*Built for VMware administrators who need historical tracking and reporting on their virtual infrastructure.*
