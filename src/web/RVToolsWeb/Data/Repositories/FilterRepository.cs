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
    /// Gets distinct datacenters from active vCenters, optionally filtered by VI SDK Server.
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
              AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
            ORDER BY Datacenter";

        return await QueryAsync<FilterOptionDto>(sql, new { VI_SDK_Server = viSdkServer });
    }

    /// <summary>
    /// Gets distinct clusters from active vCenters, optionally filtered by datacenter and/or VI SDK Server.
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
              AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
            ORDER BY Cluster";

        return await QueryAsync<FilterOptionDto>(sql, new { Datacenter = datacenter, VI_SDK_Server = viSdkServer });
    }

    /// <summary>
    /// Gets all active VI SDK Servers (vCenter servers).
    /// Only returns vCenters that are marked active in Config.ActiveVCenters
    /// or vCenters not yet registered (default to active).
    /// </summary>
    public async Task<IEnumerable<FilterOptionDto>> GetVISdkServersAsync()
    {
        const string sql = @"
            SELECT
                VI_SDK_Server AS Value,
                VI_SDK_Server AS Label
            FROM [Config].[vw_ActiveVCenterList]
            ORDER BY VI_SDK_Server";

        return await QueryAsync<FilterOptionDto>(sql);
    }

    /// <summary>
    /// Gets distinct powerstates (poweredOn, poweredOff, suspended) from active vCenters.
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
              AND VI_SDK_Server IN (SELECT VI_SDK_Server FROM [Config].[vw_ActiveVCenterList])
            ORDER BY Powerstate";

        return await QueryAsync<FilterOptionDto>(sql);
    }
}
