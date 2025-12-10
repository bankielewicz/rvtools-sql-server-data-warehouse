using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Data.Repositories;

/// <summary>
/// Repository for retrieving filter dropdown values from the database.
/// Queries Current.vInfo for distinct filter values.
/// </summary>
public class FilterRepository : BaseRepository
{
    public FilterRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    /// <summary>
    /// Gets distinct datacenters, optionally filtered by VI SDK Server.
    /// </summary>
    public async Task<IEnumerable<FilterOptionDto>> GetDatacentersAsync(string? viSdkServer = null)
    {
        const string sql = @"
            SELECT DISTINCT
                Datacenter AS Value,
                Datacenter AS Label
            FROM [Current].[vInfo]
            WHERE Datacenter IS NOT NULL
              AND Datacenter <> ''
              AND (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY Datacenter";

        return await QueryAsync<FilterOptionDto>(sql, new { VI_SDK_Server = viSdkServer });
    }

    /// <summary>
    /// Gets distinct clusters, optionally filtered by datacenter and/or VI SDK Server.
    /// </summary>
    public async Task<IEnumerable<FilterOptionDto>> GetClustersAsync(string? datacenter = null, string? viSdkServer = null)
    {
        const string sql = @"
            SELECT DISTINCT
                Cluster AS Value,
                Cluster AS Label
            FROM [Current].[vInfo]
            WHERE Cluster IS NOT NULL
              AND Cluster <> ''
              AND (@Datacenter IS NULL OR Datacenter = @Datacenter)
              AND (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY Cluster";

        return await QueryAsync<FilterOptionDto>(sql, new { Datacenter = datacenter, VI_SDK_Server = viSdkServer });
    }

    /// <summary>
    /// Gets all distinct VI SDK Servers (vCenter servers).
    /// </summary>
    public async Task<IEnumerable<FilterOptionDto>> GetVISdkServersAsync()
    {
        const string sql = @"
            SELECT DISTINCT
                VI_SDK_Server AS Value,
                VI_SDK_Server AS Label
            FROM [Current].[vInfo]
            WHERE VI_SDK_Server IS NOT NULL
              AND VI_SDK_Server <> ''
            ORDER BY VI_SDK_Server";

        return await QueryAsync<FilterOptionDto>(sql);
    }

    /// <summary>
    /// Gets distinct powerstates (poweredOn, poweredOff, suspended).
    /// </summary>
    public async Task<IEnumerable<FilterOptionDto>> GetPowerstatesAsync()
    {
        const string sql = @"
            SELECT DISTINCT
                Powerstate AS Value,
                Powerstate AS Label
            FROM [Current].[vInfo]
            WHERE Powerstate IS NOT NULL
              AND Powerstate <> ''
            ORDER BY Powerstate";

        return await QueryAsync<FilterOptionDto>(sql);
    }
}
