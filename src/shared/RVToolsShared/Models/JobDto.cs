namespace RVToolsShared.Models;

/// <summary>
/// DTO representing an import job configuration from Service.Jobs table.
/// </summary>
public class JobDto
{
    public int JobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string JobType { get; set; } = "Scheduled";
    public bool IsEnabled { get; set; } = true;

    // Folder configuration
    public string IncomingFolder { get; set; } = string.Empty;
    public string? ProcessedFolder { get; set; }
    public string? ErrorsFolder { get; set; }

    // Schedule
    public string? CronSchedule { get; set; }
    public string TimeZone { get; set; } = "UTC";

    // Database connection
    public string ServerInstance { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "RVToolsDW";
    public bool UseWindowsAuth { get; set; } = true;
    public string? EncryptedCredential { get; set; }

    // vCenter mapping
    public string? VIServer { get; set; }

    // Metadata
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

/// <summary>
/// DTO for creating a new job (subset of JobDto without auto-generated fields).
/// </summary>
public class CreateJobDto
{
    public string JobName { get; set; } = string.Empty;
    public string JobType { get; set; } = "Scheduled";
    public bool IsEnabled { get; set; } = true;

    public string IncomingFolder { get; set; } = string.Empty;
    public string? ProcessedFolder { get; set; }
    public string? ErrorsFolder { get; set; }

    public string? CronSchedule { get; set; }
    public string TimeZone { get; set; } = "UTC";

    public string ServerInstance { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "RVToolsDW";
    public bool UseWindowsAuth { get; set; } = true;
    public string? SqlUsername { get; set; }
    public string? SqlPassword { get; set; }

    public string? VIServer { get; set; }
}

/// <summary>
/// DTO for updating an existing job.
/// </summary>
public class UpdateJobDto : CreateJobDto
{
    public int JobId { get; set; }
}
