# Extending Views

> How to create new reporting views.

**Navigation**: [Home](../../README.md) | [Extending Tables](./extending-tables.md) | [Code Standards](./code-standards.md)

---

## View Guidelines

### Schema

All reporting views go in the `Reporting` schema:

```sql
CREATE VIEW Reporting.vw_Example AS
...
```

### Naming Convention

| Prefix | Purpose | Example |
|--------|---------|---------|
| `vw_` | All views | `vw_VM_Inventory` |
| `*_Inventory` | Entity listings | `vw_Host_Inventory` |
| `*_Summary` | Aggregated data | `vw_Cluster_Summary` |
| `*_Capacity` | Resource metrics | `vw_Host_Capacity` |
| `*_Trend` | Historical data | `vw_VM_Count_Trend` |
| `*_Status` | State/health | `vw_Tools_Status` |
| `*_Aging` | Time-based | `vw_Snapshot_Aging` |

## Creating a New View

### Step 1: Design the Query

Start with a working query:

```sql
-- Draft query
SELECT
    i.VM,
    i.Powerstate,
    i.CPUs,
    i.Memory,
    h.Host,
    h.[CPU usage %] AS HostCPUUsage
FROM Current.vInfo i
JOIN Current.vHost h ON i.Host = h.Host
    AND i.[VI SDK Server] = h.[VI SDK Server]
WHERE i.Powerstate = 'poweredOn';
```

### Step 2: Create the View

```sql
-- src/tsql/Views/Inventory/vw_VM_Host_Details.sql

CREATE VIEW Reporting.vw_VM_Host_Details
AS
SELECT
    i.VM,
    i.Powerstate,
    i.CPUs,
    i.Memory,
    i.Host,
    i.Cluster,
    i.Datacenter,
    h.[CPU usage %] AS HostCPUUsage,
    h.[Memory usage %] AS HostMemoryUsage,
    h.[# VMs] AS HostVMCount
FROM Current.vInfo i
JOIN Current.vHost h ON i.Host = h.Host
    AND i.[VI SDK Server] = h.[VI SDK Server]
WHERE i.Powerstate = 'poweredOn';
GO
```

### Step 3: File Location

Place the view in the appropriate category folder:

```
src/tsql/Views/
├── Inventory/     # Entity listings
├── Health/        # Issues, compliance
├── Capacity/      # Utilization metrics
└── Trends/        # Historical analysis
```

### Step 4: Deploy and Test

```sql
-- Deploy
:r src/tsql/Views/Inventory/vw_VM_Host_Details.sql

-- Test
SELECT TOP 10 * FROM Reporting.vw_VM_Host_Details;

-- Verify columns
EXEC sp_describe_first_result_set
    N'SELECT * FROM Reporting.vw_VM_Host_Details';
```

## View Templates

### Inventory View

```sql
CREATE VIEW Reporting.vw_[Entity]_Inventory
AS
SELECT
    -- Primary identifier
    t.Name,
    -- Key attributes
    t.Attribute1,
    t.Attribute2,
    -- Location
    t.Datacenter,
    t.Cluster,
    -- Metadata
    t.VISDKServer AS VCenter
FROM Current.v[Entity] t;
GO
```

### Capacity View

```sql
CREATE VIEW Reporting.vw_[Entity]_Capacity
AS
SELECT
    t.Name,
    -- Capacity metrics
    t.TotalCapacity,
    t.UsedCapacity,
    t.TotalCapacity - t.UsedCapacity AS FreeCapacity,
    CAST(t.UsedCapacity AS FLOAT) / NULLIF(t.TotalCapacity, 0) * 100 AS UsedPercent,
    -- Thresholds
    CASE
        WHEN CAST(t.UsedCapacity AS FLOAT) / NULLIF(t.TotalCapacity, 0) > 0.85 THEN 'Critical'
        WHEN CAST(t.UsedCapacity AS FLOAT) / NULLIF(t.TotalCapacity, 0) > 0.70 THEN 'Warning'
        ELSE 'OK'
    END AS Status
FROM Current.v[Entity] t;
GO
```

### Trend View

```sql
CREATE VIEW Reporting.vw_[Entity]_Trend
AS
SELECT
    CAST(h.ValidFrom AS DATE) AS SnapshotDate,
    COUNT(*) AS RecordCount,
    SUM(h.MetricColumn) AS TotalMetric,
    AVG(h.MetricColumn) AS AvgMetric
FROM History.v[Entity] h
GROUP BY CAST(h.ValidFrom AS DATE);
GO
```

### Health View

```sql
CREATE VIEW Reporting.vw_[Entity]_Issues
AS
SELECT
    t.Name,
    t.IssueColumn AS Issue,
    CASE
        WHEN t.Severity > 2 THEN 'Error'
        WHEN t.Severity > 1 THEN 'Warning'
        ELSE 'Info'
    END AS Severity,
    t.VISDKServer AS VCenter
FROM Current.v[Entity] t
WHERE t.IssueColumn IS NOT NULL;
GO
```

## Best Practices

### Column Naming

- Use clear, descriptive names
- Avoid abbreviations
- Include units where applicable: `MemoryMiB`, `CapacityGiB`

### Performance

- Avoid functions in WHERE clauses
- Use appropriate JOINs (INNER vs LEFT)
- Consider indexed columns for filters

### Multi-vCenter Support

Always join on both entity key AND VI SDK Server:

```sql
-- Correct
JOIN Current.vHost h ON i.Host = h.Host
    AND i.[VI SDK Server] = h.[VI SDK Server]

-- Incorrect (may join wrong entities)
JOIN Current.vHost h ON i.Host = h.Host
```

### NULL Handling

Use ISNULL or COALESCE for nullable columns:

```sql
ISNULL(t.OptionalColumn, 'N/A') AS OptionalColumn
```

## Checklist

- [ ] View uses `Reporting` schema
- [ ] Name follows `vw_` convention
- [ ] File in correct category folder
- [ ] Joins include VI SDK Server
- [ ] Column names are clear
- [ ] NULL values handled appropriately
- [ ] Tested with sample data

---

## Next Steps

- [Extending Reports](./extending-reports.md) - Create SSRS reports
- [Code Standards](./code-standards.md) - Naming conventions

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
