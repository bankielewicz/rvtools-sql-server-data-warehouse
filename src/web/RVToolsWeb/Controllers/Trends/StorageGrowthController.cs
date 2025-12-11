using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.DTOs;
using RVToolsWeb.Models.ViewModels.Trends;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Trends;

namespace RVToolsWeb.Controllers.Trends;

/// <summary>
/// Controller for the Storage Growth report.
/// </summary>
[Authorize]
public class StorageGrowthController : Controller
{
    private readonly StorageGrowthService _reportService;
    private readonly IFilterService _filterService;

    public StorageGrowthController(StorageGrowthService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(StorageGrowthFilter? filter = null)
    {
        filter ??= new StorageGrowthFilter();

        var items = await _reportService.GetReportDataAsync(filter);
        var datastoreNames = await _reportService.GetDatastoreNamesAsync(filter.VI_SDK_Server);

        var viewModel = new StorageGrowthViewModel
        {
            Filter = filter,
            Items = items,
            VISdkServers = await _filterService.GetVISdkServersAsync(),
            Datastores = datastoreNames.Select(n => new FilterOptionDto { Value = n, Label = n }),
            ChartData = _reportService.BuildChartData(items, filter.DatastoreName)
        };

        return View(viewModel);
    }
}
