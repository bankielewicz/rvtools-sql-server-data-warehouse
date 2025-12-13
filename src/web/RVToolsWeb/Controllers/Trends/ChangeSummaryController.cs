using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models;
using RVToolsWeb.Models.ViewModels.Trends;
using RVToolsWeb.Services;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Trends;

namespace RVToolsWeb.Controllers.Trends;

[Authorize]
public class ChangeSummaryController : Controller
{
    private readonly IChangeSummaryService _changeSummaryService;
    private readonly IFilterService _filterService;

    public ChangeSummaryController(
        IChangeSummaryService changeSummaryService,
        IFilterService filterService)
    {
        _changeSummaryService = changeSummaryService;
        _filterService = filterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ChangeSummaryFilter? filter = null)
    {
        filter ??= new ChangeSummaryFilter();

        // Use global filter if not overridden
        if (string.IsNullOrEmpty(filter.TimeFilter))
        {
            filter.TimeFilter = HttpContext.Items["GlobalTimeFilter"]?.ToString() ?? TimeFilter.DefaultFilter;
        }

        var viewModel = await _changeSummaryService.GetChangeSummaryAsync(filter);
        viewModel.VISdkServers = await _filterService.GetVISdkServersAsync();

        // Add flag for "using global" vs "overridden"
        ViewBag.UsingGlobalFilter = filter.TimeFilter == HttpContext.Items["GlobalTimeFilter"]?.ToString();
        ViewBag.GlobalTimeFilter = HttpContext.Items["GlobalTimeFilter"];

        return View(viewModel);
    }
}
