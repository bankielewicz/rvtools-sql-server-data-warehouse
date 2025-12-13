using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service interface for managing import jobs (Service.Jobs, JobRuns, JobTriggers tables).
/// </summary>
public interface IJobManagementService
{
    // ===== Jobs =====
    /// <summary>
    /// Get all jobs.
    /// </summary>
    Task<IEnumerable<JobViewModel>> GetAllJobsAsync();

    /// <summary>
    /// Get a single job by ID.
    /// </summary>
    Task<JobViewModel?> GetJobByIdAsync(int jobId);

    /// <summary>
    /// Create a new job.
    /// </summary>
    Task<int> CreateJobAsync(JobEditViewModel model, string createdBy);

    /// <summary>
    /// Update an existing job.
    /// </summary>
    Task<bool> UpdateJobAsync(JobEditViewModel model, string modifiedBy);

    /// <summary>
    /// Delete a job.
    /// </summary>
    Task<bool> DeleteJobAsync(int jobId);

    /// <summary>
    /// Enable or disable a job.
    /// </summary>
    Task<bool> SetJobEnabledAsync(int jobId, bool isEnabled);

    // ===== Job Runs =====
    /// <summary>
    /// Get recent job runs for a specific job.
    /// </summary>
    Task<IEnumerable<JobRunViewModel>> GetJobRunsAsync(int jobId, int limit = 50);

    /// <summary>
    /// Get recent job runs across all jobs.
    /// </summary>
    Task<IEnumerable<JobRunViewModel>> GetRecentJobRunsAsync(int limit = 50);

    /// <summary>
    /// Get a single job run by ID.
    /// </summary>
    Task<JobRunViewModel?> GetJobRunByIdAsync(long jobRunId);

    // ===== Triggers =====
    /// <summary>
    /// Trigger a job to run immediately (inserts into Service.JobTriggers).
    /// </summary>
    Task<bool> TriggerJobNowAsync(int jobId, string triggerUser);

    /// <summary>
    /// Get pending triggers for a job.
    /// </summary>
    Task<IEnumerable<JobTriggerViewModel>> GetPendingTriggersAsync(int jobId);

    /// <summary>
    /// Check if a job has pending (unprocessed) triggers.
    /// </summary>
    Task<bool> HasPendingTriggerAsync(int jobId);

    // ===== Job Status (for AJAX polling) =====
    /// <summary>
    /// Get the latest job run for polling job progress.
    /// </summary>
    Task<Models.DTOs.JobRunStatusDto?> GetLatestJobRunAsync(int jobId);

    // ===== Service Status =====
    /// <summary>
    /// Get the current service status.
    /// </summary>
    Task<ServiceStatusViewModel?> GetServiceStatusAsync();

    // ===== Statistics =====
    /// <summary>
    /// Get job statistics for the dashboard.
    /// </summary>
    Task<JobStatisticsViewModel> GetStatisticsAsync();
}
