using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models;
using RVToolsWeb.Services.Home;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DashboardService _dashboardService;
    private readonly IWebLoggingService _loggingService;

    public HomeController(
        ILogger<HomeController> logger,
        DashboardService dashboardService,
        IWebLoggingService loggingService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
        _loggingService = loggingService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Get global time filter from middleware
            var timeFilter = HttpContext.Items["GlobalTimeFilter"]?.ToString() ?? "30d";
            ViewBag.TimeFilterLabel = HttpContext.Items["GlobalTimeFilterLabel"]?.ToString() ?? "Last 30 Days";

            var viewModel = await _dashboardService.GetDashboardDataAsync(timeFilter);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");

            // Log to database via our logging service
            await _loggingService.LogExceptionAsync(ex, HttpContext);

            // Return empty dashboard on error rather than crashing
            return View(new Models.ViewModels.Home.DashboardViewModel());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
