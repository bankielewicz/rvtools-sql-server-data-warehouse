using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;

namespace RVToolsWeb.Controllers.Inventory;

/// <summary>
/// Controller for the Host Inventory report.
/// </summary>
[Authorize]
public class HostInventoryController : Controller
{
    private readonly HostInventoryService _reportService;
    private readonly IFilterService _filterService;

    public HostInventoryController(HostInventoryService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(HostInventoryFilter? filter = null)
    {
        filter ??= new HostInventoryFilter();

        var viewModel = new HostInventoryViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            Datacenters = await _filterService.GetDatacentersAsync(filter.VI_SDK_Server),
            Clusters = await _filterService.GetClustersAsync(filter.Datacenter, filter.VI_SDK_Server),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
