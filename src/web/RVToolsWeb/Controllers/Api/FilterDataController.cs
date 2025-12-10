using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Api;

/// <summary>
/// API controller for AJAX filter dropdown requests.
/// Used by cascading dropdowns in report filter panels.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilterDataController : ControllerBase
{
    private readonly IFilterService _filterService;

    public FilterDataController(IFilterService filterService)
    {
        _filterService = filterService;
    }

    /// <summary>
    /// Gets datacenters for dropdown, optionally filtered by VI SDK Server.
    /// </summary>
    /// <param name="viSdkServer">Optional VI SDK Server to filter by</param>
    /// <returns>List of FilterOptionDto with Value and Label</returns>
    [HttpGet("datacenters")]
    public async Task<IActionResult> GetDatacenters([FromQuery] string? viSdkServer = null)
    {
        var datacenters = await _filterService.GetDatacentersAsync(viSdkServer);
        return Ok(datacenters);
    }

    /// <summary>
    /// Gets clusters for dropdown, optionally filtered by datacenter and/or VI SDK Server.
    /// </summary>
    /// <param name="datacenter">Optional datacenter to filter by</param>
    /// <param name="viSdkServer">Optional VI SDK Server to filter by</param>
    /// <returns>List of FilterOptionDto with Value and Label</returns>
    [HttpGet("clusters")]
    public async Task<IActionResult> GetClusters(
        [FromQuery] string? datacenter = null,
        [FromQuery] string? viSdkServer = null)
    {
        var clusters = await _filterService.GetClustersAsync(datacenter, viSdkServer);
        return Ok(clusters);
    }

    /// <summary>
    /// Gets all VI SDK Servers (vCenter servers) for dropdown.
    /// </summary>
    /// <returns>List of FilterOptionDto with Value and Label</returns>
    [HttpGet("viservers")]
    public async Task<IActionResult> GetVIServers()
    {
        var servers = await _filterService.GetVISdkServersAsync();
        return Ok(servers);
    }

    /// <summary>
    /// Gets all powerstates for dropdown.
    /// </summary>
    /// <returns>List of FilterOptionDto with Value and Label</returns>
    [HttpGet("powerstates")]
    public async Task<IActionResult> GetPowerstates()
    {
        var powerstates = await _filterService.GetPowerstatesAsync();
        return Ok(powerstates);
    }
}
