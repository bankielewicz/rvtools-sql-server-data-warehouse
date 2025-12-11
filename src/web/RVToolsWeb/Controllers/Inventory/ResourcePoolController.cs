using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;

namespace RVToolsWeb.Controllers.Inventory;

/// <summary>
/// Controller for the Resource Pool Utilization report.
/// </summary>
[Authorize]
public class ResourcePoolController : Controller
{
    private readonly ResourcePoolService _reportService;
    private readonly IFilterService _filterService;

    public ResourcePoolController(ResourcePoolService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ResourcePoolFilter? filter = null)
    {
        filter ??= new ResourcePoolFilter();

        var viewModel = new ResourcePoolViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
