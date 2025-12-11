using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service for managing Config.Settings database table.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ISqlConnectionFactory connectionFactory, ILogger<SettingsService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<GeneralSettingViewModel>> GetAllSettingsAsync()
    {
        const string sql = @"
            SELECT
                SettingId,
                SettingName,
                SettingValue,
                Description,
                DataType,
                CreatedDate,
                ModifiedDate
            FROM [Config].[Settings]
            ORDER BY SettingName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<GeneralSettingViewModel>(sql);
    }

    public async Task<GeneralSettingViewModel?> GetSettingByNameAsync(string settingName)
    {
        const string sql = @"
            SELECT
                SettingId,
                SettingName,
                SettingValue,
                Description,
                DataType,
                CreatedDate,
                ModifiedDate
            FROM [Config].[Settings]
            WHERE SettingName = @SettingName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<GeneralSettingViewModel>(sql, new { SettingName = settingName });
    }

    public async Task<bool> UpdateSettingAsync(string settingName, string settingValue)
    {
        const string sql = @"
            UPDATE [Config].[Settings]
            SET SettingValue = @SettingValue,
                ModifiedDate = GETUTCDATE()
            WHERE SettingName = @SettingName";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { SettingName = settingName, SettingValue = settingValue });

            if (rowsAffected > 0)
            {
                _logger.LogInformation("Updated setting {SettingName} to {SettingValue}", settingName, settingValue);
            }

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update setting {SettingName}", settingName);
            return false;
        }
    }
}
