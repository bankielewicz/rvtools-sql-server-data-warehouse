using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Options;
using RVToolsWeb.Configuration;
using RVToolsWeb.Data;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Services.Logging;

/// <summary>
/// Implementation of web logging service with database and console sinks.
/// </summary>
public class WebLoggingService : IWebLoggingService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly WebLoggingConfig _config;
    private readonly ILogger<WebLoggingService> _logger;
    private readonly string _machineName;
    private readonly WebLogLevel _minimumLevel;
    private readonly WebLogLevel _databaseMinimumLevel;
    private readonly WebLogLevel _consoleMinimumLevel;

    public WebLoggingService(
        ISqlConnectionFactory connectionFactory,
        IOptions<AppSettings> settings,
        ILogger<WebLoggingService> logger)
    {
        _connectionFactory = connectionFactory;
        _config = settings.Value.WebLogging;
        _logger = logger;
        _machineName = Environment.MachineName;

        _minimumLevel = ParseLogLevel(_config.MinimumLevel);
        _databaseMinimumLevel = ParseLogLevel(_config.DatabaseLogging.MinimumLevel);
        _consoleMinimumLevel = ParseLogLevel(_config.ConsoleLogging.MinimumLevel);
    }

    public bool IsEnabled(WebLogLevel level)
    {
        return _config.Enabled && level >= _minimumLevel;
    }

    public async Task LogExceptionAsync(Exception exception, HttpContext? context = null, int? durationMs = null)
    {
        if (!IsEnabled(WebLogLevel.Error)) return;

        var entry = CreateLogEntry(WebLogLevel.Error, exception.Message, context);
        entry.DurationMs = durationMs;
        PopulateExceptionDetails(entry, exception);

        await WriteLogEntryAsync(entry);
    }

    public async Task LogAsync(WebLogLevel level, string message, HttpContext? context = null, object? contextData = null)
    {
        if (!IsEnabled(level)) return;

        var entry = CreateLogEntry(level, message, context);
        if (contextData != null)
        {
            entry.ContextData = JsonSerializer.Serialize(contextData);
        }

        await WriteLogEntryAsync(entry);
    }

    public Task LogVerboseAsync(string message, HttpContext? context = null)
        => LogAsync(WebLogLevel.Verbose, message, context);

    public Task LogInfoAsync(string message, HttpContext? context = null)
        => LogAsync(WebLogLevel.Info, message, context);

    public Task LogWarningAsync(string message, HttpContext? context = null)
        => LogAsync(WebLogLevel.Warning, message, context);

    public Task LogErrorAsync(string message, HttpContext? context = null)
        => LogAsync(WebLogLevel.Error, message, context);

    private WebLogEntry CreateLogEntry(WebLogLevel level, string? message, HttpContext? context)
    {
        var entry = new WebLogEntry
        {
            LogLevel = level,
            Message = message,
            MachineName = _machineName
        };

        if (context != null)
        {
            entry.RequestId = context.TraceIdentifier;
            entry.RequestPath = context.Request.Path.Value;
            entry.RequestMethod = context.Request.Method;
            entry.QueryString = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value
                : null;
            entry.UserName = context.User.Identity?.Name;
            entry.ClientIP = context.Connection.RemoteIpAddress?.ToString();
            entry.UserAgent = context.Request.Headers.UserAgent.FirstOrDefault();

            if (_config.IncludeRequestHeaders)
            {
                entry.RequestHeaders = SerializeHeaders(context.Request.Headers);
            }
        }

        return entry;
    }

    private void PopulateExceptionDetails(WebLogEntry entry, Exception exception)
    {
        entry.ExceptionType = exception.GetType().FullName;
        entry.ExceptionMessage = exception.Message;
        entry.StackTrace = TruncateStackTrace(exception.StackTrace);

        if (exception.InnerException != null)
        {
            entry.InnerException = SerializeInnerExceptions(exception.InnerException);
        }
    }

    private string? TruncateStackTrace(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace)) return null;
        return stackTrace.Length > _config.MaxStackTraceLength
            ? stackTrace.Substring(0, _config.MaxStackTraceLength) + "...[truncated]"
            : stackTrace;
    }

    private string SerializeInnerExceptions(Exception inner)
    {
        var exceptions = new List<object>();
        var current = inner;
        var depth = 0;

        while (current != null && depth < 5) // Limit depth to prevent infinite loops
        {
            exceptions.Add(new
            {
                Type = current.GetType().FullName,
                Message = current.Message,
                StackTrace = TruncateStackTrace(current.StackTrace)
            });
            current = current.InnerException;
            depth++;
        }

        return JsonSerializer.Serialize(exceptions);
    }

    private string SerializeHeaders(IHeaderDictionary headers)
    {
        var filtered = headers
            .Where(h => !_config.ExcludedHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        return JsonSerializer.Serialize(filtered);
    }

    private async Task WriteLogEntryAsync(WebLogEntry entry)
    {
        // Console logging
        if (_config.ConsoleLogging.Enabled && entry.LogLevel >= _consoleMinimumLevel)
        {
            LogToConsole(entry);
        }

        // Database logging
        if (_config.DatabaseLogging.Enabled && entry.LogLevel >= _databaseMinimumLevel)
        {
            await WriteToDatabaseAsync(entry);
        }
    }

    private void LogToConsole(WebLogEntry entry)
    {
        var logLevel = entry.LogLevel switch
        {
            WebLogLevel.Verbose => LogLevel.Debug,
            WebLogLevel.Info => LogLevel.Information,
            WebLogLevel.Warning => LogLevel.Warning,
            WebLogLevel.Error => LogLevel.Error,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel,
            "[{RequestId}] {Level}: {Message} | Path: {Path} | User: {User}",
            entry.RequestId ?? "N/A",
            entry.LogLevel,
            entry.Message ?? entry.ExceptionMessage,
            entry.RequestPath ?? "N/A",
            entry.UserName ?? "Anonymous");
    }

    private async Task WriteToDatabaseAsync(WebLogEntry entry)
    {
        const string sql = @"
            INSERT INTO Web.ErrorLog (
                LogTime, LogLevel, Message, ExceptionType, ExceptionMessage,
                StackTrace, InnerException, RequestId, RequestPath, RequestMethod,
                QueryString, RequestHeaders, UserName, ClientIP, UserAgent,
                DurationMs, MachineName, ContextData
            ) VALUES (
                @LogTime, @LogLevel, @Message, @ExceptionType, @ExceptionMessage,
                @StackTrace, @InnerException, @RequestId, @RequestPath, @RequestMethod,
                @QueryString, @RequestHeaders, @UserName, @ClientIP, @UserAgent,
                @DurationMs, @MachineName, @ContextData
            )";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new
            {
                entry.LogTime,
                LogLevel = entry.LogLevel.ToString(),
                entry.Message,
                entry.ExceptionType,
                entry.ExceptionMessage,
                entry.StackTrace,
                entry.InnerException,
                entry.RequestId,
                entry.RequestPath,
                entry.RequestMethod,
                entry.QueryString,
                entry.RequestHeaders,
                entry.UserName,
                entry.ClientIP,
                entry.UserAgent,
                entry.DurationMs,
                entry.MachineName,
                entry.ContextData
            });
        }
        catch (Exception ex)
        {
            // Fall back to console if database write fails - don't throw to avoid infinite loops
            _logger.LogError(ex, "Failed to write log entry to database");
        }
    }

    private static WebLogLevel ParseLogLevel(string level)
    {
        return level?.ToLowerInvariant() switch
        {
            "off" => WebLogLevel.Off,
            "verbose" => WebLogLevel.Verbose,
            "info" or "information" => WebLogLevel.Info,
            "warning" or "warn" => WebLogLevel.Warning,
            "error" => WebLogLevel.Error,
            _ => WebLogLevel.Warning
        };
    }
}
