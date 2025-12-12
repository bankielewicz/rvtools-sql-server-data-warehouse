using System.Collections.Concurrent;
using Dapper;
using Microsoft.Data.SqlClient;
using RVToolsShared.Data;
using RVToolsShared.Models;

namespace RVToolsService.Services;

/// <summary>
/// Service for monitoring incoming folders for new RVTools export files.
/// Uses FileSystemWatcher to detect new files and trigger imports automatically.
/// </summary>
public class FileMonitorService : IFileMonitorService, IDisposable
{
    private readonly ILogger<FileMonitorService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlConnectionFactory _connectionFactory;

    private readonly ConcurrentDictionary<int, FileWatcherContext> _watchers = new();
    private readonly ConcurrentQueue<PendingFile> _pendingFiles = new();
    private readonly object _lock = new();

    private CancellationTokenSource? _cts;
    private Task? _processingTask;
    private bool _isRunning;

    // Debounce settings
    private readonly TimeSpan _debounceDelay;
    private readonly TimeSpan _fileStabilityDelay;

    public FileMonitorService(
        ILogger<FileMonitorService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ISqlConnectionFactory connectionFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _connectionFactory = connectionFactory;

        // Configuration with defaults
        _debounceDelay = TimeSpan.FromSeconds(
            configuration.GetValue("FileMonitor:DebounceDelaySeconds", 5));
        _fileStabilityDelay = TimeSpan.FromSeconds(
            configuration.GetValue("FileMonitor:FileStabilityDelaySeconds", 2));
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("FileMonitorService is already running");
            return;
        }

        _logger.LogInformation("Starting FileMonitorService");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isRunning = true;

        // Load and start watchers for all enabled FileWatcher jobs
        await LoadWatchersAsync(_cts.Token);

        // Start background task to process pending files
        _processingTask = ProcessPendingFilesAsync(_cts.Token);

        _logger.LogInformation("FileMonitorService started with {Count} active watchers",
            _watchers.Count(w => w.Value.IsActive));
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            return;
        }

        _logger.LogInformation("Stopping FileMonitorService");

        _isRunning = false;
        _cts?.Cancel();

        // Stop all watchers
        foreach (var (jobId, context) in _watchers)
        {
            try
            {
                context.Watcher.EnableRaisingEvents = false;
                context.Watcher.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error stopping watcher for job {JobId}", jobId);
            }
        }
        _watchers.Clear();

        // Wait for processing task to complete
        if (_processingTask != null)
        {
            try
            {
                await _processingTask.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timed out waiting for file processing task to complete");
            }
        }

        _cts?.Dispose();
        _cts = null;

        _logger.LogInformation("FileMonitorService stopped");
    }

    public async Task ReloadWatchersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reloading file watchers from database");

        // Load current jobs from database
        var jobs = await GetFileWatcherJobsAsync(cancellationToken);
        var activeJobIds = new HashSet<int>(jobs.Select(j => j.JobId));

        // Remove watchers for jobs that no longer exist or are disabled
        var watchersToRemove = _watchers.Keys.Where(id => !activeJobIds.Contains(id)).ToList();
        foreach (var jobId in watchersToRemove)
        {
            if (_watchers.TryRemove(jobId, out var context))
            {
                _logger.LogInformation("Removing watcher for job {JobId} '{JobName}'",
                    jobId, context.JobName);
                context.Watcher.EnableRaisingEvents = false;
                context.Watcher.Dispose();
            }
        }

        // Add or update watchers for current jobs
        foreach (var job in jobs)
        {
            if (_watchers.TryGetValue(job.JobId, out var existing))
            {
                // Check if folder changed
                if (existing.FolderPath != job.IncomingFolder)
                {
                    _logger.LogInformation(
                        "Folder changed for job {JobId} '{JobName}': {OldFolder} -> {NewFolder}",
                        job.JobId, job.JobName, existing.FolderPath, job.IncomingFolder);

                    existing.Watcher.EnableRaisingEvents = false;
                    existing.Watcher.Dispose();
                    _watchers.TryRemove(job.JobId, out _);

                    // Re-add with new folder
                    AddWatcher(job);
                }
            }
            else
            {
                // New job - add watcher
                AddWatcher(job);
            }
        }

        _logger.LogInformation("File watchers reloaded: {Count} active",
            _watchers.Count(w => w.Value.IsActive));
    }

    public IReadOnlyList<FileWatcherStatus> GetWatcherStatuses()
    {
        return _watchers.Values.Select(w => new FileWatcherStatus
        {
            JobId = w.JobId,
            JobName = w.JobName,
            FolderPath = w.FolderPath,
            IsWatching = w.IsActive && w.Watcher.EnableRaisingEvents,
            FilesDetected = w.FilesDetected,
            LastFileDetectedAt = w.LastFileDetectedAt,
            LastError = w.LastError
        }).ToList();
    }

    private async Task LoadWatchersAsync(CancellationToken cancellationToken)
    {
        var jobs = await GetFileWatcherJobsAsync(cancellationToken);

        foreach (var job in jobs)
        {
            AddWatcher(job);
        }
    }

    private async Task<List<JobDto>> GetFileWatcherJobsAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT JobId, JobName, JobType, IsEnabled,
                   IncomingFolder, ProcessedFolder, ErrorsFolder,
                   CronSchedule, TimeZone,
                   ServerInstance, DatabaseName, UseWindowsAuth, EncryptedCredential,
                   VIServer, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
            FROM [Service].[Jobs]
            WHERE JobType = 'FileWatcher' AND IsEnabled = 1";

        using var connection = _connectionFactory.CreateConnection();
        var jobs = await connection.QueryAsync<JobDto>(sql);
        return jobs.ToList();
    }

    private void AddWatcher(JobDto job)
    {
        try
        {
            // Validate folder exists
            if (!Directory.Exists(job.IncomingFolder))
            {
                _logger.LogWarning(
                    "Incoming folder does not exist for job {JobId} '{JobName}': {Folder}",
                    job.JobId, job.JobName, job.IncomingFolder);

                _watchers[job.JobId] = new FileWatcherContext
                {
                    JobId = job.JobId,
                    JobName = job.JobName,
                    FolderPath = job.IncomingFolder,
                    Job = job,
                    Watcher = new FileSystemWatcher(), // Placeholder
                    IsActive = false,
                    LastError = $"Folder does not exist: {job.IncomingFolder}"
                };
                return;
            }

            var watcher = new FileSystemWatcher(job.IncomingFolder)
            {
                Filter = "*.xlsx",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = false
            };

            var context = new FileWatcherContext
            {
                JobId = job.JobId,
                JobName = job.JobName,
                FolderPath = job.IncomingFolder,
                Job = job,
                Watcher = watcher,
                IsActive = true
            };

            watcher.Created += (sender, args) => OnFileCreated(context, args);
            watcher.Renamed += (sender, args) => OnFileRenamed(context, args);
            watcher.Error += (sender, args) => OnWatcherError(context, args);

            _watchers[job.JobId] = context;
            watcher.EnableRaisingEvents = true;

            _logger.LogInformation(
                "Started watching folder for job {JobId} '{JobName}': {Folder}",
                job.JobId, job.JobName, job.IncomingFolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create watcher for job {JobId} '{JobName}': {Error}",
                job.JobId, job.JobName, ex.Message);

            _watchers[job.JobId] = new FileWatcherContext
            {
                JobId = job.JobId,
                JobName = job.JobName,
                FolderPath = job.IncomingFolder,
                Job = job,
                Watcher = new FileSystemWatcher(),
                IsActive = false,
                LastError = ex.Message
            };
        }
    }

    private void OnFileCreated(FileWatcherContext context, FileSystemEventArgs args)
    {
        HandleNewFile(context, args.FullPath);
    }

    private void OnFileRenamed(FileWatcherContext context, RenamedEventArgs args)
    {
        // Handle files that are renamed to .xlsx (e.g., temp file renamed after download)
        if (args.FullPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            HandleNewFile(context, args.FullPath);
        }
    }

    private void HandleNewFile(FileWatcherContext context, string filePath)
    {
        // Ignore temp files
        var fileName = Path.GetFileName(filePath);
        if (fileName.StartsWith("~$") || fileName.StartsWith("."))
        {
            return;
        }

        _logger.LogInformation(
            "Detected new file for job {JobId} '{JobName}': {FileName}",
            context.JobId, context.JobName, fileName);

        context.FilesDetected++;
        context.LastFileDetectedAt = DateTime.UtcNow;

        // Add to pending queue with debounce time
        _pendingFiles.Enqueue(new PendingFile
        {
            FilePath = filePath,
            Job = context.Job,
            DetectedAt = DateTime.UtcNow,
            ProcessAfter = DateTime.UtcNow.Add(_debounceDelay)
        });
    }

    private void OnWatcherError(FileWatcherContext context, ErrorEventArgs args)
    {
        var ex = args.GetException();
        _logger.LogError(ex,
            "FileSystemWatcher error for job {JobId} '{JobName}': {Error}",
            context.JobId, context.JobName, ex.Message);

        context.LastError = ex.Message;
        context.IsActive = false;

        // Try to restart the watcher
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            if (_isRunning)
            {
                _logger.LogInformation(
                    "Attempting to restart watcher for job {JobId} '{JobName}'",
                    context.JobId, context.JobName);

                try
                {
                    context.Watcher.EnableRaisingEvents = false;
                    await Task.Delay(1000);
                    context.Watcher.EnableRaisingEvents = true;
                    context.IsActive = true;
                    context.LastError = null;
                    _logger.LogInformation(
                        "Watcher restarted for job {JobId} '{JobName}'",
                        context.JobId, context.JobName);
                }
                catch (Exception restartEx)
                {
                    _logger.LogError(restartEx,
                        "Failed to restart watcher for job {JobId} '{JobName}'",
                        context.JobId, context.JobName);
                    context.LastError = restartEx.Message;
                }
            }
        });
    }

    private async Task ProcessPendingFilesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, cancellationToken);

                var now = DateTime.UtcNow;
                var filesToProcess = new List<PendingFile>();

                // Dequeue files that are ready to process
                while (_pendingFiles.TryPeek(out var pending))
                {
                    if (pending.ProcessAfter <= now)
                    {
                        if (_pendingFiles.TryDequeue(out var file))
                        {
                            // Check if file is stable (not being written to)
                            if (await IsFileStableAsync(file.FilePath, cancellationToken))
                            {
                                filesToProcess.Add(file);
                            }
                            else
                            {
                                // Re-queue with new delay
                                file.ProcessAfter = DateTime.UtcNow.Add(_fileStabilityDelay);
                                _pendingFiles.Enqueue(file);
                            }
                        }
                    }
                    else
                    {
                        break; // Files are ordered by ProcessAfter, so stop if next isn't ready
                    }
                }

                // Process ready files
                foreach (var file in filesToProcess)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await ProcessFileAsync(file, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in file processing loop");
            }
        }
    }

    private Task<bool> IsFileStableAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return Task.FromResult(false);
            }

            // Try to open the file exclusively
            try
            {
                using var stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);
                return Task.FromResult(true);
            }
            catch (IOException)
            {
                // File is locked/in use
                return Task.FromResult(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking file stability: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    private async Task ProcessFileAsync(PendingFile file, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing file for job {JobId}: {FilePath}",
            file.Job.JobId, file.FilePath);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IImportJobService>();

            var result = await importService.ExecuteImportAsync(
                file.FilePath,
                file.Job,
                "FileWatcher",
                "FileMonitorService",
                cancellationToken);

            if (result.Status == "Success")
            {
                _logger.LogInformation(
                    "File import completed successfully: {FilePath} -> {Status}",
                    file.FilePath, result.Status);
            }
            else
            {
                _logger.LogWarning(
                    "File import completed with issues: {FilePath} -> {Status}: {Error}",
                    file.FilePath, result.Status, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process file {FilePath} for job {JobId}: {Error}",
                file.FilePath, file.Job.JobId, ex.Message);
        }
    }

    public void Dispose()
    {
        StopAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Internal context for tracking a FileSystemWatcher instance.
    /// </summary>
    private class FileWatcherContext
    {
        public int JobId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public JobDto Job { get; set; } = new();
        public FileSystemWatcher Watcher { get; set; } = null!;
        public bool IsActive { get; set; }
        public int FilesDetected { get; set; }
        public DateTime? LastFileDetectedAt { get; set; }
        public string? LastError { get; set; }
    }

    /// <summary>
    /// Represents a file waiting to be processed after debounce period.
    /// </summary>
    private class PendingFile
    {
        public string FilePath { get; set; } = string.Empty;
        public JobDto Job { get; set; } = new();
        public DateTime DetectedAt { get; set; }
        public DateTime ProcessAfter { get; set; }
    }
}
