/*
    RVTools Data Warehouse - Add RVToolsExportDate Column

    Purpose: Track when RVTools captured the data (vs when we imported it)

    For historical imports, this date is parsed from the filename.
    For regular imports, this defaults to the import timestamp.

    Usage:
        Run against RVToolsDW database:
        sqlcmd -S localhost -d RVToolsDW -i 004_AddRVToolsExportDate.sql
*/

USE [RVToolsDW]
GO

PRINT 'Adding RVToolsExportDate column to Audit.ImportBatch...';
GO

-- Add RVToolsExportDate to Audit.ImportBatch
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Audit.ImportBatch')
    AND name = 'RVToolsExportDate'
)
BEGIN
    ALTER TABLE Audit.ImportBatch ADD
        RVToolsExportDate DATETIME2 NULL;

    PRINT 'Added RVToolsExportDate column to Audit.ImportBatch';
END
ELSE
BEGIN
    PRINT 'RVToolsExportDate column already exists';
END
GO

-- Create index for date-based queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ImportBatch_RVToolsExportDate')
BEGIN
    CREATE NONCLUSTERED INDEX IX_ImportBatch_RVToolsExportDate
    ON Audit.ImportBatch(RVToolsExportDate)
    WHERE RVToolsExportDate IS NOT NULL;

    PRINT 'Created index IX_ImportBatch_RVToolsExportDate';
END
ELSE
BEGIN
    PRINT 'Index IX_ImportBatch_RVToolsExportDate already exists';
END
GO

-- Backfill existing records with ImportStartTime
UPDATE Audit.ImportBatch
SET RVToolsExportDate = ImportStartTime
WHERE RVToolsExportDate IS NULL;

DECLARE @RowsUpdated INT = @@ROWCOUNT;
PRINT 'Backfilled ' + CAST(@RowsUpdated AS VARCHAR(10)) + ' existing records with ImportStartTime';
GO

PRINT 'Migration 004_AddRVToolsExportDate completed successfully';
GO
