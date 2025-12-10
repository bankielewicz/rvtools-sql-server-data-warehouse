using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Health;

/// <summary>
/// View model for the Health Issues report.
/// </summary>
public class HealthIssuesViewModel
{
    public HealthIssuesFilter Filter { get; set; } = new();
    public IEnumerable<HealthIssuesItem> Items { get; set; } = Enumerable.Empty<HealthIssuesItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the Health Issues report.
/// </summary>
public class HealthIssuesFilter
{
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single health issue record from the vw_Health_Issues view.
/// </summary>
public class HealthIssuesItem
{
    public string? ObjectName { get; set; }
    public string? Message { get; set; }
    public string? IssueType { get; set; }
    public string? VI_SDK_Server { get; set; }
    public int? ImportBatchId { get; set; }
    public DateTime? DetectedDate { get; set; }
}
