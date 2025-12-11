using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;

namespace RVToolsWeb.Controllers.Inventory;

/// <summary>
/// Controller for the License Compliance report.
/// </summary>
[Authorize]
public class LicenseComplianceController : Controller
{
    private readonly LicenseComplianceService _reportService;
    private readonly IFilterService _filterService;

    public LicenseComplianceController(LicenseComplianceService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(LicenseComplianceFilter? filter = null)
    {
        filter ??= new LicenseComplianceFilter();

        var viewModel = new LicenseComplianceViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
