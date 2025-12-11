using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Capacity;
using RVToolsWeb.Services.Capacity;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Capacity;

/// <summary>
/// Controller for the VM Resource Allocation report.
/// </summary>
[Authorize]
public class VMResourceAllocationController : Controller
{
    private readonly VMResourceAllocationService _reportService;
    private readonly IFilterService _filterService;

    public VMResourceAllocationController(VMResourceAllocationService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(VMResourceAllocationFilter? filter = null)
    {
        filter ??= new VMResourceAllocationFilter();

        var viewModel = new VMResourceAllocationViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync(),
            Datacenters = await _filterService.GetDatacentersAsync(filter.VI_SDK_Server),
            Clusters = await _filterService.GetClustersAsync(filter.Datacenter, filter.VI_SDK_Server),
            Powerstates = await _filterService.GetPowerstatesAsync()
        };

        return View(viewModel);
    }
}
