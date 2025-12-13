using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Trends;

public class ChangeSummaryViewModel
{
    public ChangeSummaryFilter Filter { get; set; } = new();
    public IEnumerable<ChangeSummaryItem> Items { get; set; } = [];
    public int CreatedCount { get; set; }
    public int DeletedCount { get; set; }
    public int NetChange => CreatedCount - DeletedCount;
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = [];
}

public class ChangeSummaryFilter : BaseFilterViewModel
{
    public string? TimeFilter { get; set; }
    public string? ChangeType { get; set; }  // "Created", "Deleted", or null for both
}

public class ChangeSummaryItem
{
    public string VM { get; set; } = string.Empty;
    public string? VM_UUID { get; set; }
    public string? Powerstate { get; set; }
    public string? Host { get; set; }
    public string? Cluster { get; set; }
    public string? Datacenter { get; set; }
    public string VI_SDK_Server { get; set; } = string.Empty;
    public DateTime ChangeDate { get; set; }
    public string ChangeType { get; set; } = string.Empty;  // "Created" or "Deleted"
    public int? CPUs { get; set; }
    public long? Memory { get; set; }
    public long? Provisioned_MiB { get; set; }
}

public class DashboardChangeWidget
{
    public int VMsCreated { get; set; }
    public int VMsDeleted { get; set; }
    public int NetChange { get; set; }
    public int VCentersAffected { get; set; }
    public double GrowthPercent { get; set; }
    public List<WeeklyChangeData> WeeklyData { get; set; } = [];
}

public class WeeklyChangeData
{
    public int WeekNumber { get; set; }
    public DateTime WeekStart { get; set; }
    public int Created { get; set; }
    public int Deleted { get; set; }
}
