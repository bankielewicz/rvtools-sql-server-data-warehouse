using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace RVToolsShared.Data;

/// <summary>
/// Factory for creating SQL Server connections.
/// Supports both Windows Authentication and SQL Authentication.
/// </summary>
public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("RVToolsDW")
            ?? throw new InvalidOperationException("Connection string 'RVToolsDW' not found in configuration.");
    }

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <inheritdoc/>
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    /// <inheritdoc/>
    public SqlConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    /// <summary>
    /// Builds a connection string from individual components.
    /// </summary>
    public static string BuildConnectionString(
        string serverInstance,
        string databaseName,
        bool useWindowsAuth,
        string? username = null,
        string? password = null,
        bool trustServerCertificate = true)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = serverInstance,
            InitialCatalog = databaseName,
            TrustServerCertificate = trustServerCertificate,
            ApplicationName = "RVToolsImportService"
        };

        if (useWindowsAuth)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Username and password required for SQL Authentication.");

            builder.IntegratedSecurity = false;
            builder.UserID = username;
            builder.Password = password;
        }

        return builder.ConnectionString;
    }
}
