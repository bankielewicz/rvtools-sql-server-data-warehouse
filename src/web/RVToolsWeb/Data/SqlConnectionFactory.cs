using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using RVToolsWeb.Configuration;

namespace RVToolsWeb.Data;

/// <summary>
/// Factory for creating SQL Server database connections.
/// Registered as a singleton in DI to reuse connection string parsing.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Creates a new database connection. Caller is responsible for disposing.
    /// </summary>
    IDbConnection CreateConnection();
}

/// <summary>
/// Implementation of ISqlConnectionFactory using Microsoft.Data.SqlClient.
/// </summary>
public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IOptions<AppSettings> settings)
    {
        _connectionString = settings.Value.ConnectionStrings.RVToolsDW;

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'RVToolsDW' is not configured. " +
                "Please check appsettings.json ConnectionStrings section.");
        }
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
