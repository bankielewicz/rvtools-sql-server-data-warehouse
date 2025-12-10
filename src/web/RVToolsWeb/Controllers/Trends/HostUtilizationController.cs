using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.DTOs;
using RVToolsWeb.Models.ViewModels.Trends;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Trends;

namespace RVToolsWeb.Controllers.Trends;

/// <summary>
/// Controller for the Host Utilization Trend report.
/// </summary>
public class HostUtilizationController : Controller
{
    private readonly HostUtilizationService _reportService;
    private readonly IFilterService _filterService;

    public HostUtilizationController(HostUtilizationService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(HostUtilizationFilter? filter = null)
    {
        filter ??= new HostUtilizationFilter();

        var items = await _reportService.GetReportDataAsync(filter);
        var hostNames = await _reportService.GetHostNamesAsync(filter.VI_SDK_Server, filter.Cluster);
        var clusterNames = await _reportService.GetClusterNamesAsync(filter.VI_SDK_Server);

        var viewModel = new HostUtilizationViewModel
        {
            Filter = filter,
            Items = items,
            VISdkServers = await _filterService.GetVISdkServersAsync(),
            Hosts = hostNames.Select(n => new FilterOptionDto { Value = n, Label = n }),
            Clusters = clusterNames.Select(n => new FilterOptionDto { Value = n, Label = n }),
            ChartData = _reportService.BuildChartData(items, filter.HostName)
        };

        return View(viewModel);
    }
}
