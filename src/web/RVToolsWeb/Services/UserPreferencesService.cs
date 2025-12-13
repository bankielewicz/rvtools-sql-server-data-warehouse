using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models;

namespace RVToolsWeb.Services;

public interface IUserPreferencesService
{
    Task<string> GetTimeFilterAsync(int userId);
    Task SetTimeFilterAsync(int userId, string filter);
    Task<Dictionary<string, string>> GetAllPreferencesAsync(int userId);
    Task SetPreferenceAsync(int userId, string key, string value);
}

public class UserPreferencesService : IUserPreferencesService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private const string TimeFilterKey = "GlobalTimeFilter";

    public UserPreferencesService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<string> GetTimeFilterAsync(int userId)
    {
        const string sql = @"
            SELECT PreferenceValue
            FROM Web.UserPreferences
            WHERE UserId = @UserId AND PreferenceKey = @Key";

        using var connection = _connectionFactory.CreateConnection();
        var value = await connection.QueryFirstOrDefaultAsync<string>(sql,
            new { UserId = userId, Key = TimeFilterKey });

        return TimeFilter.IsValid(value) ? value! : TimeFilter.DefaultFilter;
    }

    public async Task SetTimeFilterAsync(int userId, string filter)
    {
        if (!TimeFilter.IsValid(filter))
            filter = TimeFilter.DefaultFilter;

        await SetPreferenceAsync(userId, TimeFilterKey, filter);
    }

    public async Task<Dictionary<string, string>> GetAllPreferencesAsync(int userId)
    {
        const string sql = @"
            SELECT PreferenceKey, PreferenceValue
            FROM Web.UserPreferences
            WHERE UserId = @UserId";

        using var connection = _connectionFactory.CreateConnection();
        var prefs = await connection.QueryAsync<(string Key, string Value)>(sql, new { UserId = userId });
        return prefs.ToDictionary(p => p.Key, p => p.Value);
    }

    public async Task SetPreferenceAsync(int userId, string key, string value)
    {
        const string sql = @"
            MERGE Web.UserPreferences AS target
            USING (SELECT @UserId AS UserId, @Key AS PreferenceKey) AS source
            ON target.UserId = source.UserId AND target.PreferenceKey = source.PreferenceKey
            WHEN MATCHED THEN
                UPDATE SET PreferenceValue = @Value, ModifiedDate = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (UserId, PreferenceKey, PreferenceValue)
                VALUES (@UserId, @Key, @Value);";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId, Key = key, Value = value });
    }
}
