using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Trends;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Trends;

namespace RVToolsWeb.Controllers.Trends;

/// <summary>
/// Controller for the VM Configuration Changes report.
/// </summary>
public class VMConfigChangesController : Controller
{
    private readonly VMConfigChangesService _reportService;
    private readonly IFilterService _filterService;

    public VMConfigChangesController(VMConfigChangesService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(VMConfigChangesFilter? filter = null)
    {
        filter ??= new VMConfigChangesFilter();

        var viewModel = new VMConfigChangesViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
