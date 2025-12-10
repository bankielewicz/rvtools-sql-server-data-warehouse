namespace RVToolsWeb.Models.ViewModels.Shared;

/// <summary>
/// View model for Chart.js chart data.
/// </summary>
public class ChartDataViewModel
{
    /// <summary>
    /// Labels for the X-axis (e.g., dates).
    /// </summary>
    public List<string> Labels { get; set; } = new();

    /// <summary>
    /// Dataset series for the chart.
    /// </summary>
    public List<ChartDataset> Datasets { get; set; } = new();
}

/// <summary>
/// A single dataset (line/area) for a Chart.js chart.
/// </summary>
public class ChartDataset
{
    /// <summary>
    /// Label for this dataset (shown in legend).
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Data points for this dataset.
    /// </summary>
    public List<decimal?> Data { get; set; } = new();

    /// <summary>
    /// Border color (CSS color string).
    /// </summary>
    public string BorderColor { get; set; } = "#0d6efd";

    /// <summary>
    /// Background color (CSS color string, used for area charts).
    /// </summary>
    public string BackgroundColor { get; set; } = "rgba(13, 110, 253, 0.1)";

    /// <summary>
    /// Whether to fill under the line (for area charts).
    /// </summary>
    public bool Fill { get; set; } = false;

    /// <summary>
    /// Line tension (0 = straight lines, 0.4 = smooth curves).
    /// </summary>
    public decimal Tension { get; set; } = 0.3m;

    /// <summary>
    /// Point radius (size of data point markers).
    /// </summary>
    public int PointRadius { get; set; } = 3;

    /// <summary>
    /// Border width (line thickness).
    /// </summary>
    public int BorderWidth { get; set; } = 2;
}

/// <summary>
/// Chart configuration options.
/// </summary>
public class ChartOptions
{
    /// <summary>
    /// Chart type: "line", "bar", "area" (line with fill).
    /// </summary>
    public string ChartType { get; set; } = "line";

    /// <summary>
    /// Chart title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Y-axis label.
    /// </summary>
    public string YAxisLabel { get; set; } = string.Empty;

    /// <summary>
    /// X-axis label.
    /// </summary>
    public string XAxisLabel { get; set; } = "Date";

    /// <summary>
    /// Whether to begin Y-axis at zero.
    /// </summary>
    public bool BeginAtZero { get; set; } = true;

    /// <summary>
    /// Whether to show the legend.
    /// </summary>
    public bool ShowLegend { get; set; } = true;

    /// <summary>
    /// Chart height in pixels.
    /// </summary>
    public int Height { get; set; } = 300;
}
