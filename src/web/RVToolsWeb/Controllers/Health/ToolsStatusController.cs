using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Health;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Health;

/// <summary>
/// Controller for the VMware Tools Status report.
/// </summary>
[Authorize]
public class ToolsStatusController : Controller
{
    private readonly ToolsStatusService _reportService;
    private readonly IFilterService _filterService;

    public ToolsStatusController(ToolsStatusService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ToolsStatusFilter? filter = null)
    {
        filter ??= new ToolsStatusFilter();

        var viewModel = new ToolsStatusViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
