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

        // Skip for static files only (not setup endpoints)
        if (IsStaticFilePath(path))
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

        // If setup IS required, allow only setup endpoints
        if (_isSetupRequired == true)
        {
            if (IsSetupPath(path))
            {
                await _next(context);
                return;
            }
            context.Response.Redirect("/Account/Setup");
            return;
        }

        // If setup is NOT required (already complete), BLOCK setup endpoints
        // This prevents unauthorized re-initialization
        if (IsSetupPath(path))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Setup has already been completed. Access denied.");
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

    private static bool IsStaticFilePath(string path)
    {
        return path.StartsWith("/lib/") ||
               path.StartsWith("/css/") ||
               path.StartsWith("/js/") ||
               path.StartsWith("/images/") ||
               path.StartsWith("/favicon");
    }

    private static bool IsSetupPath(string path)
    {
        // Only block the setup wizard and completion POST endpoint
        // Do NOT block /setupcomplete - it's the confirmation page shown after setup
        return path == "/account/setup" ||
               path == "/account/completesetup";
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
