using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;

namespace RVToolsWeb.Controllers.Inventory;

/// <summary>
/// Controller for the Enterprise Summary report.
/// </summary>
[Authorize]
public class EnterpriseSummaryController : Controller
{
    private readonly EnterpriseSummaryService _reportService;
    private readonly IFilterService _filterService;

    public EnterpriseSummaryController(EnterpriseSummaryService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(EnterpriseSummaryFilter? filter = null)
    {
        filter ??= new EnterpriseSummaryFilter();

        var viewModel = new EnterpriseSummaryViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
