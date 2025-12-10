using RVToolsWeb.Services.Logging;

namespace RVToolsWeb.Services.Interfaces;

/// <summary>
/// Service interface for web application logging to database and console.
/// </summary>
public interface IWebLoggingService
{
    /// <summary>
    /// Logs an exception with full request context.
    /// </summary>
    Task LogExceptionAsync(Exception exception, HttpContext? context = null, int? durationMs = null);

    /// <summary>
    /// Logs a message at the specified level.
    /// </summary>
    Task LogAsync(WebLogLevel level, string message, HttpContext? context = null, object? contextData = null);

    /// <summary>
    /// Logs a verbose message (detailed diagnostics).
    /// </summary>
    Task LogVerboseAsync(string message, HttpContext? context = null);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    Task LogInfoAsync(string message, HttpContext? context = null);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    Task LogWarningAsync(string message, HttpContext? context = null);

    /// <summary>
    /// Logs an error message without exception.
    /// </summary>
    Task LogErrorAsync(string message, HttpContext? context = null);

    /// <summary>
    /// Checks if logging is enabled for the specified level.
    /// </summary>
    bool IsEnabled(WebLogLevel level);
}
