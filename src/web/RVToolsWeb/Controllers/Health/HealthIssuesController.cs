using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Health;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Health;

/// <summary>
/// Controller for the Health Issues report.
/// </summary>
public class HealthIssuesController : Controller
{
    private readonly HealthIssuesService _reportService;
    private readonly IFilterService _filterService;

    public HealthIssuesController(HealthIssuesService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(HealthIssuesFilter? filter = null)
    {
        filter ??= new HealthIssuesFilter();

        var viewModel = new HealthIssuesViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
