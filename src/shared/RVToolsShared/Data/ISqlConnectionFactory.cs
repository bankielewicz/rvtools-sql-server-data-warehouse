using Microsoft.Data.SqlClient;

namespace RVToolsShared.Data;

/// <summary>
/// Factory interface for creating SQL Server connections.
/// Shared between RVToolsService and RVToolsWeb for consistent database access.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Creates a connection using the default connection string from configuration.
    /// </summary>
    SqlConnection CreateConnection();

    /// <summary>
    /// Creates a connection using a specific connection string.
    /// </summary>
    SqlConnection CreateConnection(string connectionString);
}
