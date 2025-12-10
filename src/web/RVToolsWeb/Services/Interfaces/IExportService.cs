namespace RVToolsWeb.Services.Interfaces;

/// <summary>
/// Service interface for exporting report data to CSV and Excel formats.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports data to CSV format.
    /// </summary>
    /// <typeparam name="T">The type of data to export</typeparam>
    /// <param name="data">Collection of data to export</param>
    /// <param name="fileName">Name for the exported file (without extension)</param>
    /// <returns>Byte array containing the CSV file content</returns>
    byte[] ExportToCsv<T>(IEnumerable<T> data, string fileName);

    /// <summary>
    /// Exports data to Excel (xlsx) format using ClosedXML.
    /// </summary>
    /// <typeparam name="T">The type of data to export</typeparam>
    /// <param name="data">Collection of data to export</param>
    /// <param name="fileName">Name for the exported file (without extension)</param>
    /// <param name="sheetName">Name for the worksheet</param>
    /// <returns>Byte array containing the Excel file content</returns>
    byte[] ExportToExcel<T>(IEnumerable<T> data, string fileName, string sheetName = "Data");
}
