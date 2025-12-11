using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Capacity;
using RVToolsWeb.Services.Capacity;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Capacity;

/// <summary>
/// Controller for the Host Capacity report.
/// </summary>
[Authorize]
public class HostCapacityController : Controller
{
    private readonly HostCapacityService _reportService;
    private readonly IFilterService _filterService;

    public HostCapacityController(HostCapacityService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(HostCapacityFilter? filter = null)
    {
        filter ??= new HostCapacityFilter();

        var viewModel = new HostCapacityViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync(),
            Datacenters = await _filterService.GetDatacentersAsync(filter.VI_SDK_Server),
            Clusters = await _filterService.GetClustersAsync(filter.Datacenter, filter.VI_SDK_Server)
        };

        return View(viewModel);
    }
}
