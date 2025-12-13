namespace RVToolsWeb.Models.DTOs;

/// <summary>
/// DTO for job run status returned by the GetJobStatus endpoint.
/// Used for AJAX polling to track job progress after triggering.
/// </summary>
public class JobRunStatusDto
{
    public long JobRunId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationSeconds { get; set; }
    public int? FilesProcessed { get; set; }
    public int? FilesFailed { get; set; }
    public string? ErrorMessage { get; set; }
}
