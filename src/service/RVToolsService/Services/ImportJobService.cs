using System.Diagnostics;
using System.Text.RegularExpressions;
using Dapper;
using RVToolsShared.Data;
using RVToolsShared.Models;
using RVToolsShared.Security;

namespace RVToolsService.Services;

/// <summary>
/// Core service for orchestrating RVTools import jobs.
/// Ports PowerShell Import-RVToolsFile logic to C#.
/// </summary>
public class ImportJobService : IImportJobService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly IExcelReaderService _excelReader;
    private readonly IStagingService _stagingService;
    private readonly IBatchService _batchService;
    private readonly ICredentialProtectionService _credentialService;
    private readonly ILogger<ImportJobService> _logger;

    // Processing order (important sheets first, matching PowerShell)
    private static readonly string[] SheetProcessingOrder =
    {
        "vInfo", "vCPU", "vMemory", "vDisk", "vPartition", "vNetwork",
        "vSnapshot", "vTools", "vHost", "vCluster", "vDatastore", "vHealth",
        "vCD", "vUSB", "vSource", "vRP", "vHBA", "vNIC", "vSwitch", "vPort",
        "dvSwitch", "dvPort", "vSC_VMK", "vMultiPath", "vLicense", "vFileInfo", "vMetaData"
    };

    // Regex for parsing historical filename dates: {vcenter}_{m_d_yyyy}.{domain}.xlsx
    private static readonly Regex FilenameDatePattern = new(
        @"^([a-zA-Z0-9-]+)_(\d{1,2})_(\d{1,2})_(\d{4})\.[^.]+\.[^.]+.*\.xlsx$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ImportJobService(
        ISqlConnectionFactory connectionFactory,
        IExcelReaderService excelReader,
        IStagingService stagingService,
        IBatchService batchService,
        ICredentialProtectionService credentialService,
        ILogger<ImportJobService> logger)
    {
        _connectionFactory = connectionFactory;
        _excelReader = excelReader;
        _stagingService = stagingService;
        _batchService = batchService;
        _credentialService = credentialService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ImportResult> ExecuteImportAsync(
        string filePath,
        JobDto job,
        string triggerType,
        string? triggerUser = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var fileName = Path.GetFileName(filePath);
        var result = new ImportResult { FileName = fileName };

        _logger.LogInformation("Starting import of '{FileName}'", fileName);

        int? importBatchId = null;

        try
        {
            // Validate file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}", filePath);
            }

            // Parse filename for date and VIServer (for historical imports)
            var exportInfo = ParseFilenameInfo(fileName);
            var viServer = job.VIServer ?? exportInfo.VIServer;
            var exportDate = exportInfo.ExportDate;

            // Create import batch
            importBatchId = await _batchService.CreateBatchAsync(fileName, viServer, exportDate);
            result.ImportBatchId = importBatchId.Value;

            // Read Excel file
            _logger.LogDebug("Reading Excel file '{FileName}'", fileName);
            var sheets = await _excelReader.ReadAllSheetsAsync(filePath);

            if (sheets.Count == 0)
            {
                throw new InvalidOperationException($"No readable sheets found in file: {fileName}");
            }

            _logger.LogInformation("Found {SheetCount} sheets in '{FileName}'", sheets.Count, fileName);

            // Process sheets in order
            var sheetResults = new List<SheetImportResult>();
            var sheetsInFile = sheets.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var sheetName in SheetProcessingOrder)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!sheetsInFile.Contains(sheetName))
                    continue;

                var sheetData = sheets.First(s =>
                    string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase));

                try
                {
                    var sheetResult = await _stagingService.ImportSheetToStagingAsync(
                        sheetData, importBatchId.Value);
                    sheetResults.Add(sheetResult);

                    result.TotalSourceRows += sheetResult.SourceRows;
                    result.TotalStagedRows += sheetResult.StagedRows;
                    result.TotalFailedRows += sheetResult.FailedRows;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sheet '{SheetName}' failed: {Error}", sheetName, ex.Message);
                    result.TotalFailedRows++;
                }
            }

            result.SheetsProcessed = sheetResults.Count;

            // Process staged data through usp_ProcessImport
            if (result.TotalStagedRows > 0)
            {
                _logger.LogInformation("Processing staged data to Current/History tables...");
                await ExecuteProcessImportAsync(importBatchId.Value, fileName, exportDate);
            }

            // Determine status based on failure rate
            var failurePercent = result.TotalSourceRows > 0
                ? (result.TotalFailedRows * 100.0 / result.TotalSourceRows)
                : 0;

            result.Status = result.TotalFailedRows == 0 ? "Success"
                          : failurePercent < 50 ? "Partial"
                          : "Failed";

            // Update batch with final stats
            await _batchService.UpdateBatchAsync(
                importBatchId.Value,
                result.Status,
                sheets.Count,
                result.SheetsProcessed,
                result.TotalSourceRows,
                result.TotalStagedRows,
                result.TotalFailedRows);

            // Move file to appropriate folder
            result.DestinationPath = await MoveFileAsync(
                filePath,
                result.Status == "Failed" ? job.ErrorsFolder : job.ProcessedFolder,
                job.IncomingFolder);

            stopwatch.Stop();
            result.DurationMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Import complete: Status={Status}, Duration={Duration}ms, Staged={Staged}, Failed={Failed}, File moved to {Destination}",
                result.Status, result.DurationMs, result.TotalStagedRows, result.TotalFailedRows, result.DestinationPath);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            result.Status = "Failed";
            result.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Import failed for '{FileName}': {Error}", fileName, ex.Message);

            // Update batch if created
            if (importBatchId.HasValue)
            {
                await _batchService.FailBatchAsync(importBatchId.Value, ex.Message);
            }

            // Move to errors folder
            try
            {
                result.DestinationPath = await MoveFileAsync(filePath, job.ErrorsFolder, job.IncomingFolder);
            }
            catch (Exception moveEx)
            {
                _logger.LogWarning(moveEx, "Failed to move file to errors folder: {Error}", moveEx.Message);
            }

            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<JobRunResult> ExecuteJobAsync(
        JobDto job,
        string triggerType,
        string? triggerUser = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new JobRunResult();

        _logger.LogInformation("Starting job '{JobName}' (trigger: {TriggerType})", job.JobName, triggerType);

        // Create job run record
        result.JobRunId = await CreateJobRunAsync(job.JobId, triggerType, triggerUser);

        try
        {
            // Get files from incoming folder
            if (!Directory.Exists(job.IncomingFolder))
            {
                throw new DirectoryNotFoundException($"Incoming folder not found: {job.IncomingFolder}");
            }

            var files = Directory.GetFiles(job.IncomingFolder, "*.xlsx", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f) // Process in alphabetical order
                .ToList();

            if (files.Count == 0)
            {
                _logger.LogInformation("No xlsx files found in '{Folder}'", job.IncomingFolder);
                result.Status = "Success";
                await UpdateJobRunAsync(result.JobRunId, "Success", 0, 0);
                return result;
            }

            _logger.LogInformation("Found {FileCount} xlsx file(s) to process", files.Count);

            // Process each file
            foreach (var filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileResult = await ExecuteImportAsync(filePath, job, triggerType, triggerUser, cancellationToken);
                result.FileResults.Add(fileResult);

                if (fileResult.Status == "Failed")
                {
                    result.FilesFailed++;
                }
                else
                {
                    result.FilesProcessed++;
                }

                result.TotalRowsStaged += fileResult.TotalStagedRows;
                result.TotalRowsFailed += fileResult.TotalFailedRows;
            }

            // Determine overall status
            result.Status = result.FilesFailed == 0 ? "Success"
                          : result.FilesProcessed == 0 ? "Failed"
                          : "PartialSuccess";

            stopwatch.Stop();
            result.DurationMs = (int)stopwatch.ElapsedMilliseconds;

            // Update job run record
            await UpdateJobRunAsync(result.JobRunId, result.Status, result.FilesProcessed, result.FilesFailed);

            _logger.LogInformation(
                "Job '{JobName}' complete: Status={Status}, Files={Processed}/{Total}, Staged={Staged}, Duration={Duration}ms",
                job.JobName, result.Status, result.FilesProcessed, files.Count, result.TotalRowsStaged, result.DurationMs);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            result.Status = "Failed";
            result.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Job '{JobName}' failed: {Error}", job.JobName, ex.Message);

            await UpdateJobRunAsync(result.JobRunId, "Failed", result.FilesProcessed, result.FilesFailed, ex.Message);

            return result;
        }
    }

    /// <summary>
    /// Parses filename for VIServer and export date (historical import support).
    /// Pattern: {vcenter-name}_{m_d_yyyy}.{domain.tld}.xlsx
    /// </summary>
    private (string? VIServer, DateTime? ExportDate) ParseFilenameInfo(string fileName)
    {
        var match = FilenameDatePattern.Match(fileName);
        if (!match.Success)
            return (null, null);

        try
        {
            var viServer = match.Groups[1].Value;
            var month = int.Parse(match.Groups[2].Value);
            var day = int.Parse(match.Groups[3].Value);
            var year = int.Parse(match.Groups[4].Value);

            var exportDate = new DateTime(year, month, day);
            return (viServer, exportDate);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Failed to parse date from filename '{FileName}': {Error}", fileName, ex.Message);
            return (null, null);
        }
    }

    /// <summary>
    /// Executes usp_ProcessImport to merge staged data to Current/History.
    /// </summary>
    private async Task ExecuteProcessImportAsync(int importBatchId, string sourceFile, DateTime? exportDate)
    {
        var sql = @"
            SET QUOTED_IDENTIFIER ON;
            SET ANSI_NULLS ON;
            EXEC dbo.usp_ProcessImport
                @ImportBatchId = @ImportBatchId,
                @SourceFile = @SourceFile" +
            (exportDate.HasValue ? ", @RVToolsExportDate = @ExportDate" : "");

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        try
        {
            await connection.ExecuteAsync(sql, new
            {
                ImportBatchId = importBatchId,
                SourceFile = sourceFile,
                ExportDate = exportDate
            }, commandTimeout: 600);

            _logger.LogInformation("usp_ProcessImport completed for batch {BatchId}", importBatchId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "usp_ProcessImport failed for batch {BatchId}: {Error}",
                importBatchId, ex.Message);
            // Don't rethrow - staging data is preserved for retry
        }
    }

    /// <summary>
    /// Moves a file to the destination folder with date suffix.
    /// </summary>
    private async Task<string> MoveFileAsync(string sourcePath, string? destinationFolder, string fallbackFolder)
    {
        var folder = !string.IsNullOrEmpty(destinationFolder) ? destinationFolder
            : Path.Combine(Path.GetDirectoryName(fallbackFolder) ?? fallbackFolder, "processed");

        // Ensure destination folder exists
        Directory.CreateDirectory(folder);

        // Add date suffix to filename
        var fileName = Path.GetFileNameWithoutExtension(sourcePath);
        var extension = Path.GetExtension(sourcePath);
        var dateSuffix = DateTime.Now.ToString("yyyyMMdd");
        var newFileName = $"{fileName}.{dateSuffix}{extension}";

        var destinationPath = Path.Combine(folder, newFileName);

        // Handle existing file
        if (File.Exists(destinationPath))
        {
            var timestamp = DateTime.Now.ToString("HHmmss");
            newFileName = $"{fileName}.{dateSuffix}_{timestamp}{extension}";
            destinationPath = Path.Combine(folder, newFileName);
        }

        await Task.Run(() => File.Move(sourcePath, destinationPath, overwrite: true));

        _logger.LogDebug("Moved file from '{Source}' to '{Destination}'", sourcePath, destinationPath);
        return destinationPath;
    }

    /// <summary>
    /// Creates a job run record.
    /// </summary>
    private async Task<long> CreateJobRunAsync(int jobId, string triggerType, string? triggerUser)
    {
        const string sql = @"
            INSERT INTO [Service].[JobRuns] (JobId, TriggerType, TriggerUser, StartTime, Status)
            OUTPUT INSERTED.JobRunId
            VALUES (@JobId, @TriggerType, @TriggerUser, GETUTCDATE(), 'Running')";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<long>(sql, new
        {
            JobId = jobId,
            TriggerType = triggerType,
            TriggerUser = triggerUser
        });
    }

    /// <summary>
    /// Updates a job run record with final status.
    /// </summary>
    private async Task UpdateJobRunAsync(long jobRunId, string status, int filesProcessed, int filesFailed, string? errorMessage = null)
    {
        const string sql = @"
            UPDATE [Service].[JobRuns]
            SET EndTime = GETUTCDATE(),
                DurationMs = DATEDIFF(MILLISECOND, StartTime, GETUTCDATE()),
                Status = @Status,
                FilesProcessed = @FilesProcessed,
                FilesFailed = @FilesFailed,
                ErrorMessage = @ErrorMessage
            WHERE JobRunId = @JobRunId";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new
        {
            JobRunId = jobRunId,
            Status = status,
            FilesProcessed = filesProcessed,
            FilesFailed = filesFailed,
            ErrorMessage = errorMessage
        });
    }
}
