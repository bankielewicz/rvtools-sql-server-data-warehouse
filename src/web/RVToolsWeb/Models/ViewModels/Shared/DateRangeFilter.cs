namespace RVToolsWeb.Models.ViewModels.Shared;

/// <summary>
/// Base class for filters that support date range selection.
/// Provides StartDate/EndDate properties with calculated effective dates.
/// </summary>
public abstract class DateRangeFilter
{
    /// <summary>
    /// Start date for the date range (inclusive). If null, uses default days back from today.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for the date range (inclusive). If null, defaults to today.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Default number of days to look back if no date range is specified.
    /// Override in derived classes to set different defaults.
    /// </summary>
    protected virtual int DefaultDaysBack => 30;

    /// <summary>
    /// Gets the effective start date for queries.
    /// Uses StartDate if set, otherwise calculates from DefaultDaysBack.
    /// </summary>
    public DateTime EffectiveStartDate =>
        StartDate ?? DateTime.UtcNow.Date.AddDays(-DefaultDaysBack);

    /// <summary>
    /// Gets the effective end date for queries.
    /// Uses EndDate if set, otherwise defaults to today.
    /// </summary>
    public DateTime EffectiveEndDate =>
        EndDate ?? DateTime.UtcNow.Date;
}
