using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Health;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Health;

/// <summary>
/// Controller for the Orphaned Files report.
/// </summary>
[Authorize]
public class OrphanedFilesController : Controller
{
    private readonly OrphanedFilesService _reportService;
    private readonly IFilterService _filterService;

    public OrphanedFilesController(OrphanedFilesService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(OrphanedFilesFilter? filter = null)
    {
        filter ??= new OrphanedFilesFilter();

        var viewModel = new OrphanedFilesViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
