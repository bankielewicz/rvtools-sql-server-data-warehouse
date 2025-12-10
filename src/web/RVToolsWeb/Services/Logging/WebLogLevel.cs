namespace RVToolsWeb.Services.Logging;

/// <summary>
/// Log levels for web application logging.
/// Ordered by severity (lowest to highest).
/// </summary>
public enum WebLogLevel
{
    /// <summary>Logging disabled</summary>
    Off = 0,

    /// <summary>Detailed diagnostic information</summary>
    Verbose = 1,

    /// <summary>General informational messages</summary>
    Info = 2,

    /// <summary>Potential issues that should be reviewed</summary>
    Warning = 3,

    /// <summary>Errors and exceptions</summary>
    Error = 4
}
