using System.Diagnostics;
using System.Text;
using Dapper;
using RVToolsShared.Data;
using RVToolsShared.Security;

namespace RVToolsService.Services;

/// <summary>
/// Service for managing staging table operations during imports.
/// Ports PowerShell Import-SheetToStaging and Insert-StagingBatch logic to C#.
/// </summary>
public class StagingService : IStagingService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<StagingService> _logger;

    // Batch size for bulk inserts (matching PowerShell)
    private const int BatchSize = 1000;

    // Cache of staging columns per table
    private readonly Dictionary<string, List<string>> _columnCache = new(StringComparer.OrdinalIgnoreCase);

    public StagingService(ISqlConnectionFactory connectionFactory, ILogger<StagingService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetStagingColumnsAsync(string tableName)
    {
        // SEC-006: Validate table name against whitelist
        TableNameValidator.Validate(tableName);

        // Check cache first
        if (_columnCache.TryGetValue(tableName, out var cached))
            return cached;

        const string sql = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = @TableName
            AND COLUMN_NAME NOT IN ('StagingId', 'ImportBatchId', 'ImportRowNum')
            ORDER BY ORDINAL_POSITION";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var columns = (await connection.QueryAsync<string>(sql, new { TableName = tableName })).ToList();

        // Cache the result
        _columnCache[tableName] = columns;

        return columns;
    }

    /// <inheritdoc/>
    public async Task ClearStagingTableAsync(string tableName)
    {
        // SEC-006: Validate table name against whitelist
        TableNameValidator.Validate(tableName);

        // Use parameterized table name (safe because we validated against whitelist)
        var sql = $"TRUNCATE TABLE [Staging].[{tableName}]";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql);
        _logger.LogDebug("Cleared staging table [Staging].[{TableName}]", tableName);
    }

    /// <inheritdoc/>
    public async Task<SheetImportResult> ImportSheetToStagingAsync(SheetData sheetData, int importBatchId)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new SheetImportResult { SheetName = sheetData.Name };

        // SEC-006: Validate table name against whitelist
        var canonicalName = TableNameValidator.GetCanonicalName(sheetData.Name);
        if (canonicalName == null)
        {
            _logger.LogWarning("Sheet '{SheetName}' is not a valid RVTools table, skipping", sheetData.Name);
            result.ErrorMessage = $"Sheet '{sheetData.Name}' is not in SEC-006 whitelist";
            return result;
        }

        try
        {
            result.SourceRows = sheetData.RowCount;

            if (sheetData.RowCount == 0)
            {
                _logger.LogDebug("Sheet '{SheetName}' is empty, skipping", sheetData.Name);
                return result;
            }

            // Clear staging table before import
            await ClearStagingTableAsync(canonicalName);

            // Get staging table columns
            var stagingColumns = await GetStagingColumnsAsync(canonicalName);
            if (stagingColumns.Count == 0)
            {
                result.ErrorMessage = $"No columns found for staging table [Staging].[{canonicalName}]";
                _logger.LogWarning("No columns found for staging table [Staging].[{TableName}]", canonicalName);
                return result;
            }

            // Build column mapping (Excel header -> Staging column)
            var columnMap = BuildColumnMapping(sheetData.Headers, stagingColumns);

            _logger.LogDebug("Sheet '{SheetName}': Mapped {MappedCount}/{TotalCount} columns to staging",
                sheetData.Name, columnMap.Count, sheetData.Headers.Count);

            // Process rows in batches
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var batch = new List<Dictionary<string, object?>>();
            var rowNum = 0;

            foreach (var row in sheetData.Rows)
            {
                rowNum++;

                try
                {
                    var stagingRow = new Dictionary<string, object?>
                    {
                        ["ImportBatchId"] = importBatchId,
                        ["ImportRowNum"] = rowNum
                    };

                    // Map Excel columns to staging columns
                    foreach (var (excelCol, stagingCol) in columnMap)
                    {
                        if (row.TryGetValue(excelCol, out var value))
                        {
                            // Convert all values to string for staging (NVARCHAR columns)
                            stagingRow[stagingCol] = value?.ToString();
                        }
                        else
                        {
                            stagingRow[stagingCol] = null;
                        }
                    }

                    batch.Add(stagingRow);
                    result.StagedRows++;

                    // Insert batch when full
                    if (batch.Count >= BatchSize)
                    {
                        await InsertBatchAsync(connection, canonicalName, batch);
                        batch.Clear();

                        _logger.LogDebug("[{SheetName}] Staged {Count} rows...", sheetData.Name, result.StagedRows);
                    }
                }
                catch (Exception ex)
                {
                    result.FailedRows++;
                    result.StagedRows--;
                    _logger.LogWarning(ex, "[{SheetName}] Row {RowNum} failed: {Error}",
                        sheetData.Name, rowNum, ex.Message);
                }
            }

            // Insert remaining rows
            if (batch.Count > 0)
            {
                await InsertBatchAsync(connection, canonicalName, batch);
            }

            stopwatch.Stop();
            result.DurationMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("[{SheetName}] Completed: {Staged} staged, {Failed} failed ({Duration}ms)",
                sheetData.Name, result.StagedRows, result.FailedRows, result.DurationMs);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            result.ErrorMessage = ex.Message;
            result.FailedRows = result.SourceRows - result.StagedRows;

            _logger.LogError(ex, "[{SheetName}] Import failed: {Error}", sheetData.Name, ex.Message);
            return result;
        }
    }

    /// <summary>
    /// Builds a mapping from Excel column names to staging column names.
    /// </summary>
    private static Dictionary<string, string> BuildColumnMapping(List<string> excelHeaders, List<string> stagingColumns)
    {
        var columnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var stagingSet = new HashSet<string>(stagingColumns, StringComparer.OrdinalIgnoreCase);

        foreach (var header in excelHeaders)
        {
            // Sanitize column name (same logic as ExcelReaderService)
            var sanitized = SanitizeColumnName(header);

            // Check if staging table has this column
            if (stagingSet.Contains(sanitized))
            {
                columnMap[header] = sanitized;
            }
        }

        return columnMap;
    }

    /// <summary>
    /// Sanitizes a column name to match staging table column names.
    /// Matches PowerShell: $sanitized = $excelCol -replace '[^a-zA-Z0-9_]', '_' -replace '__+', '_'
    /// </summary>
    private static string SanitizeColumnName(string columnName)
    {
        // Replace non-alphanumeric with underscore
        var sanitized = System.Text.RegularExpressions.Regex.Replace(columnName, @"[^a-zA-Z0-9_]", "_");

        // Replace multiple underscores with single
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"__+", "_");

        // Trim leading/trailing underscores
        sanitized = sanitized.Trim('_');

        // Handle special cases (same as PowerShell)
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"^Num_", "#_");
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"_Num$", "_#");

        return sanitized;
    }

    /// <summary>
    /// Inserts a batch of rows into a staging table.
    /// Uses dynamic SQL with proper escaping (safe because table name is validated).
    /// </summary>
    private async Task InsertBatchAsync(Microsoft.Data.SqlClient.SqlConnection connection,
        string tableName, List<Dictionary<string, object?>> batch)
    {
        if (batch.Count == 0) return;

        // Get column list from first row
        var columns = batch[0].Keys.ToList();
        var columnList = string.Join(", ", columns.Select(c => $"[{c}]"));

        // Build VALUES clause
        var valueRows = new StringBuilder();
        for (int i = 0; i < batch.Count; i++)
        {
            if (i > 0) valueRows.Append(",\n");

            valueRows.Append('(');
            for (int j = 0; j < columns.Count; j++)
            {
                if (j > 0) valueRows.Append(", ");

                var value = batch[i][columns[j]];
                if (value == null)
                {
                    valueRows.Append("NULL");
                }
                else
                {
                    // Escape single quotes and wrap in N'' for NVARCHAR
                    var escaped = value.ToString()!.Replace("'", "''");
                    valueRows.Append($"N'{escaped}'");
                }
            }
            valueRows.Append(')');
        }

        // Build and execute INSERT statement
        // SEC-006: Table name is validated against whitelist before reaching here
        var sql = $"INSERT INTO [Staging].[{tableName}] ({columnList}) VALUES {valueRows}";

        await connection.ExecuteAsync(sql, commandTimeout: 300);
    }
}
