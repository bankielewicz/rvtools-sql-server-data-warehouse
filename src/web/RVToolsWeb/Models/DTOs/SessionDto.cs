namespace RVToolsWeb.Models.DTOs;

/// <summary>
/// Data transfer object for Web.Sessions table
/// Tracks authentication events for both LocalDB and LDAP users
/// </summary>
public class SessionDto
{
    public long SessionId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AuthSource { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionToken { get; set; }
}
