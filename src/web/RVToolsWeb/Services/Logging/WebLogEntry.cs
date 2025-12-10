namespace RVToolsWeb.Services.Logging;

/// <summary>
/// Represents a log entry to be written to Web.ErrorLog.
/// </summary>
public class WebLogEntry
{
    public DateTime LogTime { get; set; } = DateTime.UtcNow;
    public WebLogLevel LogLevel { get; set; }
    public string? Message { get; set; }
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public string? RequestId { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? QueryString { get; set; }
    public string? RequestHeaders { get; set; }
    public string? UserName { get; set; }
    public string? ClientIP { get; set; }
    public string? UserAgent { get; set; }
    public int? DurationMs { get; set; }
    public string? MachineName { get; set; }
    public string? ContextData { get; set; }
}
