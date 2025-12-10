using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Capacity;
using RVToolsWeb.Services.Capacity;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Capacity;

/// <summary>
/// Controller for the VM Right-Sizing report.
/// </summary>
public class VMRightSizingController : Controller
{
    private readonly VMRightSizingService _reportService;
    private readonly IFilterService _filterService;

    public VMRightSizingController(VMRightSizingService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(VMRightSizingFilter? filter = null)
    {
        filter ??= new VMRightSizingFilter();

        var viewModel = new VMRightSizingViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync(),
            Datacenters = await _filterService.GetDatacentersAsync(filter.VI_SDK_Server),
            Clusters = await _filterService.GetClustersAsync(filter.Datacenter, filter.VI_SDK_Server)
        };

        return View(viewModel);
    }
}
