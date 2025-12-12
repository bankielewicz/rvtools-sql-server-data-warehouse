using RVToolsService.Services;

namespace RVToolsService;

/// <summary>
/// Temporary test helper for Excel reader validation.
/// Run with: dotnet run -- --test-excel /path/to/file.xlsx
/// </summary>
public static class TestExcelReader
{
    public static async Task<int> RunAsync(IExcelReaderService reader, string filePath)
    {
        Console.WriteLine($"Testing Excel reader with: {filePath}");
        Console.WriteLine(new string('-', 60));

        try
        {
            // First, get sheet names
            var sheetNames = await reader.GetSheetNamesAsync(filePath);
            Console.WriteLine($"Found {sheetNames.Count} worksheets:");
            foreach (var name in sheetNames)
            {
                Console.WriteLine($"  - {name}");
            }
            Console.WriteLine();

            // Then read all sheets
            var sheets = await reader.ReadAllSheetsAsync(filePath);
            Console.WriteLine($"Successfully read {sheets.Count} sheets with data:");
            Console.WriteLine();

            foreach (var sheet in sheets)
            {
                Console.WriteLine($"Sheet: {sheet.Name}");
                Console.WriteLine($"  Rows: {sheet.RowCount}");
                Console.WriteLine($"  Columns: {sheet.Headers.Count}");
                Console.WriteLine($"  Headers: {string.Join(", ", sheet.Headers.Take(5))}{(sheet.Headers.Count > 5 ? "..." : "")}");

                // Show sample data from first row
                if (sheet.Rows.Count > 0)
                {
                    var firstRow = sheet.Rows[0];
                    Console.WriteLine("  Sample (first row):");
                    var sampleKeys = firstRow.Keys.Take(3);
                    foreach (var key in sampleKeys)
                    {
                        Console.WriteLine($"    {key}: {firstRow[key]}");
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine(new string('-', 60));
            Console.WriteLine("Excel reader test PASSED!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.ToString());
            return 1;
        }
    }
}
