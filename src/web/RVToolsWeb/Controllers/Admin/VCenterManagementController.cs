using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Admin;
using RVToolsWeb.Services.Admin;

namespace RVToolsWeb.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class VCenterManagementController : Controller
{
    private readonly IActiveVCentersService _vCenterService;

    public VCenterManagementController(IActiveVCentersService vCenterService)
    {
        _vCenterService = vCenterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new VCenterStatusViewModel
        {
            VCenters = await _vCenterService.GetAllVCentersAsync()
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(string viServer, bool isActive)
    {
        await _vCenterService.SetVCenterActiveAsync(viServer, isActive);
        TempData["Success"] = $"vCenter '{viServer}' marked as {(isActive ? "active" : "inactive")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateNotes(string viServer, string? notes)
    {
        await _vCenterService.UpdateNotesAsync(viServer, notes);
        TempData["Success"] = "Notes updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SyncFromImports()
    {
        await _vCenterService.SyncFromImportsAsync();
        TempData["Success"] = "vCenter list synchronized from import history.";
        return RedirectToAction(nameof(Index));
    }
}
