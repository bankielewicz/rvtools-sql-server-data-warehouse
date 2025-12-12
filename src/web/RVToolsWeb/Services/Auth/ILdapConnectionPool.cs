namespace RVToolsWeb.Services.Auth;

using System.DirectoryServices.Protocols;

/// <summary>
/// Interface for LDAP connection pooling.
/// Provides reusable connections authenticated with service account for user lookups.
/// </summary>
public interface ILdapConnectionPool
{
    /// <summary>
    /// Gets a pooled LDAP connection authenticated with the service account.
    /// Connection should be returned via ReturnConnection when done.
    /// </summary>
    /// <returns>A pooled LDAP connection, or null if pool is unavailable</returns>
    Task<LdapConnection?> GetConnectionAsync();

    /// <summary>
    /// Returns a connection to the pool for reuse.
    /// </summary>
    /// <param name="connection">The connection to return</param>
    void ReturnConnection(LdapConnection connection);

    /// <summary>
    /// Invalidates all pooled connections (call when LDAP settings change).
    /// </summary>
    void InvalidatePool();

    /// <summary>
    /// Gets whether the pool is available (service account configured).
    /// </summary>
    bool IsAvailable { get; }
}
