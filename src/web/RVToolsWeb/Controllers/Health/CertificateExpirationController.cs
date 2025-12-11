using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Health;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers.Health;

/// <summary>
/// Controller for the Certificate Expiration report.
/// </summary>
[Authorize]
public class CertificateExpirationController : Controller
{
    private readonly CertificateExpirationService _reportService;
    private readonly IFilterService _filterService;

    public CertificateExpirationController(CertificateExpirationService reportService, IFilterService filterService)
    {
        _reportService = reportService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CertificateExpirationFilter? filter = null)
    {
        filter ??= new CertificateExpirationFilter();

        var viewModel = new CertificateExpirationViewModel
        {
            Filter = filter,
            Items = await _reportService.GetReportDataAsync(filter),
            VISdkServers = await _filterService.GetVISdkServersAsync()
        };

        return View(viewModel);
    }
}
