namespace RVToolsWeb.Models.ViewModels.Admin;

public class VCenterStatusViewModel
{
    public IEnumerable<VCenterStatusItem> VCenters { get; set; } = [];
    public int ActiveCount => VCenters.Count(v => v.IsActive);
    public int InactiveCount => VCenters.Count(v => !v.IsActive);
    public int StaleCount => VCenters.Count(v => v.ImportStatus == "Stale" || v.ImportStatus == "Inactive");
}

public class VCenterStatusItem
{
    public string VI_SDK_Server { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastImportDate { get; set; }
    public int TotalImports { get; set; }
    public DateTime? FirstImportDate { get; set; }
    public int DaysSinceLastImport { get; set; }
    public string ImportStatus { get; set; } = string.Empty;  // "Current", "Recent", "Stale", "Inactive", "Never"
    public string? Notes { get; set; }
    public int VMCount { get; set; }
    public int HostCount { get; set; }
}
