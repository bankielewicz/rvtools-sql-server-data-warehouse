using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service interface for retrieving database health and status information.
/// </summary>
public interface IDatabaseStatusService
{
    /// <summary>
    /// Get comprehensive database status including connection health, import stats, and record counts.
    /// </summary>
    Task<DatabaseStatusViewModel> GetStatusAsync();

    /// <summary>
    /// Get all table mappings from Config.TableMapping.
    /// </summary>
    Task<IEnumerable<TableMappingViewModel>> GetTableMappingsAsync();

    /// <summary>
    /// Get total count of column mappings.
    /// </summary>
    Task<int> GetColumnMappingCountAsync();
}
