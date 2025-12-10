namespace RVToolsWeb.Models.DTOs;

/// <summary>
/// Represents a dropdown option for filter controls.
/// Used by FilterRepository and FilterService for cascading dropdowns.
/// </summary>
public class FilterOptionDto
{
    /// <summary>
    /// The value to submit when this option is selected.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The display text shown to the user.
    /// </summary>
    public string Label { get; set; } = string.Empty;
}
