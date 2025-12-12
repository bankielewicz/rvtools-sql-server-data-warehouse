using System.ComponentModel.DataAnnotations;

namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// ViewModel for displaying a job in the list.
/// </summary>
public class JobViewModel
{
    public int JobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string JobType { get; set; } = "Scheduled";
    public bool IsEnabled { get; set; }

    public string IncomingFolder { get; set; } = string.Empty;
    public string? ProcessedFolder { get; set; }
    public string? ErrorsFolder { get; set; }

    public string? CronSchedule { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string? CronDescription { get; set; }

    public string ServerInstance { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "RVToolsDW";
    public bool UseWindowsAuth { get; set; }
    public bool HasCredential { get; set; }

    public string? VIServer { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Computed/UI properties
    public string StatusDisplay => IsEnabled ? "Enabled" : "Disabled";
    public string AuthTypeDisplay => UseWindowsAuth ? "Windows" : "SQL Auth";
    public DateTime? LastRunTime { get; set; }
    public string? LastRunStatus { get; set; }
    public DateTime? NextRunTime { get; set; }
}

/// <summary>
/// ViewModel for creating/editing a job.
/// </summary>
public class JobEditViewModel
{
    public int JobId { get; set; }

    [Required(ErrorMessage = "Job name is required")]
    [StringLength(100, ErrorMessage = "Job name cannot exceed 100 characters")]
    [Display(Name = "Job Name")]
    public string JobName { get; set; } = string.Empty;

    [Display(Name = "Job Type")]
    public string JobType { get; set; } = "Scheduled";

    [Display(Name = "Enabled")]
    public bool IsEnabled { get; set; } = true;

    [Required(ErrorMessage = "Incoming folder is required")]
    [StringLength(500, ErrorMessage = "Path cannot exceed 500 characters")]
    [Display(Name = "Incoming Folder")]
    public string IncomingFolder { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Path cannot exceed 500 characters")]
    [Display(Name = "Processed Folder")]
    public string? ProcessedFolder { get; set; }

    [StringLength(500, ErrorMessage = "Path cannot exceed 500 characters")]
    [Display(Name = "Errors Folder")]
    public string? ErrorsFolder { get; set; }

    [StringLength(100, ErrorMessage = "Cron expression cannot exceed 100 characters")]
    [Display(Name = "Cron Schedule")]
    public string? CronSchedule { get; set; }

    [Display(Name = "Time Zone")]
    public string TimeZone { get; set; } = "UTC";

    [Required(ErrorMessage = "Server instance is required")]
    [StringLength(200, ErrorMessage = "Server instance cannot exceed 200 characters")]
    [Display(Name = "Server Instance")]
    public string ServerInstance { get; set; } = "localhost";

    [StringLength(100, ErrorMessage = "Database name cannot exceed 100 characters")]
    [Display(Name = "Database")]
    public string DatabaseName { get; set; } = "RVToolsDW";

    [Display(Name = "Use Windows Authentication")]
    public bool UseWindowsAuth { get; set; } = true;

    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
    [Display(Name = "SQL Username")]
    public string? SqlUsername { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "SQL Password")]
    public string? SqlPassword { get; set; }

    [StringLength(100, ErrorMessage = "VIServer cannot exceed 100 characters")]
    [Display(Name = "vCenter Server")]
    public string? VIServer { get; set; }

    // Indicates if job already has stored credentials
    public bool HasExistingCredential { get; set; }
}

/// <summary>
/// ViewModel for displaying job run history.
/// </summary>
public class JobRunViewModel
{
    public long JobRunId { get; set; }
    public int JobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public int? ImportBatchId { get; set; }

    public string TriggerType { get; set; } = string.Empty;
    public string? TriggerUser { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationMs { get; set; }

    public string Status { get; set; } = "Running";
    public int FilesProcessed { get; set; }
    public int FilesFailed { get; set; }

    public string? ErrorMessage { get; set; }

    // Computed properties
    public string DurationDisplay => DurationMs.HasValue
        ? DurationMs.Value > 60000
            ? $"{DurationMs.Value / 60000}m {(DurationMs.Value % 60000) / 1000}s"
            : $"{DurationMs.Value / 1000}s"
        : "Running...";

    public string StatusCssClass => Status switch
    {
        "Success" => "text-success",
        "PartialSuccess" => "text-warning",
        "Failed" => "text-danger",
        "Running" => "text-info",
        "Cancelled" => "text-secondary",
        _ => ""
    };
}

/// <summary>
/// ViewModel for displaying pending triggers.
/// </summary>
public class JobTriggerViewModel
{
    public long TriggerId { get; set; }
    public int JobId { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public string? TriggerUser { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }

    public bool IsProcessed => ProcessedDate.HasValue;
}

/// <summary>
/// ViewModel for displaying service status.
/// </summary>
public class ServiceStatusViewModel
{
    public string ServiceName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastHeartbeat { get; set; }
    public string? ServiceVersion { get; set; }
    public int ActiveJobs { get; set; }
    public int QueuedJobs { get; set; }

    // Computed properties
    public bool IsHealthy => Status == "Running" && (DateTime.UtcNow - LastHeartbeat).TotalMinutes < 2;
    public string StatusDisplay => IsHealthy ? "Running" : Status == "Running" ? "Stale" : Status;
    public string StatusCssClass => IsHealthy ? "text-success" : Status == "Stopped" ? "text-warning" : "text-danger";
    public string TimeSinceHeartbeat
    {
        get
        {
            var span = DateTime.UtcNow - LastHeartbeat;
            if (span.TotalMinutes < 1) return $"{(int)span.TotalSeconds}s ago";
            if (span.TotalHours < 1) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalDays < 1) return $"{(int)span.TotalHours}h ago";
            return $"{(int)span.TotalDays}d ago";
        }
    }
}

/// <summary>
/// ViewModel for the job management index page.
/// </summary>
public class JobManagementIndexViewModel
{
    public IEnumerable<JobViewModel> Jobs { get; set; } = Enumerable.Empty<JobViewModel>();
    public ServiceStatusViewModel? ServiceStatus { get; set; }
    public IEnumerable<JobRunViewModel> RecentRuns { get; set; } = Enumerable.Empty<JobRunViewModel>();
    public JobStatisticsViewModel Statistics { get; set; } = new();
}

/// <summary>
/// Statistics for the dashboard.
/// </summary>
public class JobStatisticsViewModel
{
    public int TotalJobs { get; set; }
    public int EnabledJobs { get; set; }
    public int ScheduledJobs { get; set; }
    public int FileWatcherJobs { get; set; }
    public int ManualJobs { get; set; }

    // 24-hour statistics
    public int RunsLast24Hours { get; set; }
    public int SuccessfulRunsLast24Hours { get; set; }
    public int FailedRunsLast24Hours { get; set; }
    public int FilesProcessedLast24Hours { get; set; }

    // Computed properties
    public double SuccessRateLast24Hours => RunsLast24Hours > 0
        ? Math.Round((double)SuccessfulRunsLast24Hours / RunsLast24Hours * 100, 1)
        : 0;
    public string SuccessRateDisplay => $"{SuccessRateLast24Hours}%";
}

/// <summary>
/// ViewModel for job run history page.
/// </summary>
public class JobRunHistoryViewModel
{
    public int JobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public IEnumerable<JobRunViewModel> Runs { get; set; } = Enumerable.Empty<JobRunViewModel>();
}
