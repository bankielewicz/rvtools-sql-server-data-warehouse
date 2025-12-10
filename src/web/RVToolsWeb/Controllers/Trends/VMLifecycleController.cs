using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Trends;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Trends;

namespace RVToolsWeb.Controllers.Trends;

/// <summary>
/// Controller for the VM Lifecycle report.
/// </summary>
public class VMLifecycleController : Controller
{
    private readonly VMLifecycleService _reportService;
    private readonly IFilterService _filterService;

    public VMLifecycleController(VMLifecycleService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(VMLifecycleFilter? filter = null)
    {
        filter ??= new VMLifecycleFilter();

        var viewModel = new VMLifecycleViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
