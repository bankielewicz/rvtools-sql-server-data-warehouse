using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Capacity;
using RVToolsWeb.Services.Capacity;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Capacity;

/// <summary>
/// Controller for the Datastore Capacity report.
/// </summary>
[Authorize]
public class DatastoreCapacityController : Controller
{
    private readonly DatastoreCapacityService _reportService;
    private readonly IFilterService _filterService;

    public DatastoreCapacityController(DatastoreCapacityService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DatastoreCapacityFilter? filter = null)
    {
        filter ??= new DatastoreCapacityFilter();

        var viewModel = new DatastoreCapacityViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
