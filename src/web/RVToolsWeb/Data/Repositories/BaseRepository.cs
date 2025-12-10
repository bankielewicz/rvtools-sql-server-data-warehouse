using System.Data;
using Dapper;

namespace RVToolsWeb.Data.Repositories;

/// <summary>
/// Base class for Dapper repositories providing common query patterns.
/// </summary>
public abstract class BaseRepository
{
    protected readonly ISqlConnectionFactory ConnectionFactory;

    protected BaseRepository(ISqlConnectionFactory connectionFactory)
    {
        ConnectionFactory = connectionFactory;
    }

    /// <summary>
    /// Executes a query and returns a collection of results.
    /// </summary>
    protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Executes a query and returns the first result or default.
    /// </summary>
    protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Executes a query and returns a single result. Throws if not exactly one.
    /// </summary>
    protected async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Executes a command (INSERT, UPDATE, DELETE) and returns affected row count.
    /// </summary>
    protected async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }

    /// <summary>
    /// Executes a query and returns a scalar value.
    /// </summary>
    protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<T>(sql, parameters);
    }
}
