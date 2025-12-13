using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RVToolsWeb.Models;
using RVToolsWeb.Services;

namespace RVToolsWeb.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TimeFilterController : ControllerBase
{
    private readonly IUserPreferencesService _preferencesService;

    public TimeFilterController(IUserPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentFilter()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var filter = await _preferencesService.GetTimeFilterAsync(userId.Value);
        return Ok(new { filter, label = TimeFilter.Options[filter] });
    }

    [HttpPost]
    public async Task<IActionResult> SetFilter([FromBody] SetFilterRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (!TimeFilter.IsValid(request.Filter))
            return BadRequest("Invalid filter value");

        await _preferencesService.SetTimeFilterAsync(userId.Value, request.Filter);
        return Ok(new { success = true, filter = request.Filter, label = TimeFilter.Options[request.Filter] });
    }

    [HttpGet("options")]
    public IActionResult GetOptions()
    {
        var options = TimeFilter.Options.Select(o => new { value = o.Key, label = o.Value });
        return Ok(options);
    }

    private int? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(claim?.Value, out var id) ? id : null;
    }

    public class SetFilterRequest
    {
        public string Filter { get; set; } = string.Empty;
    }
}
