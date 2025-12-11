using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service interface for managing Config.TableRetention database table.
/// </summary>
public interface ITableRetentionService
{
    /// <summary>
    /// Get all table retention overrides.
    /// </summary>
    Task<IEnumerable<TableRetentionViewModel>> GetAllRetentionsAsync();

    /// <summary>
    /// Get list of tables available for retention override (History tables not already configured).
    /// </summary>
    Task<IEnumerable<string>> GetAvailableTablesAsync();

    /// <summary>
    /// Add a new retention override.
    /// </summary>
    Task<bool> AddRetentionAsync(string schemaName, string tableName, int retentionDays);

    /// <summary>
    /// Update an existing retention override.
    /// </summary>
    Task<bool> UpdateRetentionAsync(int id, int retentionDays);

    /// <summary>
    /// Delete a retention override.
    /// </summary>
    Task<bool> DeleteRetentionAsync(int id);
}
