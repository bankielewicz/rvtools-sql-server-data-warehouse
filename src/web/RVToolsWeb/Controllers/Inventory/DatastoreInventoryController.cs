using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;

namespace RVToolsWeb.Controllers.Inventory;

/// <summary>
/// Controller for the Datastore Inventory report.
/// </summary>
[Authorize]
public class DatastoreInventoryController : Controller
{
    private readonly DatastoreInventoryService _reportService;
    private readonly IFilterService _filterService;

    public DatastoreInventoryController(DatastoreInventoryService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DatastoreInventoryFilter? filter = null)
    {
        filter ??= new DatastoreInventoryFilter();

        var viewModel = new DatastoreInventoryViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
