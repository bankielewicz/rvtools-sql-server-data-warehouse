using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Middleware;

/// <summary>
/// Global exception handling middleware that logs exceptions and provides user-friendly error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IWebLoggingService loggingService)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log to our custom logging service
            await loggingService.LogExceptionAsync(ex, context, (int)stopwatch.ElapsedMilliseconds);

            // Also log to built-in logger for development console
            _logger.LogError(ex, "Unhandled exception for request {Path}", context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // For API requests or AJAX calls, return JSON error
        var isAjaxRequest = context.Request.Path.StartsWithSegments("/api")
            || context.Request.Headers.XRequestedWith == "XMLHttpRequest"
            || context.Request.Headers.Accept.Any(h => h?.Contains("application/json") == true)
            || context.Request.ContentType?.Contains("application/json") == true;

        if (isAjaxRequest)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = GetUserFriendlyMessage(exception),
                requestId = context.TraceIdentifier
            });
            return;
        }

        // For MVC requests, store error info in TempData for toast display
        try
        {
            var tempDataFactory = context.RequestServices.GetService<ITempDataDictionaryFactory>();
            if (tempDataFactory != null)
            {
                var tempData = tempDataFactory.GetTempData(context);
                tempData["ErrorMessage"] = GetUserFriendlyMessage(exception);
                tempData["ErrorRequestId"] = context.TraceIdentifier;
                tempData["ShowErrorToast"] = true;
            }
        }
        catch
        {
            // If TempData fails, continue with redirect anyway
        }

        // Redirect to the referring page or home
        var referer = context.Request.Headers.Referer.FirstOrDefault();
        if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            // Only redirect to same host to prevent open redirect
            if (refererUri.Host.Equals(context.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect(referer);
                return;
            }
        }

        context.Response.Redirect("/");
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        // Map specific exceptions to user-friendly messages
        return exception switch
        {
            Microsoft.Data.SqlClient.SqlException sqlEx when sqlEx.Number == -2
                => "The database connection timed out. Please try again.",
            Microsoft.Data.SqlClient.SqlException
                => "A database error occurred. Please try again later.",
            HttpRequestException
                => "A network error occurred. Please check your connection.",
            TimeoutException
                => "The request timed out. Please try again.",
            UnauthorizedAccessException
                => "You do not have permission to perform this action.",
            InvalidOperationException
                => "The requested operation could not be completed.",
            ArgumentException
                => "Invalid request parameters. Please check your input.",
            _ => "An unexpected error occurred. Please try again or contact support."
        };
    }
}

/// <summary>
/// Extension methods for registering the global exception middleware.
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
