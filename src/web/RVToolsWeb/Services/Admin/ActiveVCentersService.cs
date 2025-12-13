using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

public interface IActiveVCentersService
{
    Task<IEnumerable<VCenterStatusItem>> GetAllVCentersAsync();
    Task<IEnumerable<string>> GetActiveVCenterNamesAsync();
    Task SetVCenterActiveAsync(string viServer, bool isActive);
    Task UpdateNotesAsync(string viServer, string? notes);
    Task SyncFromImportsAsync();
}

public class ActiveVCentersService : IActiveVCentersService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ActiveVCentersService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<VCenterStatusItem>> GetAllVCentersAsync()
    {
        const string sql = "SELECT * FROM [Reporting].[vw_VCenter_Status] ORDER BY LastImportDate DESC";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<VCenterStatusItem>(sql);
    }

    public async Task<IEnumerable<string>> GetActiveVCenterNamesAsync()
    {
        const string sql = @"
            SELECT VIServer FROM [Config].[ActiveVCenters] WHERE IsActive = 1
            UNION
            SELECT DISTINCT VIServer FROM [Audit].[ImportBatch]
            WHERE VIServer NOT IN (SELECT VIServer FROM [Config].[ActiveVCenters])";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql);
    }

    public async Task SetVCenterActiveAsync(string viServer, bool isActive)
    {
        const string sql = @"
            UPDATE [Config].[ActiveVCenters]
            SET IsActive = @IsActive, ModifiedDate = SYSUTCDATETIME()
            WHERE VIServer = @VIServer";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { VIServer = viServer, IsActive = isActive });
    }

    public async Task UpdateNotesAsync(string viServer, string? notes)
    {
        const string sql = @"
            UPDATE [Config].[ActiveVCenters]
            SET Notes = @Notes, ModifiedDate = SYSUTCDATETIME()
            WHERE VIServer = @VIServer";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { VIServer = viServer, Notes = notes });
    }

    public async Task SyncFromImportsAsync()
    {
        const string sql = "EXEC [dbo].[usp_SyncActiveVCenters]";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql);
    }
}
