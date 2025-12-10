using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Services.Interfaces;

/// <summary>
/// Service interface for retrieving cached filter dropdown options.
/// </summary>
public interface IFilterService
{
    /// <summary>
    /// Gets datacenters, optionally filtered by VI SDK Server.
    /// Results are cached with sliding expiration.
    /// </summary>
    Task<IEnumerable<FilterOptionDto>> GetDatacentersAsync(string? viSdkServer = null);

    /// <summary>
    /// Gets clusters, optionally filtered by datacenter and/or VI SDK Server.
    /// Results are cached with sliding expiration.
    /// </summary>
    Task<IEnumerable<FilterOptionDto>> GetClustersAsync(string? datacenter = null, string? viSdkServer = null);

    /// <summary>
    /// Gets all VI SDK Servers (vCenter servers).
    /// Results are cached with sliding expiration.
    /// </summary>
    Task<IEnumerable<FilterOptionDto>> GetVISdkServersAsync();

    /// <summary>
    /// Gets all powerstates (poweredOn, poweredOff, suspended).
    /// Results are cached with sliding expiration.
    /// </summary>
    Task<IEnumerable<FilterOptionDto>> GetPowerstatesAsync();
}
