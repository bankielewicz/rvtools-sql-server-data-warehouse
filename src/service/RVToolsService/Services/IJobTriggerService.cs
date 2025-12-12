using RVToolsShared.Models;

namespace RVToolsService.Services;

/// <summary>
/// Service for managing job triggers (manual and scheduled).
/// Handles polling for pending triggers and processing them.
/// </summary>
public interface IJobTriggerService
{
    /// <summary>
    /// Gets all pending (unprocessed) triggers.
    /// </summary>
    /// <returns>List of pending triggers with associated job info</returns>
    Task<List<PendingTrigger>> GetPendingTriggersAsync();

    /// <summary>
    /// Marks a trigger as processed.
    /// </summary>
    /// <param name="triggerId">Trigger ID to mark</param>
    Task MarkTriggerProcessedAsync(long triggerId);

    /// <summary>
    /// Gets a job by ID.
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <returns>Job configuration or null if not found</returns>
    Task<JobDto?> GetJobAsync(int jobId);

    /// <summary>
    /// Gets all enabled jobs.
    /// </summary>
    /// <returns>List of enabled jobs</returns>
    Task<List<JobDto>> GetEnabledJobsAsync();
}

/// <summary>
/// Represents a pending trigger with job information.
/// </summary>
public class PendingTrigger
{
    public long TriggerId { get; set; }
    public int JobId { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public string? TriggerUser { get; set; }
    public DateTime CreatedDate { get; set; }

    // Job info (joined from Service.Jobs)
    public string JobName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
