using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;

namespace RVToolsWeb.Controllers.Inventory;

/// <summary>
/// Controller for the Cluster Summary report.
/// </summary>
public class ClusterSummaryController : Controller
{
    private readonly ClusterSummaryService _reportService;
    private readonly IFilterService _filterService;

    public ClusterSummaryController(ClusterSummaryService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ClusterSummaryFilter? filter = null)
    {
        filter ??= new ClusterSummaryFilter();

        var viewModel = new ClusterSummaryViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
