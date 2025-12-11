namespace RVToolsWeb.Middleware;

using RVToolsWeb.Services.Auth;

/// <summary>
/// Middleware that redirects to first-time setup if authentication is not configured
/// </summary>
public class FirstTimeSetupMiddleware
{
    private readonly RequestDelegate _next;
    private static bool? _isSetupRequired;
    private static readonly object _lock = new();

    public FirstTimeSetupMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Skip for static files and setup-related endpoints
        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        // Check setup status (cached for performance)
        if (_isSetupRequired == null)
        {
            lock (_lock)
            {
                _isSetupRequired ??= authService.IsSetupRequiredAsync().GetAwaiter().GetResult();
            }
        }

        if (_isSetupRequired == true)
        {
            context.Response.Redirect("/Account/Setup");
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Call this after setup completes to reset the cached status
    /// </summary>
    public static void ResetSetupCache()
    {
        lock (_lock)
        {
            _isSetupRequired = null;
        }
    }

    private static bool IsExcludedPath(string path)
    {
        // Static files
        if (path.StartsWith("/lib/") ||
            path.StartsWith("/css/") ||
            path.StartsWith("/js/") ||
            path.StartsWith("/images/") ||
            path.StartsWith("/favicon"))
            return true;

        // Setup endpoints
        if (path.StartsWith("/account/setup") ||
            path == "/account/completesetup")
            return true;

        return false;
    }
}

/// <summary>
/// Extension method for middleware registration
/// </summary>
public static class FirstTimeSetupMiddlewareExtensions
{
    public static IApplicationBuilder UseFirstTimeSetup(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FirstTimeSetupMiddleware>();
    }
}
