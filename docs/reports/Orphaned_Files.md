# Orphaned Files Report

**Navigation**: [SSRS Reports Guide](../SSRS_Reports_User_Guide.md) | [Reports Overview](../usage/reports.md) | Health Reports

---

**Category**: Health
**View**: `[Reporting].[vw_Health_Orphaned_Files]`
**RDL File**: `src/reports/Health/Orphaned_Files.rdl`
**SQL Source**: `src/tsql/Views/Health/vw_Orphaned_Files.sql`

## Purpose

Identifies VMDK files and snapshots that are not linked to any registered VM, indicating potential storage that can be reclaimed.

## Data Source

- **Primary Table**: `Current.vFileInfo`
- **Secondary Table**: `Current.vInfo` (for orphan detection)
- **Update Frequency**: Real-time (queries current snapshot from last RVTools import)
- **Prerequisite**: RVTools must be run with `-GetFileInfo` flag to populate vFileInfo data

## View Columns

| Column | Data Type | Description |
|--------|-----------|-------------|
| File_Name | NVARCHAR | VMDK/snapshot file name |
| Friendly_Path_Name | NVARCHAR | Human-readable path |
| Path | NVARCHAR | Full datastore path |
| File_Type | NVARCHAR | File type: VMDK, VMSD, or VMSN |
| File_Size_Bytes | BIGINT | File size in bytes |
| File_Size_GiB | DECIMAL(10,2) | File size in gibibytes |
| Datastore | NVARCHAR | Datastore name (extracted from Path) |
| IsOrphaned | BIT | 1 = orphaned, 0 = attached to a VM |
| VI_SDK_Server | NVARCHAR | vCenter server |
| ImportBatchId | INT | Import batch reference |
| LastModifiedDate | DATETIME | Last import timestamp |

## File Types

- **VMDK**: Virtual machine disk file (contains actual VM data)
- **VMSD**: Snapshot descriptor file (snapshot metadata)
- **VMSN**: Snapshot data file (memory + disk state at snapshot time)

## Orphan Detection Logic

A file is marked as orphaned when:
1. File_Type is 'VMDK', AND
2. No VM in `Current.vInfo` has a Path that matches the file's location

```sql
CASE
    WHEN f.File_Type = 'VMDK' AND NOT EXISTS (
        SELECT 1 FROM [Current].[vInfo] i
        WHERE f.Path LIKE i.Path + '%'
          AND f.VI_SDK_Server = i.VI_SDK_Server
    ) THEN 1
    ELSE 0
END AS IsOrphaned
```

## Sample Queries

**Large orphaned VMDKs (potential storage reclamation):**
```sql
SELECT File_Name, Datastore, File_Size_GiB, Path
FROM [Reporting].[vw_Health_Orphaned_Files]
WHERE IsOrphaned = 1
  AND File_Size_GiB >= 10
ORDER BY File_Size_GiB DESC;
```

**Total reclaimable storage by datastore:**
```sql
SELECT Datastore,
       COUNT(*) AS Orphaned_File_Count,
       SUM(File_Size_GiB) AS Total_GiB_Reclaimable
FROM [Reporting].[vw_Health_Orphaned_Files]
WHERE IsOrphaned = 1
GROUP BY Datastore
ORDER BY Total_GiB_Reclaimable DESC;
```

**Check if vFileInfo has data:**
```sql
SELECT COUNT(*) AS FileCount FROM [Current].[vFileInfo];
-- If 0, RVTools was not run with -GetFileInfo flag
```

## Related Reports

- [Datastore Inventory](./Datastore_Inventory.md) - Datastores containing orphaned files
- [Datastore Capacity](./Datastore_Capacity.md) - Storage impact of orphaned files

## Notes

- If the view returns no rows, verify that RVTools was run with the `-GetFileInfo` parameter.
- The view only includes files of type VMDK, VMSD, and VMSN.
- Orphan detection compares file paths against registered VM paths; false positives may occur if VMs use non-standard disk locations.
