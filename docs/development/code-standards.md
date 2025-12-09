# Code Standards

> Naming conventions and coding standards.

**Navigation**: [Home](../../README.md) | [Contributing](./contributing.md) | [Extending Tables](./extending-tables.md)

---

## SQL Naming Conventions

### Schemas

| Schema | Purpose |
|--------|---------|
| `Staging` | Raw import data |
| `Current` | Current state |
| `History` | Historical tracking |
| `Audit` | Import logging |
| `Config` | Settings |
| `Reporting` | Views for reports |

### Tables

| Pattern | Example | Use |
|---------|---------|-----|
| `v[TabName]` | `vInfo`, `vHost` | RVTools tab data |
| `[Purpose]` | `ImportBatch`, `Settings` | Support tables |

### Columns

- Match RVTools column names in Staging
- Use PascalCase for derived columns
- Include units: `CapacityMiB`, `MemoryGiB`
- Standard tracking columns:
  - `RowId` - Identity primary key
  - `ImportBatchId` - Links to import
  - `CreatedDate`, `ModifiedDate` - Timestamps
  - `ValidFrom`, `ValidTo` - History tracking

### Stored Procedures

| Prefix | Purpose | Example |
|--------|---------|---------|
| `usp_` | User stored procedure | `usp_ProcessImport` |
| `usp_MergeTable_` | Table-specific merge | `usp_MergeTable_vInfo` |
| `usp_Purge` | Data cleanup | `usp_PurgeOldHistory` |

### Views

| Prefix | Category | Example |
|--------|----------|---------|
| `vw_` | All views | `vw_VM_Inventory` |
| `*_Inventory` | Entity listings | `vw_Host_Inventory` |
| `*_Summary` | Aggregated | `vw_Cluster_Summary` |
| `*_Capacity` | Utilization | `vw_Host_Capacity` |
| `*_Trend` | Historical | `vw_VM_Count_Trend` |
| `*_Status` | State/health | `vw_Tools_Status` |

### Indexes

```sql
-- Primary key
PK_[TableName]

-- Unique index
IX_[TableName]_[Column(s)]

-- Non-unique index
IX_[TableName]_[Column(s)]

-- Examples
PK_vInfo
IX_vInfo_NaturalKey
IX_vInfo_Host_Cluster
```

## SQL Formatting

### Keywords

- Use UPPERCASE for SQL keywords
- Use lowercase for functions

```sql
-- Good
SELECT
    VM,
    ISNULL(Host, 'Unknown') AS Host
FROM Current.vInfo
WHERE Powerstate = 'poweredOn'
ORDER BY VM;

-- Avoid
select vm, isnull(host, 'Unknown') as host
from current.vinfo where powerstate = 'poweredOn' order by vm;
```

### Indentation

- 4 spaces (no tabs)
- Indent subqueries and CTEs

```sql
WITH VMCounts AS (
    SELECT
        Cluster,
        COUNT(*) AS VMCount
    FROM Current.vInfo
    GROUP BY Cluster
)
SELECT
    vc.Cluster,
    vc.VMCount
FROM VMCounts vc
ORDER BY vc.VMCount DESC;
```

### JOINs

- Put JOIN type on its own line
- Align ON clause

```sql
SELECT
    i.VM,
    h.Host
FROM Current.vInfo i
INNER JOIN Current.vHost h
    ON i.Host = h.Host
    AND i.[VI SDK Server] = h.[VI SDK Server]
```

### Comments

```sql
-- Single line comment

/*
    Multi-line comment
    for longer explanations
*/

-- Section headers
-- ============================================================================
-- Step 1: Prepare data
-- ============================================================================
```

## PowerShell Standards

### Naming

| Type | Convention | Example |
|------|------------|---------|
| Functions | Verb-Noun | `Import-SheetToStaging` |
| Variables | camelCase | `$importBatchId` |
| Parameters | PascalCase | `-ServerInstance` |
| Constants | UPPER_SNAKE | `$MAX_BATCH_SIZE` |

### Formatting

```powershell
# Function template
function Import-RVToolsData {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$ServerInstance = "localhost",

        [Parameter(Mandatory = $false)]
        [string]$Database = "RVToolsDW"
    )

    begin {
        # Initialization
    }

    process {
        # Main logic
    }

    end {
        # Cleanup
    }
}
```

### Error Handling

```powershell
try {
    # Risky operation
    $result = Invoke-Sqlcmd -Query $query
}
catch {
    Write-Error "Failed to execute query: $_"
    throw
}
finally {
    # Cleanup if needed
}
```

## File Organization

### SQL Files

```
src/tsql/
├── Database/
│   ├── 001_CreateDatabase.sql
│   └── 002_CreateSchemas.sql
├── Tables/
│   ├── Staging/
│   │   └── 001_AllStagingTables.sql
│   ├── Current/
│   │   └── 001_AllCurrentTables.sql
│   └── History/
│       └── 001_AllHistoryTables.sql
├── StoredProcedures/
│   └── usp_*.sql
└── Views/
    ├── Inventory/
    ├── Health/
    ├── Capacity/
    └── Trends/
```

### PowerShell Files

```
src/powershell/
├── Import-RVToolsData.ps1    # Main entry point
└── modules/
    └── RVToolsImport.psm1    # Shared functions
```

## Version Control

### Commit Messages

```
Add new view for cluster capacity analysis

- Create vw_Cluster_Capacity view
- Add indexes for performance
- Update documentation

Fixes #123
```

### File Changes

- One logical change per commit
- Include related files together
- Reference issues when applicable

---

## Next Steps

- [Contributing](./contributing.md) - Contribution workflow
- [Extending Tables](./extending-tables.md) - Add new tables

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
