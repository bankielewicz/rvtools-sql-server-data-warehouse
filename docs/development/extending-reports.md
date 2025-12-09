# Extending Reports

> How to create new SSRS reports.

**Navigation**: [Home](../../README.md) | [Extending Views](./extending-views.md) | [Code Standards](./code-standards.md)

---

## Overview

Reports in this project are designed to work with SQL Server Reporting Services (SSRS). Each report uses views from the Reporting schema as data sources.

## Report Structure

```
src/reports/
├── Inventory/      # Infrastructure listings
├── Health/         # Issues and compliance
├── Capacity/       # Resource utilization
└── Trends/         # Historical analysis
```

## Creating a New Report

### Step 1: Create the Underlying View

Reports should query views, not tables directly. See [Extending Views](./extending-views.md).

### Step 2: Create Report Definition

Create an .rdl file using:
- SQL Server Data Tools (SSDT)
- Report Builder
- Visual Studio with SSRS extensions

### Step 3: Configure Data Source

Use a shared data source pointing to RVToolsDW:

```xml
<DataSource Name="RVToolsDW">
  <ConnectionProperties>
    <DataProvider>SQL</DataProvider>
    <ConnectString>Data Source=.;Initial Catalog=RVToolsDW</ConnectString>
  </ConnectionProperties>
</DataSource>
```

### Step 4: Create Dataset

Query the reporting view:

```sql
SELECT * FROM Reporting.vw_VM_Inventory
WHERE (@Datacenter IS NULL OR Datacenter = @Datacenter)
ORDER BY VM
```

### Step 5: Add Parameters

Common parameters:

| Parameter | Type | Purpose |
|-----------|------|---------|
| @Datacenter | String | Filter by datacenter |
| @Cluster | String | Filter by cluster |
| @StartDate | DateTime | Date range start |
| @EndDate | DateTime | Date range end |
| @VCenter | String | Filter by vCenter |

### Step 6: Design Layout

Typical report sections:

1. **Header** - Report title, parameters, run date
2. **Body** - Data table or chart
3. **Footer** - Page numbers, data source

## Report Templates

### List Report

For entity listings (VMs, Hosts, etc.):

```
┌─────────────────────────────────────────┐
│ [Report Title]                          │
│ Generated: [DateTime] | Filters: [...] │
├─────────────────────────────────────────┤
│ Name  │ Col1  │ Col2  │ Col3  │ Col4   │
├───────┼───────┼───────┼───────┼────────┤
│ ...   │ ...   │ ...   │ ...   │ ...    │
└─────────────────────────────────────────┘
```

### Summary Report

For aggregated data:

```
┌─────────────────────────────────────────┐
│ [Report Title]                          │
├─────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐            │
│  │ Metric 1 │  │ Metric 2 │            │
│  │  [123]   │  │  [456]   │            │
│  └──────────┘  └──────────┘            │
├─────────────────────────────────────────┤
│ [Detail Table]                          │
└─────────────────────────────────────────┘
```

### Trend Report

For time-series data:

```
┌─────────────────────────────────────────┐
│ [Report Title]                          │
│ Period: [StartDate] to [EndDate]        │
├─────────────────────────────────────────┤
│         [Line/Bar Chart]                │
│  ▲                                      │
│  │    ___/                              │
│  │___/                                  │
│  └───────────────────────▶              │
├─────────────────────────────────────────┤
│ [Data Table]                            │
└─────────────────────────────────────────┘
```

## Report Categories

### Inventory Reports

**Purpose**: List infrastructure entities

**Common datasets**:
- `vw_VM_Inventory`
- `vw_Host_Inventory`
- `vw_Datastore_Inventory`
- `vw_Cluster_Summary`

### Health Reports

**Purpose**: Identify issues

**Common datasets**:
- `vw_Health_Issues`
- `vw_Snapshot_Aging`
- `vw_Tools_Status`

### Capacity Reports

**Purpose**: Resource utilization

**Common datasets**:
- `vw_Host_Capacity`
- `vw_Datastore_Capacity`
- `vw_VM_Resource_Allocation`

### Trend Reports

**Purpose**: Historical analysis

**Common datasets**:
- `vw_VM_Count_Trend`
- `vw_Datastore_Capacity_Trend`
- `vw_VM_Config_Changes`

## Deployment

### Manual Deployment

1. Open Report Manager (http://server/Reports)
2. Navigate to target folder
3. Upload .rdl file
4. Configure data source

### Script Deployment

```powershell
$reportPath = "src/reports/Inventory/VM_Inventory.rdl"
$targetFolder = "/RVToolsDW/Inventory"

# Using rs.exe
rs -i DeployReport.rss -s http://server/reportserver -v report="$reportPath" -v folder="$targetFolder"
```

## Best Practices

### Performance

- Use parameters to filter large datasets
- Include default date ranges
- Consider pagination for large lists

### Usability

- Include clear titles
- Show active filters
- Add export options (Excel, PDF)
- Include run timestamp

### Maintenance

- Document parameter dependencies
- Use shared data sources
- Version control .rdl files

## Checklist

- [ ] Underlying view exists
- [ ] Report definition (.rdl) created
- [ ] Data source configured
- [ ] Parameters added
- [ ] Layout designed
- [ ] Tested with sample data
- [ ] Deployed to report server

---

## Next Steps

- [Code Standards](./code-standards.md) - Naming conventions
- [Reports](../usage/reports.md) - Available reports

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
