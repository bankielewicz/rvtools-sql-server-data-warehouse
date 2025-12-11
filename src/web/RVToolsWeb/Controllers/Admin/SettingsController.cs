using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Admin;
using RVToolsWeb.Services.Admin;

namespace RVToolsWeb.Controllers.Admin;

/// <summary>
/// Administration/Settings controller for managing configuration.
/// </summary>
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly ITableRetentionService _retentionService;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IDatabaseStatusService _statusService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISettingsService settingsService,
        ITableRetentionService retentionService,
        IAppSettingsService appSettingsService,
        IDatabaseStatusService statusService,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _retentionService = retentionService;
        _appSettingsService = appSettingsService;
        _statusService = statusService;
        _logger = logger;
    }

    /// <summary>
    /// Display the settings page with all tabs.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? tab = null)
    {
        var viewModel = new SettingsIndexViewModel
        {
            ActiveTab = tab ?? "general",
            GeneralSettings = await _settingsService.GetAllSettingsAsync(),
            TableRetentions = await _retentionService.GetAllRetentionsAsync(),
            AvailableTables = await _retentionService.GetAvailableTablesAsync(),
            AppSettings = await _appSettingsService.GetAppSettingsAsync(),
            DatabaseStatus = await _statusService.GetStatusAsync(),
            TableMappings = await _statusService.GetTableMappingsAsync(),
            TotalColumnMappings = await _statusService.GetColumnMappingCountAsync()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Update a Config.Settings value via AJAX.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.SettingName))
        {
            return BadRequest(new { success = false, error = "Setting name is required" });
        }

        var success = await _settingsService.UpdateSettingAsync(request.SettingName, request.SettingValue ?? "");
        return Json(new { success });
    }

    /// <summary>
    /// Add a table retention override.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRetention([FromBody] AddRetentionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.FullTableName))
        {
            return BadRequest(new { success = false, error = "Table name is required" });
        }

        // Parse schema.table from FullTableName
        var parts = request.FullTableName.Split('.', 2);
        if (parts.Length != 2)
        {
            return BadRequest(new { success = false, error = "Invalid table name format. Expected Schema.TableName" });
        }

        var success = await _retentionService.AddRetentionAsync(parts[0], parts[1], request.RetentionDays);
        return Json(new { success });
    }

    /// <summary>
    /// Update a table retention override.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRetention([FromBody] UpdateRetentionRequest request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest(new { success = false, error = "Valid retention ID is required" });
        }

        var success = await _retentionService.UpdateRetentionAsync(request.Id, request.RetentionDays);
        return Json(new { success });
    }

    /// <summary>
    /// Delete a table retention override.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRetention([FromBody] DeleteRetentionRequest request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest(new { success = false, error = "Valid retention ID is required" });
        }

        var success = await _retentionService.DeleteRetentionAsync(request.Id);
        return Json(new { success });
    }

    /// <summary>
    /// Update appsettings.json values.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAppSettings([FromBody] AppSettingsViewModel settings)
    {
        if (settings == null)
        {
            return BadRequest(new { success = false, error = "Settings data is required" });
        }

        var (success, error) = await _appSettingsService.UpdateAppSettingsAsync(settings);

        if (success)
        {
            TempData["SettingsSuccess"] = "Application settings updated. Some changes may require an application restart to take effect.";
        }

        return Json(new { success, error });
    }

    /// <summary>
    /// Refresh database status (for AJAX polling).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RefreshStatus()
    {
        var status = await _statusService.GetStatusAsync();
        return PartialView("_DatabaseStatusPartial", status);
    }
}

// Request DTOs for AJAX operations
public class UpdateSettingRequest
{
    public string? SettingName { get; set; }
    public string? SettingValue { get; set; }
}

public class AddRetentionRequest
{
    public string? FullTableName { get; set; }
    public int RetentionDays { get; set; }
}

public class UpdateRetentionRequest
{
    public int Id { get; set; }
    public int RetentionDays { get; set; }
}

public class DeleteRetentionRequest
{
    public int Id { get; set; }
}
