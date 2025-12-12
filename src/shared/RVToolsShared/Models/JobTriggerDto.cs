namespace RVToolsShared.Models;

/// <summary>
/// DTO representing a manual trigger request from Service.JobTriggers table.
/// Used for communication between web app and Windows service.
/// </summary>
public class JobTriggerDto
{
    public long TriggerId { get; set; }
    public int JobId { get; set; }
    public string TriggerType { get; set; } = "Manual";
    public string? TriggerUser { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
}

/// <summary>
/// Trigger types for job execution.
/// </summary>
public static class TriggerType
{
    public const string Manual = "Manual";
    public const string Scheduled = "Scheduled";
    public const string FileWatcher = "FileWatcher";
    public const string Reschedule = "Reschedule";
}
