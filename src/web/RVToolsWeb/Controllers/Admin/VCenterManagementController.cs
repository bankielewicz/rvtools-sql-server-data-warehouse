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
    [IgnoreAntiforgeryToken]  // Safe: protected by [Authorize] + cookie auth
    public async Task<IActionResult> ToggleActiveAjax([FromBody] ToggleActiveRequest request)
    {
        if (string.IsNullOrEmpty(request.ViServer))
            return Json(new { success = false, error = "vCenter name required" });

        await _vCenterService.SetVCenterActiveAsync(request.ViServer, request.IsActive);
        return Json(new { success = true, isActive = request.IsActive });
    }

    public class ToggleActiveRequest
    {
        public string ViServer { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateNotes(string viServer, string? notes)
    {
        await _vCenterService.UpdateNotesAsync(viServer, notes);
        TempData["Success"] = "Notes updated.";
        return RedirectToAction(nameof(Index));
    }

}
