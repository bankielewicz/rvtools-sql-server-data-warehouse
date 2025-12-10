using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;

namespace RVToolsWeb.Controllers.Inventory;

/// <summary>
/// Controller for the Network Topology report.
/// </summary>
[Authorize]
public class NetworkTopologyController : Controller
{
    private readonly NetworkTopologyService _reportService;
    private readonly IFilterService _filterService;

    public NetworkTopologyController(NetworkTopologyService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(NetworkTopologyFilter? filter = null)
    {
        filter ??= new NetworkTopologyFilter();

        var viewModel = new NetworkTopologyViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
