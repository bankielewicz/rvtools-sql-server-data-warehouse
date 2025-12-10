using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Health;
using Dapper;

namespace RVToolsWeb.Services.Health;

/// <summary>
/// Service for retrieving Orphaned Files report data.
/// </summary>
public class OrphanedFilesService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public OrphanedFilesService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OrphanedFilesItem>> GetReportDataAsync(OrphanedFilesFilter filter)
    {
        const string sql = @"
            SELECT
                File_Name,
                Friendly_Path_Name,
                Path,
                File_Type,
                File_Size_Bytes,
                File_Size_GiB,
                Datastore,
                IsOrphaned,
                VI_SDK_Server
            FROM [Reporting].[vw_Health_Orphaned_Files]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@ShowOrphanedOnly = 0 OR IsOrphaned = 1)
            ORDER BY IsOrphaned DESC, File_Size_GiB DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<OrphanedFilesItem>(sql, new
        {
            filter.VI_SDK_Server,
            ShowOrphanedOnly = filter.ShowOrphanedOnly ? 1 : 0
        });
    }
}
