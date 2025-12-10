# Web Reports Application

**Navigation**: [Getting Started](./getting-started.md) | [Architecture Overview](./architecture/overview.md) | [Reports Guide](./usage/reports.md)

---

A modern, browser-based reporting interface for the RVTools Data Warehouse that provides real-time access to your VMware infrastructure data through interactive dashboards and reports.

![Web Dashboard](./images/web-dashboard.png)

## Overview

The Web Reports application is an ASP.NET Core 8.0 MVC application that connects directly to the RVToolsDW database and presents the same 24 reports available through SSRS in a modern, responsive web interface. The application features a unified dashboard with infrastructure summary metrics, health status indicators, and quick navigation to all reports.

## Features

### Dashboard
- **Infrastructure Summary** - At-a-glance counts for vCenters, Datacenters, Clusters, ESXi Hosts, Datastores, and Virtual Machines
- **VM Power State** - Breakdown of powered on, powered off, and template VMs with visual progress indicator
- **Resource Allocation** - Total vCPUs, memory, provisioned storage, and in-use storage across all VMs
- **Storage Capacity** - Datastore health status with capacity utilization and warning/critical thresholds
- **Health Status** - Aggregated health issues, expiring certificates, aging snapshots, and orphaned files

### Reports by Category

**Inventory (7 reports)**
- VM Inventory
- Host Inventory
- Cluster Summary
- Datastore Inventory
- Network Topology
- License Compliance
- Resource Pools

**Health (6 reports)**
- Health Issues
- Certificate Expiration
- Snapshot Aging
- Configuration Compliance
- Orphaned Files
- VMware Tools Status

**Capacity (4 reports)**
- Datastore Capacity
- Host Capacity
- VM Resource Allocation
- VM Right-Sizing

**Trends (6 reports)**
- VM Count Trend
- Storage Growth
- Datastore Capacity Trend
- Host Utilization
- VM Config Changes
- VM Lifecycle

### Interactive Features
- **Sortable & Searchable Tables** - DataTables integration for all tabular reports
- **Interactive Charts** - Chart.js visualizations for trend data and capacity metrics
- **Responsive Design** - Bootstrap 5 layout adapts to desktop, tablet, and mobile screens
- **Dark Sidebar Navigation** - Collapsible sidebar with organized report categories
- **Export Capabilities** - Excel export functionality for report data

## Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | ASP.NET Core 8.0 MVC |
| ORM | Dapper (micro-ORM) |
| Database | SQL Server 2016+ (RVToolsDW) |
| CSS Framework | Bootstrap 5 |
| Icons | Bootstrap Icons |
| Charts | Chart.js |
| Data Tables | DataTables |
| Client Library Manager | LibMan |

## Project Structure

```
src/web/
├── RVToolsWeb.sln
└── RVToolsWeb/
    ├── Configuration/       # App settings models
    ├── Controllers/         # MVC controllers by category
    │   ├── Api/            # REST API endpoints
    │   ├── Capacity/       # Capacity report controllers
    │   ├── Health/         # Health report controllers
    │   ├── Inventory/      # Inventory report controllers
    │   └── Trends/         # Trend report controllers
    ├── Data/               # Data access layer
    │   └── Repositories/   # Dapper repository implementations
    ├── Models/             # Data models and view models
    │   ├── DTOs/          # Data transfer objects
    │   └── ViewModels/    # View-specific models
    ├── Services/           # Business logic services
    ├── Views/              # Razor views by controller
    └── wwwroot/            # Static assets (CSS, JS, libs)
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2016+ with RVToolsDW database deployed
- RVTools data imported via PowerShell ETL pipeline

### Configuration

Update `appsettings.json` with your database connection:

```json
{
  "ConnectionStrings": {
    "RVToolsDW": "Server=localhost;Database=RVToolsDW;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Running the Application

```bash
cd src/web/RVToolsWeb
dotnet restore
dotnet run
```

The application will start on `https://localhost:5001` (or the port configured in `launchSettings.json`).

### Deployment Options

- **IIS** - Publish to IIS with ASP.NET Core hosting bundle
- **Docker** - Container deployment with SQL Server connectivity
- **Azure App Service** - Cloud deployment with Azure SQL Database
- **Self-Contained** - Single-file executable for standalone deployment

## Related Documentation

- [SSRS Reports User Guide](./SSRS_Reports_User_Guide.md) - Original SSRS report documentation
- [Reports Overview](./usage/reports.md) - Report categories and descriptions
- [Database Schema](./architecture/database-schema.md) - Understanding the data model
- [Data Flow](./architecture/data-flow.md) - How data moves from RVTools to reports
