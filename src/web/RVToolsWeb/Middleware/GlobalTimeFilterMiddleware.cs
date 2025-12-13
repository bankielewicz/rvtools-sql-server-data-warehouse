using System.Security.Claims;
using RVToolsWeb.Models;
using RVToolsWeb.Services;

namespace RVToolsWeb.Middleware;

/// <summary>
/// Middleware to load user's global time filter preference into HttpContext.Items
/// Available to all controllers/views via HttpContext.Items["GlobalTimeFilter"]
/// </summary>
public class GlobalTimeFilterMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalTimeFilterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserPreferencesService preferencesService)
    {
        // Default filter for unauthenticated users
        var timeFilter = TimeFilter.DefaultFilter;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim?.Value, out var userId))
            {
                timeFilter = await preferencesService.GetTimeFilterAsync(userId);
            }
        }

        // Make available to controllers and views
        context.Items["GlobalTimeFilter"] = timeFilter;
        context.Items["GlobalTimeFilterLabel"] = TimeFilter.Options.GetValueOrDefault(timeFilter, "Last 30 Days");

        await _next(context);
    }
}

public static class GlobalTimeFilterMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalTimeFilter(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalTimeFilterMiddleware>();
    }
}
