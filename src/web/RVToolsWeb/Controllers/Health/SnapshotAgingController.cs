using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Health;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Health;

/// <summary>
/// Controller for the Snapshot Aging report.
/// </summary>
[Authorize]
public class SnapshotAgingController : Controller
{
    private readonly SnapshotAgingService _reportService;
    private readonly IFilterService _filterService;

    public SnapshotAgingController(SnapshotAgingService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(SnapshotAgingFilter? filter = null)
    {
        filter ??= new SnapshotAgingFilter();

        var viewModel = new SnapshotAgingViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
