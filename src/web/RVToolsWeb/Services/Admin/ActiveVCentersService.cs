using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

public interface IActiveVCentersService
{
    Task<IEnumerable<VCenterStatusItem>> GetAllVCentersAsync();
    Task SetVCenterActiveAsync(string viServer, bool isActive);
    Task UpdateNotesAsync(string viServer, string? notes);
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

    public async Task SetVCenterActiveAsync(string viServer, bool isActive)
    {
        // Use MERGE to handle vCenters that exist in import history but not yet in Config table
        const string sql = @"
            MERGE [Config].[ActiveVCenters] AS target
            USING (SELECT @VIServer AS VIServer) AS source
            ON target.VIServer = source.VIServer
            WHEN MATCHED THEN
                UPDATE SET IsActive = @IsActive, ModifiedDate = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (VIServer, IsActive, Notes, CreatedDate, ModifiedDate)
                VALUES (@VIServer, @IsActive, NULL, SYSUTCDATETIME(), SYSUTCDATETIME());";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { VIServer = viServer, IsActive = isActive });
    }

    public async Task UpdateNotesAsync(string viServer, string? notes)
    {
        // Use MERGE to handle vCenters that exist in import history but not yet in Config table
        const string sql = @"
            MERGE [Config].[ActiveVCenters] AS target
            USING (SELECT @VIServer AS VIServer) AS source
            ON target.VIServer = source.VIServer
            WHEN MATCHED THEN
                UPDATE SET Notes = @Notes, ModifiedDate = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (VIServer, IsActive, Notes, CreatedDate, ModifiedDate)
                VALUES (@VIServer, 1, @Notes, SYSUTCDATETIME(), SYSUTCDATETIME());";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { VIServer = viServer, Notes = notes });
    }
}
