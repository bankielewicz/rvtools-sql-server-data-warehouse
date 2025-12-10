using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;

namespace RVToolsWeb.Controllers.Inventory;

/// <summary>
/// Controller for the VM Inventory report.
/// </summary>
public class VMInventoryController : Controller
{
    private readonly VMInventoryService _reportService;
    private readonly IFilterService _filterService;

    public VMInventoryController(VMInventoryService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    /// <summary>
    /// Displays the VM Inventory report with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(VMInventoryFilter? filter = null)
    {
        filter ??= new VMInventoryFilter();

        var viewModel = new VMInventoryViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            Datacenters = await _filterService.GetDatacentersAsync(filter.VI_SDK_Server),
            Clusters = await _filterService.GetClustersAsync(filter.Datacenter, filter.VI_SDK_Server),
            VISdkServers = await _filterService.GetVISdkServersAsync(),
            Powerstates = await _filterService.GetPowerstatesAsync()
        };

        return View(viewModel);
    }
}
