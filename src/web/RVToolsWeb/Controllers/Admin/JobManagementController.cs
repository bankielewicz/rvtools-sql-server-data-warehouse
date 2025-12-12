using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Admin;
using RVToolsWeb.Services.Admin;

namespace RVToolsWeb.Controllers.Admin;

/// <summary>
/// Controller for managing import jobs (Service.Jobs) and viewing job run history.
/// </summary>
[Authorize(Roles = "Admin")]
public class JobManagementController : Controller
{
    private readonly IJobManagementService _jobService;
    private readonly IWindowsServiceManager _serviceManager;
    private readonly ILogger<JobManagementController> _logger;

    public JobManagementController(
        IJobManagementService jobService,
        IWindowsServiceManager serviceManager,
        ILogger<JobManagementController> logger)
    {
        _jobService = jobService;
        _serviceManager = serviceManager;
        _logger = logger;
    }

    /// <summary>
    /// Display the job management index page with job list and service status.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new JobManagementIndexViewModel
        {
            Jobs = await _jobService.GetAllJobsAsync(),
            ServiceStatus = await _jobService.GetServiceStatusAsync(),
            RecentRuns = await _jobService.GetRecentJobRunsAsync(10),
            Statistics = await _jobService.GetStatisticsAsync(),
            WindowsServiceStatus = _serviceManager.GetServiceStatus(),
            IsUserLocalAdmin = _serviceManager.IsCurrentUserLocalAdmin()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Display the create job form.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        var model = new JobEditViewModel
        {
            JobType = "Scheduled",
            IsEnabled = true,
            UseWindowsAuth = true,
            ServerInstance = "localhost",
            DatabaseName = "RVToolsDW",
            TimeZone = "UTC"
        };

        ViewBag.TimeZones = GetTimeZones();
        return View(model);
    }

    /// <summary>
    /// Create a new job.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TimeZones = GetTimeZones();
            return View(model);
        }

        try
        {
            var jobId = await _jobService.CreateJobAsync(model, User.Identity?.Name ?? "admin");
            TempData["Success"] = $"Job '{model.JobName}' created successfully.";
            _logger.LogInformation("Job {JobId} '{JobName}' created by {User}",
                jobId, model.JobName, User.Identity?.Name);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create job: {Error}", ex.Message);
            ModelState.AddModelError("", $"Failed to create job: {ex.Message}");
            ViewBag.TimeZones = GetTimeZones();
            return View(model);
        }
    }

    /// <summary>
    /// Display the edit job form.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var job = await _jobService.GetJobByIdAsync(id);
        if (job == null)
        {
            TempData["Error"] = "Job not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new JobEditViewModel
        {
            JobId = job.JobId,
            JobName = job.JobName,
            JobType = job.JobType,
            IsEnabled = job.IsEnabled,
            IncomingFolder = job.IncomingFolder,
            ProcessedFolder = job.ProcessedFolder,
            ErrorsFolder = job.ErrorsFolder,
            CronSchedule = job.CronSchedule,
            TimeZone = job.TimeZone,
            ServerInstance = job.ServerInstance,
            DatabaseName = job.DatabaseName,
            UseWindowsAuth = job.UseWindowsAuth,
            VIServer = job.VIServer,
            HasExistingCredential = job.HasCredential
        };

        ViewBag.TimeZones = GetTimeZones();
        return View(model);
    }

    /// <summary>
    /// Update an existing job.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(JobEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TimeZones = GetTimeZones();
            return View(model);
        }

        try
        {
            var success = await _jobService.UpdateJobAsync(model, User.Identity?.Name ?? "admin");
            if (success)
            {
                TempData["Success"] = $"Job '{model.JobName}' updated successfully.";
                _logger.LogInformation("Job {JobId} '{JobName}' updated by {User}",
                    model.JobId, model.JobName, User.Identity?.Name);
            }
            else
            {
                TempData["Error"] = "Job not found or update failed.";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job {JobId}: {Error}", model.JobId, ex.Message);
            ModelState.AddModelError("", $"Failed to update job: {ex.Message}");
            ViewBag.TimeZones = GetTimeZones();
            return View(model);
        }
    }

    /// <summary>
    /// Delete a job.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var job = await _jobService.GetJobByIdAsync(id);
            var success = await _jobService.DeleteJobAsync(id);
            if (success)
            {
                TempData["Success"] = $"Job '{job?.JobName ?? id.ToString()}' deleted successfully.";
                _logger.LogInformation("Job {JobId} deleted by {User}", id, User.Identity?.Name);
            }
            else
            {
                TempData["Error"] = "Job not found or delete failed.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete job {JobId}: {Error}", id, ex.Message);
            TempData["Error"] = $"Failed to delete job: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Trigger a job to run immediately.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TriggerNow(int id)
    {
        try
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return Json(new { success = false, error = "Job not found." });
            }

            if (!job.IsEnabled)
            {
                return Json(new { success = false, error = "Job is disabled." });
            }

            var success = await _jobService.TriggerJobNowAsync(id, User.Identity?.Name ?? "admin");
            if (success)
            {
                _logger.LogInformation("Manual trigger created for job {JobId} '{JobName}' by {User}",
                    id, job.JobName, User.Identity?.Name);
                return Json(new { success = true, message = $"Job '{job.JobName}' triggered successfully. It will start within 10 seconds." });
            }
            else
            {
                return Json(new { success = false, error = "Failed to create trigger." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger job {JobId}: {Error}", id, ex.Message);
            return Json(new { success = false, error = $"Failed to trigger job: {ex.Message}" });
        }
    }

    /// <summary>
    /// Enable or disable a job via AJAX.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetEnabled(int id, bool enabled)
    {
        try
        {
            var success = await _jobService.SetJobEnabledAsync(id, enabled);
            if (success)
            {
                _logger.LogInformation("Job {JobId} {Action} by {User}",
                    id, enabled ? "enabled" : "disabled", User.Identity?.Name);
                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Job not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set job {JobId} enabled state: {Error}", id, ex.Message);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Display job run history for a specific job.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> History(int id)
    {
        var job = await _jobService.GetJobByIdAsync(id);
        if (job == null)
        {
            TempData["Error"] = "Job not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new JobRunHistoryViewModel
        {
            JobId = job.JobId,
            JobName = job.JobName,
            Runs = await _jobService.GetJobRunsAsync(id, 100)
        };

        return View(viewModel);
    }

    /// <summary>
    /// Display details for a specific job run.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RunDetails(long id)
    {
        var run = await _jobService.GetJobRunByIdAsync(id);
        if (run == null)
        {
            TempData["Error"] = "Job run not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(run);
    }

    /// <summary>
    /// Get service status for AJAX polling.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ServiceStatus()
    {
        var status = await _jobService.GetServiceStatusAsync();
        return PartialView("_ServiceStatusPartial", status);
    }

    #region Windows Service Control

    /// <summary>
    /// Get Windows Service status for AJAX polling.
    /// </summary>
    [HttpGet]
    public IActionResult WindowsServiceStatus()
    {
        var status = _serviceManager.GetServiceStatus();
        var isAdmin = _serviceManager.IsCurrentUserLocalAdmin();
        return Json(new { status, isAdmin });
    }

    /// <summary>
    /// Start the Windows Service.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartService()
    {
        if (!_serviceManager.IsCurrentUserLocalAdmin())
        {
            return Json(new { success = false, error = "Local administrator privileges required" });
        }

        _logger.LogInformation("User {User} requested service start", User.Identity?.Name);
        var result = await _serviceManager.StartServiceAsync();
        return Json(new { success = result.Success, message = result.Message, error = result.ErrorDetails });
    }

    /// <summary>
    /// Stop the Windows Service.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StopService()
    {
        if (!_serviceManager.IsCurrentUserLocalAdmin())
        {
            return Json(new { success = false, error = "Local administrator privileges required" });
        }

        _logger.LogInformation("User {User} requested service stop", User.Identity?.Name);
        var result = await _serviceManager.StopServiceAsync();
        return Json(new { success = result.Success, message = result.Message, error = result.ErrorDetails });
    }

    /// <summary>
    /// Install the Windows Service.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InstallService()
    {
        if (!_serviceManager.IsCurrentUserLocalAdmin())
        {
            return Json(new { success = false, error = "Local administrator privileges required" });
        }

        _logger.LogInformation("User {User} requested service install", User.Identity?.Name);
        var result = await _serviceManager.InstallServiceAsync();
        return Json(new { success = result.Success, message = result.Message, error = result.ErrorDetails });
    }

    /// <summary>
    /// Uninstall the Windows Service.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UninstallService()
    {
        if (!_serviceManager.IsCurrentUserLocalAdmin())
        {
            return Json(new { success = false, error = "Local administrator privileges required" });
        }

        _logger.LogInformation("User {User} requested service uninstall", User.Identity?.Name);
        var result = await _serviceManager.UninstallServiceAsync();
        return Json(new { success = result.Success, message = result.Message, error = result.ErrorDetails });
    }

    #endregion

    /// <summary>
    /// Get available time zones for scheduling.
    /// </summary>
    private static List<SelectListItem> GetTimeZones()
    {
        return TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => new SelectListItem
            {
                Value = tz.Id,
                Text = tz.DisplayName
            })
            .ToList();
    }
}

/// <summary>
/// Select list item for dropdowns (local definition to avoid Microsoft.AspNetCore.Mvc.Rendering dependency in model).
/// </summary>
public class SelectListItem
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
