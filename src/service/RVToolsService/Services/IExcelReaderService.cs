namespace RVToolsService.Services;

/// <summary>
/// Service for reading RVTools Excel export files.
/// </summary>
public interface IExcelReaderService
{
    /// <summary>
    /// Reads all worksheets from an RVTools Excel file.
    /// </summary>
    /// <param name="filePath">Path to the .xlsx file</param>
    /// <returns>List of sheet data with rows</returns>
    Task<List<SheetData>> ReadAllSheetsAsync(string filePath);

    /// <summary>
    /// Reads a specific worksheet by name.
    /// </summary>
    /// <param name="filePath">Path to the .xlsx file</param>
    /// <param name="sheetName">Name of the worksheet to read</param>
    /// <returns>Sheet data with rows, or null if not found</returns>
    Task<SheetData?> ReadSheetAsync(string filePath, string sheetName);

    /// <summary>
    /// Gets the list of worksheet names in the file.
    /// </summary>
    /// <param name="filePath">Path to the .xlsx file</param>
    /// <returns>List of worksheet names</returns>
    Task<List<string>> GetSheetNamesAsync(string filePath);
}

/// <summary>
/// Represents data from a single Excel worksheet.
/// </summary>
public class SheetData
{
    /// <summary>
    /// Name of the worksheet (matches RVTools tab name).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Column headers from the first row.
    /// </summary>
    public List<string> Headers { get; set; } = new();

    /// <summary>
    /// Data rows as dictionaries (column name -> value).
    /// </summary>
    public List<Dictionary<string, object?>> Rows { get; set; } = new();

    /// <summary>
    /// Total row count (excluding header).
    /// </summary>
    public int RowCount => Rows.Count;
}
