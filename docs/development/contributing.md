# Contributing

> How to contribute to the RVTools Data Warehouse project.

**Navigation**: [Home](../../README.md) | [Code Standards](./code-standards.md) | [Extending Tables](./extending-tables.md)

---

## Getting Started

### Prerequisites

- SQL Server 2016+ (local or accessible instance)
- PowerShell 5.1+
- Git
- Text editor or IDE (VS Code recommended)

### Development Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/bankielewicz/RVToolsDW.git
   cd RVToolsDW
   ```

2. Install PowerShell modules:
   ```powershell
   Install-Module -Name ImportExcel -Scope CurrentUser
   Install-Module -Name SqlServer -Scope CurrentUser
   ```

3. Deploy the database to your development SQL Server (see [Installation](../installation.md))

4. Create operational folders:
   ```powershell
   mkdir incoming, processed, errors, failed, logs
   ```

## Branch Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Stable release code |
| `develop` | Integration branch |
| `feature/*` | New features |
| `bugfix/*` | Bug fixes |

### Workflow

1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git pull
   git checkout -b feature/my-feature
   ```

2. Make your changes

3. Commit with clear messages:
   ```bash
   git commit -m "Add new view for cluster capacity analysis"
   ```

4. Push and create a pull request:
   ```bash
   git push -u origin feature/my-feature
   ```

## Pull Request Process

1. **Title**: Clear, concise description
2. **Description**: What and why
3. **Testing**: Describe how you tested
4. **Checklist**:
   - [ ] Code follows [Code Standards](./code-standards.md)
   - [ ] All SQL scripts tested
   - [ ] Documentation updated if needed
   - [ ] No breaking changes (or documented)

## Testing

### SQL Scripts

Test all SQL changes against a development database:

```sql
-- Test in transaction (rollback if issues)
BEGIN TRANSACTION;

-- Your changes here

-- Verify results
SELECT ...

-- Rollback to undo test changes
ROLLBACK;
```

### PowerShell Scripts

Test import functionality:

```powershell
# Use a test xlsx file
.\Import-RVToolsData.ps1 -SingleFile "test-data.xlsx" -LogLevel Verbose
```

### Verify Views

After creating or modifying views:

```sql
-- Check view compiles
SELECT TOP 1 * FROM Reporting.vw_New_View;

-- Verify expected columns
EXEC sp_describe_first_result_set N'SELECT * FROM Reporting.vw_New_View';
```

## Code Review Guidelines

Reviewers should check:

- [ ] SQL scripts follow naming conventions
- [ ] PowerShell follows best practices
- [ ] No hardcoded paths or credentials
- [ ] Error handling in place
- [ ] Performance considerations
- [ ] Documentation updated

## Types of Contributions

### New Views

See [Extending Views](./extending-views.md)

### New RVTools Tabs

See [Extending Tables](./extending-tables.md)

### Bug Fixes

1. Create an issue describing the bug
2. Create a `bugfix/` branch
3. Fix and test
4. Submit pull request referencing the issue

### Documentation

Documentation improvements are welcome:

1. Fork the repository
2. Edit markdown files in `docs/`
3. Submit pull request

---

## Next Steps

- [Code Standards](./code-standards.md) - Naming conventions
- [Extending Tables](./extending-tables.md) - Add new RVTools tabs

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
