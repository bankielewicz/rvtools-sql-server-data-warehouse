using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Home;
using Dapper;

namespace RVToolsWeb.Services.Home;

/// <summary>
/// Service for retrieving Dashboard summary KPIs.
/// Aggregates data from multiple views to provide a single overview.
/// </summary>
public class DashboardService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DashboardService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var dashboard = new DashboardViewModel();

        using var connection = _connectionFactory.CreateConnection();

        // Get enterprise summary (aggregated across all vCenters)
        const string enterpriseSql = @"
            SELECT
                COALESCE(SUM(VMs_PoweredOn), 0) AS VMsPoweredOn,
                COALESCE(SUM(VMs_PoweredOff), 0) AS VMsPoweredOff,
                COALESCE(SUM(Templates), 0) AS Templates,
                COALESCE(SUM(Total_VMs), 0) AS TotalVMs,
                COALESCE(SUM(Total_vCPUs), 0) AS TotalvCPUs,
                COALESCE(SUM(Total_vMemory_MiB), 0) / 1024 AS TotalvMemoryGiB,
                COALESCE(SUM(Total_Provisioned_MiB), 0) / 1024 / 1024 AS TotalStorageTiB,
                COALESCE(SUM(Total_InUse_MiB), 0) / 1024 / 1024 AS UsedStorageTiB,
                COALESCE(SUM(Cluster_Count), 0) AS TotalClusters,
                COALESCE(SUM(Host_Count), 0) AS TotalHosts,
                COALESCE(SUM(Datacenter_Count), 0) AS TotalDatacenters,
                COUNT(DISTINCT VI_SDK_Server) AS TotalVCenters,
                MAX(Latest_Import_Date) AS LastImportDate,
                MAX(Latest_ImportBatchId) AS LastImportBatchId
            FROM [Reporting].[vw_MultiVCenter_Enterprise_Summary]";

        var enterpriseData = await connection.QuerySingleOrDefaultAsync<dynamic>(enterpriseSql);
        if (enterpriseData != null)
        {
            dashboard.VMsPoweredOn = (int)(enterpriseData.VMsPoweredOn ?? 0);
            dashboard.VMsPoweredOff = (int)(enterpriseData.VMsPoweredOff ?? 0);
            dashboard.Templates = (int)(enterpriseData.Templates ?? 0);
            dashboard.TotalVMs = (int)(enterpriseData.TotalVMs ?? 0);
            dashboard.TotalvCPUs = (long)(enterpriseData.TotalvCPUs ?? 0);
            dashboard.TotalvMemoryGiB = (long)(enterpriseData.TotalvMemoryGiB ?? 0);
            dashboard.TotalStorageTiB = (long)(enterpriseData.TotalStorageTiB ?? 0);
            dashboard.UsedStorageTiB = (long)(enterpriseData.UsedStorageTiB ?? 0);
            dashboard.TotalClusters = (int)(enterpriseData.TotalClusters ?? 0);
            dashboard.TotalHosts = (int)(enterpriseData.TotalHosts ?? 0);
            dashboard.TotalDatacenters = (int)(enterpriseData.TotalDatacenters ?? 0);
            dashboard.TotalVCenters = (int)(enterpriseData.TotalVCenters ?? 0);
            dashboard.LastImportDate = enterpriseData.LastImportDate;
            dashboard.LastImportBatchId = (int?)(enterpriseData.LastImportBatchId);
        }

        // Get datastore count
        const string datastoreSql = @"
            SELECT COUNT(DISTINCT DatastoreName) AS TotalDatastores
            FROM [Reporting].[vw_Datastore_Capacity]";

        var datastoreCount = await connection.QuerySingleOrDefaultAsync<int?>(datastoreSql);
        dashboard.TotalDatastores = datastoreCount ?? 0;

        // Get datastore capacity alerts
        const string capacitySql = @"
            SELECT
                SUM(CASE WHEN CapacityStatus = 'Critical' THEN 1 ELSE 0 END) AS CriticalDatastores,
                SUM(CASE WHEN CapacityStatus = 'Warning' THEN 1 ELSE 0 END) AS WarningDatastores
            FROM [Reporting].[vw_Datastore_Capacity]";

        var capacityData = await connection.QuerySingleOrDefaultAsync<dynamic>(capacitySql);
        if (capacityData != null)
        {
            dashboard.CriticalDatastores = (int)(capacityData.CriticalDatastores ?? 0);
            dashboard.WarningDatastores = (int)(capacityData.WarningDatastores ?? 0);
        }

        // Get health issue count
        const string healthSql = @"
            SELECT COUNT(*) AS HealthIssueCount
            FROM [Reporting].[vw_Health_Issues]";

        var healthCount = await connection.QuerySingleOrDefaultAsync<int?>(healthSql);
        dashboard.HealthIssueCount = healthCount ?? 0;

        // Get expiring certificates (expiring within 30 days)
        const string certSql = @"
            SELECT COUNT(*) AS ExpiringCertificates
            FROM [Reporting].[vw_Health_Certificate_Expiration]
            WHERE Days_Until_Expiration <= 30 AND Days_Until_Expiration > 0";

        var certCount = await connection.QuerySingleOrDefaultAsync<int?>(certSql);
        dashboard.ExpiringCertificates = certCount ?? 0;

        // Get aging snapshots (older than 7 days)
        const string snapshotSql = @"
            SELECT COUNT(*) AS AgingSnapshots
            FROM [Reporting].[vw_Snapshot_Aging]
            WHERE AgeDays >= 7";

        var snapshotCount = await connection.QuerySingleOrDefaultAsync<int?>(snapshotSql);
        dashboard.AgingSnapshots = snapshotCount ?? 0;

        // Get orphaned files count
        const string orphanedSql = @"
            SELECT COUNT(*) AS OrphanedFiles
            FROM [Reporting].[vw_Health_Orphaned_Files]
            WHERE IsOrphaned = 1";

        var orphanedCount = await connection.QuerySingleOrDefaultAsync<int?>(orphanedSql);
        dashboard.OrphanedFiles = orphanedCount ?? 0;

        return dashboard;
    }
}
