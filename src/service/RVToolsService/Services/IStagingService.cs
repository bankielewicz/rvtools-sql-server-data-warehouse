namespace RVToolsService.Services;

/// <summary>
/// Service for managing staging table operations during imports.
/// </summary>
public interface IStagingService
{
    /// <summary>
    /// Gets the column names for a staging table.
    /// </summary>
    /// <param name="tableName">Table name (must be in SEC-006 whitelist)</param>
    /// <returns>List of column names (excluding StagingId, ImportBatchId, ImportRowNum)</returns>
    Task<List<string>> GetStagingColumnsAsync(string tableName);

    /// <summary>
    /// Clears (truncates) a staging table.
    /// </summary>
    /// <param name="tableName">Table name (must be in SEC-006 whitelist)</param>
    Task ClearStagingTableAsync(string tableName);

    /// <summary>
    /// Imports a sheet's data into the corresponding staging table.
    /// </summary>
    /// <param name="sheetData">Excel sheet data</param>
    /// <param name="importBatchId">Import batch ID for tracking</param>
    /// <returns>Import result statistics</returns>
    Task<SheetImportResult> ImportSheetToStagingAsync(SheetData sheetData, int importBatchId);
}

/// <summary>
/// Result of importing a sheet to staging.
/// </summary>
public class SheetImportResult
{
    public string SheetName { get; set; } = string.Empty;
    public int SourceRows { get; set; }
    public int StagedRows { get; set; }
    public int FailedRows { get; set; }
    public int DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}
