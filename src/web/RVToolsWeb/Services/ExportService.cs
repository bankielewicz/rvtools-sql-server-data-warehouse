using System.Reflection;
using System.Text;
using ClosedXML.Excel;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Services;

/// <summary>
/// Service for exporting report data to CSV and Excel formats.
/// </summary>
public class ExportService : IExportService
{
    public byte[] ExportToCsv<T>(IEnumerable<T> data, string fileName)
    {
        var properties = GetExportProperties<T>();
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(GetDisplayName(p)))));

        // Data rows
        foreach (var item in data)
        {
            var values = properties.Select(p => EscapeCsvField(GetPropertyValue(p, item)));
            sb.AppendLine(string.Join(",", values));
        }

        // Return as UTF-8 with BOM for Excel compatibility
        var preamble = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + content.Length];
        preamble.CopyTo(result, 0);
        content.CopyTo(result, preamble.Length);

        return result;
    }

    public byte[] ExportToExcel<T>(IEnumerable<T> data, string fileName, string sheetName = "Data")
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var properties = GetExportProperties<T>();
        var dataList = data.ToList();

        // Header row
        for (int i = 0; i < properties.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = GetDisplayName(properties[i]);
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Data rows
        for (int row = 0; row < dataList.Count; row++)
        {
            var item = dataList[row];
            for (int col = 0; col < properties.Length; col++)
            {
                var cell = worksheet.Cell(row + 2, col + 1);
                var value = properties[col].GetValue(item);
                SetCellValue(cell, value, properties[col].PropertyType);
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Freeze header row
        worksheet.SheetView.FreezeRows(1);

        // Enable auto-filter
        if (dataList.Count > 0)
        {
            worksheet.Range(1, 1, dataList.Count + 1, properties.Length).SetAutoFilter();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static PropertyInfo[] GetExportProperties<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();
    }

    private static string GetDisplayName(PropertyInfo property)
    {
        // Convert property name to display name (e.g., "Total_disk_capacity_MiB" -> "Total disk capacity MiB")
        return property.Name.Replace("_", " ");
    }

    private static string GetPropertyValue(PropertyInfo property, object? item)
    {
        if (item == null) return string.Empty;

        var value = property.GetValue(item);
        if (value == null) return string.Empty;

        // Format dates consistently
        if (value is DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // Format booleans
        if (value is bool b)
        {
            return b ? "Yes" : "No";
        }

        // Format decimals
        if (value is decimal d)
        {
            return d.ToString("N2");
        }

        return value.ToString() ?? string.Empty;
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return string.Empty;

        // If field contains comma, quote, or newline, wrap in quotes and escape internal quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    private static void SetCellValue(IXLCell cell, object? value, Type propertyType)
    {
        if (value == null)
        {
            cell.Value = Blank.Value;
            return;
        }

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (value is DateTime dt)
        {
            cell.Value = dt;
            cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
        }
        else if (value is bool b)
        {
            cell.Value = b ? "Yes" : "No";
        }
        else if (underlyingType == typeof(int) || underlyingType == typeof(long) ||
                 underlyingType == typeof(short) || underlyingType == typeof(byte))
        {
            cell.Value = Convert.ToInt64(value);
        }
        else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
                 underlyingType == typeof(float))
        {
            cell.Value = Convert.ToDouble(value);
            cell.Style.NumberFormat.Format = "#,##0.00";
        }
        else
        {
            cell.Value = value.ToString();
        }
    }
}
