using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Health;

/// <summary>
/// View model for the Orphaned Files report.
/// </summary>
public class OrphanedFilesViewModel
{
    public OrphanedFilesFilter Filter { get; set; } = new();
    public IEnumerable<OrphanedFilesItem> Items { get; set; } = Enumerable.Empty<OrphanedFilesItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public decimal TotalOrphanedSizeGiB => Items.Where(x => x.IsOrphaned).Sum(x => x.File_Size_GiB ?? 0);
    public int OrphanedCount => Items.Count(x => x.IsOrphaned);
}

/// <summary>
/// Filter parameters for the Orphaned Files report.
/// </summary>
public class OrphanedFilesFilter
{
    public string? VI_SDK_Server { get; set; }
    public bool ShowOrphanedOnly { get; set; } = true;
}

/// <summary>
/// Single file record from the vw_Health_Orphaned_Files view.
/// </summary>
public class OrphanedFilesItem
{
    public string? File_Name { get; set; }
    public string? Friendly_Path_Name { get; set; }
    public string? Path { get; set; }
    public string? File_Type { get; set; }
    public long? File_Size_Bytes { get; set; }
    public decimal? File_Size_GiB { get; set; }
    public string? Datastore { get; set; }
    public bool IsOrphaned { get; set; }
    public string? VI_SDK_Server { get; set; }
}
