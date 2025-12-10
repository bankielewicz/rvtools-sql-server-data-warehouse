using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Inventory;

/// <summary>
/// View model for the Cluster Summary report.
/// </summary>
public class ClusterSummaryViewModel
{
    public ClusterSummaryFilter Filter { get; set; } = new();
    public IEnumerable<ClusterSummaryItem> Items { get; set; } = Enumerable.Empty<ClusterSummaryItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the Cluster Summary report.
/// </summary>
public class ClusterSummaryFilter
{
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single cluster record from the vw_Cluster_Summary view.
/// </summary>
public class ClusterSummaryItem
{
    // Identity
    public string ClusterName { get; set; } = string.Empty;
    public string? VI_SDK_Server { get; set; }

    // Status
    public string? Config_status { get; set; }
    public string? OverallStatus { get; set; }

    // Hosts
    public int? NumHosts { get; set; }
    public int? numEffectiveHosts { get; set; }

    // CPU Resources
    public long? TotalCpu { get; set; }
    public int? NumCpuCores { get; set; }
    public int? NumCpuThreads { get; set; }
    public long? Effective_Cpu { get; set; }

    // Memory Resources
    public long? TotalMemory { get; set; }
    public long? Effective_Memory { get; set; }

    // HA Configuration
    public bool? HA_enabled { get; set; }
    public int? Failover_Level { get; set; }
    public bool? AdmissionControlEnabled { get; set; }
    public string? Host_monitoring { get; set; }
    public string? Isolation_Response { get; set; }
    public string? Restart_Priority { get; set; }

    // VM Monitoring
    public string? VM_Monitoring { get; set; }
    public int? Max_Failures { get; set; }
    public int? Max_Failure_Window { get; set; }
    public int? Failure_Interval { get; set; }
    public int? Min_Up_Time { get; set; }

    // DRS Configuration
    public bool? DRS_enabled { get; set; }
    public string? DRS_default_VM_behavior { get; set; }
    public int? DRS_vmotion_rate { get; set; }

    // DPM Configuration
    public bool? DPM_enabled { get; set; }
    public string? DPM_default_behavior { get; set; }

    // Activity
    public int? Num_VMotions { get; set; }
}
