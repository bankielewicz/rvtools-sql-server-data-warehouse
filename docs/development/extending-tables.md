# Extending Tables

> How to add support for new RVTools tabs.

**Navigation**: [Home](../../README.md) | [Contributing](./contributing.md) | [Extending Views](./extending-views.md)

---

## Overview

Adding a new RVTools tab requires changes in three layers:

1. **Staging** - Raw import table (all NVARCHAR(MAX))
2. **Current** - Typed table with proper data types
3. **History** - SCD Type 2 tracking table
4. **PowerShell** - Sheet import mapping
5. **Stored Procedure** - MERGE logic

## Step-by-Step Guide

### Example: Adding a new tab "vExample"

Assume the tab has columns: Name, Value, Count, Description

### Step 1: Create Staging Table

```sql
-- src/tsql/Tables/Staging/Staging.vExample.sql

CREATE TABLE Staging.vExample (
    ImportBatchId INT NOT NULL,
    Name NVARCHAR(MAX),
    Value NVARCHAR(MAX),
    Count NVARCHAR(MAX),
    Description NVARCHAR(MAX),
    [VI SDK Server] NVARCHAR(MAX),
    [VI SDK UUID] NVARCHAR(MAX)
);
```

**Key points:**
- All columns are NVARCHAR(MAX)
- Include ImportBatchId
- Include VI SDK Server/UUID for multi-vCenter support

### Step 2: Create Current Table

```sql
-- src/tsql/Tables/Current/Current.vExample.sql

CREATE TABLE Current.vExample (
    RowId INT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Value NVARCHAR(500),
    Count INT,
    Description NVARCHAR(MAX),
    VISDKServer NVARCHAR(255),
    VISDKServerUUID NVARCHAR(100),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE()
);

-- Create index on natural key
CREATE UNIQUE INDEX IX_vExample_NaturalKey
ON Current.vExample (Name, VISDKServer);
```

**Key points:**
- Add RowId identity primary key
- Use appropriate data types
- Create unique index on natural key

### Step 3: Create History Table

```sql
-- src/tsql/Tables/History/History.vExample.sql

CREATE TABLE History.vExample (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    RowId INT NOT NULL,
    ImportBatchId INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Value NVARCHAR(500),
    Count INT,
    Description NVARCHAR(MAX),
    VISDKServer NVARCHAR(255),
    VISDKServerUUID NVARCHAR(100),
    ValidFrom DATETIME2 NOT NULL,
    ValidTo DATETIME2 NULL,
    IsCurrent AS CAST(CASE WHEN ValidTo IS NULL THEN 1 ELSE 0 END AS BIT)
);

-- Index for current records
CREATE INDEX IX_vExample_IsCurrent
ON History.vExample (IsCurrent) WHERE ValidTo IS NULL;

-- Index for historical lookups
CREATE INDEX IX_vExample_DateRange
ON History.vExample (RowId, ValidFrom, ValidTo);
```

**Key points:**
- Same columns as Current plus ValidFrom/ValidTo
- IsCurrent computed column
- Indexes for query performance

### Step 4: Update PowerShell Import

Add sheet mapping in `src/powershell/modules/RVToolsImport.psm1`:

```powershell
$sheetMappings = @{
    # Existing mappings...
    'vExample' = @{
        StagingTable = 'Staging.vExample'
        Columns = @(
            'Name',
            'Value',
            'Count',
            'Description',
            'VI SDK Server',
            'VI SDK UUID'
        )
    }
}
```

### Step 5: Create Merge Stored Procedure

```sql
-- src/tsql/StoredProcedures/usp_MergeTable_vExample.sql

CREATE PROCEDURE usp_MergeTable_vExample
    @ImportBatchId INT,
    @ImportTime DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Table to track changed rows
    DECLARE @Changes TABLE (
        Action NVARCHAR(10),
        RowId INT
    );

    -- MERGE into Current
    MERGE Current.vExample AS target
    USING (
        SELECT
            Name,
            Value,
            TRY_CAST(Count AS INT) AS Count,
            Description,
            [VI SDK Server] AS VISDKServer,
            [VI SDK UUID] AS VISDKServerUUID
        FROM Staging.vExample
        WHERE ImportBatchId = @ImportBatchId
          AND Name IS NOT NULL
    ) AS source
    ON target.Name = source.Name
       AND target.VISDKServer = source.VISDKServer

    WHEN MATCHED AND (
        ISNULL(target.Value, '') <> ISNULL(source.Value, '')
        OR ISNULL(target.Count, 0) <> ISNULL(source.Count, 0)
        OR ISNULL(target.Description, '') <> ISNULL(source.Description, '')
    )
    THEN UPDATE SET
        ImportBatchId = @ImportBatchId,
        Value = source.Value,
        Count = source.Count,
        Description = source.Description,
        ModifiedDate = @ImportTime

    WHEN NOT MATCHED BY TARGET
    THEN INSERT (ImportBatchId, Name, Value, Count, Description,
                 VISDKServer, VISDKServerUUID, CreatedDate, ModifiedDate)
    VALUES (@ImportBatchId, source.Name, source.Value, source.Count,
            source.Description, source.VISDKServer, source.VISDKServerUUID,
            @ImportTime, @ImportTime)

    OUTPUT $action, INSERTED.RowId INTO @Changes;

    -- Close old history records
    UPDATE History.vExample
    SET ValidTo = @ImportTime
    WHERE RowId IN (SELECT RowId FROM @Changes WHERE Action = 'UPDATE')
      AND ValidTo IS NULL;

    -- Insert new history records
    INSERT INTO History.vExample (
        RowId, ImportBatchId, Name, Value, Count, Description,
        VISDKServer, VISDKServerUUID, ValidFrom, ValidTo
    )
    SELECT
        c.RowId, c.ImportBatchId, c.Name, c.Value, c.Count, c.Description,
        c.VISDKServer, c.VISDKServerUUID, @ImportTime, NULL
    FROM Current.vExample c
    WHERE c.RowId IN (SELECT RowId FROM @Changes);

    -- Return row counts
    SELECT
        COUNT(CASE WHEN Action = 'INSERT' THEN 1 END) AS Inserted,
        COUNT(CASE WHEN Action = 'UPDATE' THEN 1 END) AS Updated
    FROM @Changes;
END;
```

### Step 6: Update usp_ProcessImport

Add call to the new merge procedure in `usp_ProcessImport`:

```sql
-- In usp_ProcessImport, add:
EXEC usp_MergeTable_vExample @ImportBatchId, @ImportTime;
```

## Testing

1. **Create test data**:
   ```sql
   INSERT INTO Staging.vExample (ImportBatchId, Name, Value, Count)
   VALUES (1, 'Test1', 'Value1', '10');
   ```

2. **Run merge**:
   ```sql
   EXEC usp_MergeTable_vExample @ImportBatchId = 1, @ImportTime = GETDATE();
   ```

3. **Verify**:
   ```sql
   SELECT * FROM Current.vExample;
   SELECT * FROM History.vExample;
   ```

## Checklist

- [ ] Staging table created (all NVARCHAR(MAX))
- [ ] Current table created with proper types
- [ ] History table created with SCD columns
- [ ] PowerShell mapping added
- [ ] Merge stored procedure created
- [ ] usp_ProcessImport updated
- [ ] Tested with sample data

---

## Next Steps

- [Extending Views](./extending-views.md) - Create views for your new table
- [Code Standards](./code-standards.md) - Naming conventions

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
