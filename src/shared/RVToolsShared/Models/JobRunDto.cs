namespace RVToolsShared.Models;

/// <summary>
/// DTO representing a job execution record from Service.JobRuns table.
/// </summary>
public class JobRunDto
{
    public long JobRunId { get; set; }
    public int JobId { get; set; }
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

    // Navigation/computed properties (not in database)
    public string? JobName { get; set; }
}

/// <summary>
/// DTO for creating a new job run record.
/// </summary>
public class CreateJobRunDto
{
    public int JobId { get; set; }
    public string TriggerType { get; set; } = "Manual";
    public string? TriggerUser { get; set; }
}

/// <summary>
/// Status values for job runs.
/// </summary>
public static class JobRunStatus
{
    public const string Running = "Running";
    public const string Success = "Success";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string PartialSuccess = "PartialSuccess";
}
