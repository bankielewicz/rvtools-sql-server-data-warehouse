using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace RVToolsService.Services;

/// <summary>
/// Service for reading RVTools Excel export files using ClosedXML.
/// Handles duplicate headers, special characters, and large files.
/// </summary>
public class ExcelReaderService : IExcelReaderService
{
    private readonly ILogger<ExcelReaderService> _logger;

    public ExcelReaderService(ILogger<ExcelReaderService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<SheetData>> ReadAllSheetsAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            ValidateFile(filePath);

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var workbook = new XLWorkbook(stream);

            var sheets = new List<SheetData>();

            foreach (var worksheet in workbook.Worksheets)
            {
                try
                {
                    var sheetData = ReadWorksheet(worksheet);
                    if (sheetData != null && sheetData.RowCount > 0)
                    {
                        sheets.Add(sheetData);
                        _logger.LogDebug("Read sheet '{SheetName}': {RowCount} rows, {ColumnCount} columns",
                            sheetData.Name, sheetData.RowCount, sheetData.Headers.Count);
                    }
                    else
                    {
                        _logger.LogDebug("Skipped empty sheet '{SheetName}'", worksheet.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read sheet '{SheetName}'", worksheet.Name);
                }
            }

            _logger.LogInformation("Read {SheetCount} sheets from file '{FileName}'",
                sheets.Count, Path.GetFileName(filePath));

            return sheets;
        });
    }

    /// <inheritdoc/>
    public async Task<SheetData?> ReadSheetAsync(string filePath, string sheetName)
    {
        return await Task.Run(() =>
        {
            ValidateFile(filePath);

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheets.FirstOrDefault(w =>
                string.Equals(w.Name, sheetName, StringComparison.OrdinalIgnoreCase));

            if (worksheet == null)
            {
                _logger.LogWarning("Sheet '{SheetName}' not found in file '{FileName}'",
                    sheetName, Path.GetFileName(filePath));
                return null;
            }

            return ReadWorksheet(worksheet);
        });
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetSheetNamesAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            ValidateFile(filePath);

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var workbook = new XLWorkbook(stream);

            return workbook.Worksheets.Select(w => w.Name).ToList();
        });
    }

    /// <summary>
    /// Reads a single worksheet into a SheetData object.
    /// </summary>
    private SheetData? ReadWorksheet(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null || usedRange.RowCount() <= 1)
            return null;

        var sheetData = new SheetData { Name = worksheet.Name };
        var headerCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var columnCount = usedRange.ColumnCount();

        // Read header row (row 1)
        var headerRow = worksheet.Row(1);
        for (int col = 1; col <= columnCount; col++)
        {
            var headerValue = headerRow.Cell(col).GetString().Trim();

            // Handle empty headers
            if (string.IsNullOrEmpty(headerValue))
                headerValue = $"Column{col}";

            // Sanitize column name (remove special characters)
            var sanitized = SanitizeColumnName(headerValue);

            // Handle duplicate headers by appending _2, _3, etc.
            if (headerCount.TryGetValue(sanitized, out var count))
            {
                headerCount[sanitized] = count + 1;
                sanitized = $"{sanitized}_{count + 1}";
            }
            else
            {
                headerCount[sanitized] = 1;
            }

            sheetData.Headers.Add(sanitized);
        }

        // Read data rows (starting from row 2)
        var lastRow = usedRange.LastRow().RowNumber();
        for (int row = 2; row <= lastRow; row++)
        {
            var dataRow = worksheet.Row(row);
            var rowData = new Dictionary<string, object?>();
            var hasData = false;

            for (int col = 0; col < sheetData.Headers.Count; col++)
            {
                var cell = dataRow.Cell(col + 1);
                var value = GetCellValue(cell);

                rowData[sheetData.Headers[col]] = value;

                if (value != null)
                    hasData = true;
            }

            // Only add rows that have at least one non-null value
            if (hasData)
            {
                sheetData.Rows.Add(rowData);
            }
        }

        return sheetData;
    }

    /// <summary>
    /// Gets the value from a cell, handling different data types.
    /// </summary>
    private static object? GetCellValue(IXLCell cell)
    {
        if (cell.IsEmpty())
            return null;

        // Return the value as appropriate type
        return cell.DataType switch
        {
            XLDataType.Boolean => cell.GetBoolean(),
            XLDataType.Number => cell.GetDouble(),
            XLDataType.DateTime => cell.GetDateTime(),
            XLDataType.TimeSpan => cell.GetTimeSpan(),
            _ => cell.GetString()
        };
    }

    /// <summary>
    /// Sanitizes a column name by replacing invalid characters with underscores.
    /// Matches StagingService/PowerShell: $sanitized = $columnName -replace '[^a-zA-Z0-9_]', '_'
    /// </summary>
    private static string SanitizeColumnName(string columnName)
    {
        // Replace non-alphanumeric with underscore (match StagingService/PowerShell)
        var sanitized = Regex.Replace(columnName, @"[^a-zA-Z0-9_]", "_");

        // Replace multiple underscores with single
        sanitized = Regex.Replace(sanitized, @"__+", "_");

        // Trim leading/trailing underscores
        sanitized = sanitized.Trim('_');

        // Ensure we don't end up with empty string
        if (string.IsNullOrEmpty(sanitized))
            sanitized = "Column";

        return sanitized;
    }

    /// <summary>
    /// Validates that the file exists and is accessible.
    /// </summary>
    private void ValidateFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Excel file not found: {filePath}", filePath);

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            _logger.LogWarning("File '{FileName}' has unexpected extension '{Extension}' (expected .xlsx)",
                Path.GetFileName(filePath), extension);
        }
    }
}
