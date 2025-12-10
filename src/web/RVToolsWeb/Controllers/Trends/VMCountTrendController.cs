using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Trends;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Trends;

namespace RVToolsWeb.Controllers.Trends;

/// <summary>
/// Controller for the VM Count Trend report.
/// </summary>
[Authorize]
public class VMCountTrendController : Controller
{
    private readonly VMCountTrendService _reportService;
    private readonly IFilterService _filterService;

    public VMCountTrendController(VMCountTrendService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(VMCountTrendFilter? filter = null)
    {
        filter ??= new VMCountTrendFilter();

        var items = await _reportService.GetReportDataAsync(filter);

        var viewModel = new VMCountTrendViewModel
        {
            Filter = filter,
            Items = items,
            VISdkServers = await _filterService.GetVISdkServersAsync(),
            ChartData = _reportService.BuildChartData(items, filter.VI_SDK_Server)
        };

        return View(viewModel);
    }
}
