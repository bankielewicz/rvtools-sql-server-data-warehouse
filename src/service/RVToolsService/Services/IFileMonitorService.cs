namespace RVToolsService.Services;

/// <summary>
/// Service for monitoring incoming folders for new RVTools export files.
/// Uses FileSystemWatcher to detect new files and trigger imports.
/// </summary>
public interface IFileMonitorService
{
    /// <summary>
    /// Start monitoring all enabled FileWatcher jobs.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop all file watchers.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reload watchers from database (call when jobs are added/removed/enabled/disabled).
    /// </summary>
    Task ReloadWatchersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the current status of all active watchers.
    /// </summary>
    IReadOnlyList<FileWatcherStatus> GetWatcherStatuses();
}

/// <summary>
/// Status information for an active file watcher.
/// </summary>
public class FileWatcherStatus
{
    public int JobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public bool IsWatching { get; set; }
    public int FilesDetected { get; set; }
    public DateTime? LastFileDetectedAt { get; set; }
    public string? LastError { get; set; }
}
