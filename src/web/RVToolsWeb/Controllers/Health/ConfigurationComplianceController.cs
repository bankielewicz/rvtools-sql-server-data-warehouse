using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Health;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Health;

/// <summary>
/// Controller for the Configuration Compliance report.
/// </summary>
public class ConfigurationComplianceController : Controller
{
    private readonly ConfigurationComplianceService _reportService;
    private readonly IFilterService _filterService;

    public ConfigurationComplianceController(ConfigurationComplianceService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ConfigurationComplianceFilter? filter = null)
    {
        filter ??= new ConfigurationComplianceFilter();

        var viewModel = new ConfigurationComplianceViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
