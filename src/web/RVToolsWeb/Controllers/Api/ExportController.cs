using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Capacity;
using RVToolsWeb.Models.ViewModels.Health;
using RVToolsWeb.Models.ViewModels.Inventory;
using RVToolsWeb.Models.ViewModels.Trends;
using RVToolsWeb.Services.Capacity;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;
using RVToolsWeb.Services.Trends;

namespace RVToolsWeb.Controllers.Api;

/// <summary>
/// API controller for exporting report data to CSV and Excel formats.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;
    // Inventory Services
    private readonly VMInventoryService _vmInventoryService;
    private readonly HostInventoryService _hostInventoryService;
    private readonly ClusterSummaryService _clusterSummaryService;
    private readonly DatastoreInventoryService _datastoreInventoryService;
    private readonly EnterpriseSummaryService _enterpriseSummaryService;
    private readonly NetworkTopologyService _networkTopologyService;
    private readonly LicenseComplianceService _licenseComplianceService;
    private readonly ResourcePoolService _resourcePoolService;
    // Health Services
    private readonly HealthIssuesService _healthIssuesService;
    private readonly CertificateExpirationService _certificateExpirationService;
    private readonly SnapshotAgingService _snapshotAgingService;
    private readonly ConfigurationComplianceService _configurationComplianceService;
    private readonly OrphanedFilesService _orphanedFilesService;
    private readonly ToolsStatusService _toolsStatusService;
    // Capacity Services
    private readonly DatastoreCapacityService _datastoreCapacityService;
    private readonly HostCapacityService _hostCapacityService;
    private readonly VMResourceAllocationService _vmResourceAllocationService;
    private readonly VMRightSizingService _vmRightSizingService;
    // Trend Services
    private readonly VMCountTrendService _vmCountTrendService;
    private readonly StorageGrowthService _storageGrowthService;
    private readonly DatastoreCapacityTrendService _datastoreCapacityTrendService;
    private readonly HostUtilizationService _hostUtilizationService;
    private readonly VMConfigChangesService _vmConfigChangesService;
    private readonly VMLifecycleService _vmLifecycleService;

    public ExportController(
        IExportService exportService,
        VMInventoryService vmInventoryService,
        HostInventoryService hostInventoryService,
        ClusterSummaryService clusterSummaryService,
        DatastoreInventoryService datastoreInventoryService,
        EnterpriseSummaryService enterpriseSummaryService,
        NetworkTopologyService networkTopologyService,
        LicenseComplianceService licenseComplianceService,
        ResourcePoolService resourcePoolService,
        HealthIssuesService healthIssuesService,
        CertificateExpirationService certificateExpirationService,
        SnapshotAgingService snapshotAgingService,
        ConfigurationComplianceService configurationComplianceService,
        OrphanedFilesService orphanedFilesService,
        ToolsStatusService toolsStatusService,
        DatastoreCapacityService datastoreCapacityService,
        HostCapacityService hostCapacityService,
        VMResourceAllocationService vmResourceAllocationService,
        VMRightSizingService vmRightSizingService,
        VMCountTrendService vmCountTrendService,
        StorageGrowthService storageGrowthService,
        DatastoreCapacityTrendService datastoreCapacityTrendService,
        HostUtilizationService hostUtilizationService,
        VMConfigChangesService vmConfigChangesService,
        VMLifecycleService vmLifecycleService)
    {
        _exportService = exportService;
        _vmInventoryService = vmInventoryService;
        _hostInventoryService = hostInventoryService;
        _clusterSummaryService = clusterSummaryService;
        _datastoreInventoryService = datastoreInventoryService;
        _enterpriseSummaryService = enterpriseSummaryService;
        _networkTopologyService = networkTopologyService;
        _licenseComplianceService = licenseComplianceService;
        _resourcePoolService = resourcePoolService;
        _healthIssuesService = healthIssuesService;
        _certificateExpirationService = certificateExpirationService;
        _snapshotAgingService = snapshotAgingService;
        _configurationComplianceService = configurationComplianceService;
        _orphanedFilesService = orphanedFilesService;
        _toolsStatusService = toolsStatusService;
        _datastoreCapacityService = datastoreCapacityService;
        _hostCapacityService = hostCapacityService;
        _vmResourceAllocationService = vmResourceAllocationService;
        _vmRightSizingService = vmRightSizingService;
        _vmCountTrendService = vmCountTrendService;
        _storageGrowthService = storageGrowthService;
        _datastoreCapacityTrendService = datastoreCapacityTrendService;
        _hostUtilizationService = hostUtilizationService;
        _vmConfigChangesService = vmConfigChangesService;
        _vmLifecycleService = vmLifecycleService;
    }

    #region VM Inventory

    [HttpGet("vm-inventory/excel")]
    public async Task<IActionResult> ExportVMInventoryToExcel([FromQuery] VMInventoryFilter? filter = null)
    {
        filter ??= new VMInventoryFilter();
        var data = await _vmInventoryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Inventory");
        var content = _exportService.ExportToExcel(data, fileName, "VM Inventory");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("vm-inventory/csv")]
    public async Task<IActionResult> ExportVMInventoryToCsv([FromQuery] VMInventoryFilter? filter = null)
    {
        filter ??= new VMInventoryFilter();
        var data = await _vmInventoryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Inventory");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Host Inventory

    [HttpGet("host-inventory/excel")]
    public async Task<IActionResult> ExportHostInventoryToExcel([FromQuery] HostInventoryFilter? filter = null)
    {
        filter ??= new HostInventoryFilter();
        var data = await _hostInventoryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Host_Inventory");
        var content = _exportService.ExportToExcel(data, fileName, "Host Inventory");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("host-inventory/csv")]
    public async Task<IActionResult> ExportHostInventoryToCsv([FromQuery] HostInventoryFilter? filter = null)
    {
        filter ??= new HostInventoryFilter();
        var data = await _hostInventoryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Host_Inventory");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Cluster Summary

    [HttpGet("cluster-summary/excel")]
    public async Task<IActionResult> ExportClusterSummaryToExcel([FromQuery] ClusterSummaryFilter? filter = null)
    {
        filter ??= new ClusterSummaryFilter();
        var data = await _clusterSummaryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Cluster_Summary");
        var content = _exportService.ExportToExcel(data, fileName, "Cluster Summary");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("cluster-summary/csv")]
    public async Task<IActionResult> ExportClusterSummaryCsv([FromQuery] ClusterSummaryFilter? filter = null)
    {
        filter ??= new ClusterSummaryFilter();
        var data = await _clusterSummaryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Cluster_Summary");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Datastore Inventory

    [HttpGet("datastore-inventory/excel")]
    public async Task<IActionResult> ExportDatastoreInventoryToExcel([FromQuery] DatastoreInventoryFilter? filter = null)
    {
        filter ??= new DatastoreInventoryFilter();
        var data = await _datastoreInventoryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Datastore_Inventory");
        var content = _exportService.ExportToExcel(data, fileName, "Datastore Inventory");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("datastore-inventory/csv")]
    public async Task<IActionResult> ExportDatastoreInventoryToCsv([FromQuery] DatastoreInventoryFilter? filter = null)
    {
        filter ??= new DatastoreInventoryFilter();
        var data = await _datastoreInventoryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Datastore_Inventory");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Enterprise Summary

    [HttpGet("enterprise-summary/excel")]
    public async Task<IActionResult> ExportEnterpriseSummaryToExcel([FromQuery] EnterpriseSummaryFilter? filter = null)
    {
        filter ??= new EnterpriseSummaryFilter();
        var data = await _enterpriseSummaryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Enterprise_Summary");
        var content = _exportService.ExportToExcel(data, fileName, "Enterprise Summary");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("enterprise-summary/csv")]
    public async Task<IActionResult> ExportEnterpriseSummaryCsv([FromQuery] EnterpriseSummaryFilter? filter = null)
    {
        filter ??= new EnterpriseSummaryFilter();
        var data = await _enterpriseSummaryService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Enterprise_Summary");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Network Topology

    [HttpGet("network-topology/excel")]
    public async Task<IActionResult> ExportNetworkTopologyToExcel([FromQuery] NetworkTopologyFilter? filter = null)
    {
        filter ??= new NetworkTopologyFilter();
        var data = await _networkTopologyService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Network_Topology");
        var content = _exportService.ExportToExcel(data, fileName, "Network Topology");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("network-topology/csv")]
    public async Task<IActionResult> ExportNetworkTopologyCsv([FromQuery] NetworkTopologyFilter? filter = null)
    {
        filter ??= new NetworkTopologyFilter();
        var data = await _networkTopologyService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Network_Topology");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region License Compliance

    [HttpGet("license-compliance/excel")]
    public async Task<IActionResult> ExportLicenseComplianceToExcel([FromQuery] LicenseComplianceFilter? filter = null)
    {
        filter ??= new LicenseComplianceFilter();
        var data = await _licenseComplianceService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("License_Compliance");
        var content = _exportService.ExportToExcel(data, fileName, "License Compliance");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("license-compliance/csv")]
    public async Task<IActionResult> ExportLicenseComplianceCsv([FromQuery] LicenseComplianceFilter? filter = null)
    {
        filter ??= new LicenseComplianceFilter();
        var data = await _licenseComplianceService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("License_Compliance");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Resource Pool

    [HttpGet("resource-pool/excel")]
    public async Task<IActionResult> ExportResourcePoolToExcel([FromQuery] ResourcePoolFilter? filter = null)
    {
        filter ??= new ResourcePoolFilter();
        var data = await _resourcePoolService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Resource_Pool_Utilization");
        var content = _exportService.ExportToExcel(data, fileName, "Resource Pool");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("resource-pool/csv")]
    public async Task<IActionResult> ExportResourcePoolCsv([FromQuery] ResourcePoolFilter? filter = null)
    {
        filter ??= new ResourcePoolFilter();
        var data = await _resourcePoolService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Resource_Pool_Utilization");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Health Issues

    [HttpGet("health-issues/excel")]
    public async Task<IActionResult> ExportHealthIssuesToExcel([FromQuery] HealthIssuesFilter? filter = null)
    {
        filter ??= new HealthIssuesFilter();
        var data = await _healthIssuesService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Health_Issues");
        var content = _exportService.ExportToExcel(data, fileName, "Health Issues");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("health-issues/csv")]
    public async Task<IActionResult> ExportHealthIssuesToCsv([FromQuery] HealthIssuesFilter? filter = null)
    {
        filter ??= new HealthIssuesFilter();
        var data = await _healthIssuesService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Health_Issues");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Certificate Expiration

    [HttpGet("certificate-expiration/excel")]
    public async Task<IActionResult> ExportCertificateExpirationToExcel([FromQuery] CertificateExpirationFilter? filter = null)
    {
        filter ??= new CertificateExpirationFilter();
        var data = await _certificateExpirationService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Certificate_Expiration");
        var content = _exportService.ExportToExcel(data, fileName, "Certificate Expiration");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("certificate-expiration/csv")]
    public async Task<IActionResult> ExportCertificateExpirationToCsv([FromQuery] CertificateExpirationFilter? filter = null)
    {
        filter ??= new CertificateExpirationFilter();
        var data = await _certificateExpirationService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Certificate_Expiration");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Snapshot Aging

    [HttpGet("snapshot-aging/excel")]
    public async Task<IActionResult> ExportSnapshotAgingToExcel([FromQuery] SnapshotAgingFilter? filter = null)
    {
        filter ??= new SnapshotAgingFilter();
        var data = await _snapshotAgingService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Snapshot_Aging");
        var content = _exportService.ExportToExcel(data, fileName, "Snapshot Aging");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("snapshot-aging/csv")]
    public async Task<IActionResult> ExportSnapshotAgingToCsv([FromQuery] SnapshotAgingFilter? filter = null)
    {
        filter ??= new SnapshotAgingFilter();
        var data = await _snapshotAgingService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Snapshot_Aging");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Configuration Compliance

    [HttpGet("configuration-compliance/excel")]
    public async Task<IActionResult> ExportConfigurationComplianceToExcel([FromQuery] ConfigurationComplianceFilter? filter = null)
    {
        filter ??= new ConfigurationComplianceFilter();
        var data = await _configurationComplianceService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Configuration_Compliance");
        var content = _exportService.ExportToExcel(data, fileName, "Configuration Compliance");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("configuration-compliance/csv")]
    public async Task<IActionResult> ExportConfigurationComplianceToCsv([FromQuery] ConfigurationComplianceFilter? filter = null)
    {
        filter ??= new ConfigurationComplianceFilter();
        var data = await _configurationComplianceService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Configuration_Compliance");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Orphaned Files

    [HttpGet("orphaned-files/excel")]
    public async Task<IActionResult> ExportOrphanedFilesToExcel([FromQuery] OrphanedFilesFilter? filter = null)
    {
        filter ??= new OrphanedFilesFilter();
        var data = await _orphanedFilesService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Orphaned_Files");
        var content = _exportService.ExportToExcel(data, fileName, "Orphaned Files");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("orphaned-files/csv")]
    public async Task<IActionResult> ExportOrphanedFilesToCsv([FromQuery] OrphanedFilesFilter? filter = null)
    {
        filter ??= new OrphanedFilesFilter();
        var data = await _orphanedFilesService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Orphaned_Files");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Tools Status

    [HttpGet("tools-status/excel")]
    public async Task<IActionResult> ExportToolsStatusToExcel([FromQuery] ToolsStatusFilter? filter = null)
    {
        filter ??= new ToolsStatusFilter();
        var data = await _toolsStatusService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Tools_Status");
        var content = _exportService.ExportToExcel(data, fileName, "Tools Status");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("tools-status/csv")]
    public async Task<IActionResult> ExportToolsStatusToCsv([FromQuery] ToolsStatusFilter? filter = null)
    {
        filter ??= new ToolsStatusFilter();
        var data = await _toolsStatusService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Tools_Status");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Datastore Capacity

    [HttpGet("datastore-capacity/excel")]
    public async Task<IActionResult> ExportDatastoreCapacityToExcel([FromQuery] DatastoreCapacityFilter? filter = null)
    {
        filter ??= new DatastoreCapacityFilter();
        var data = await _datastoreCapacityService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Datastore_Capacity");
        var content = _exportService.ExportToExcel(data, fileName, "Datastore Capacity");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("datastore-capacity/csv")]
    public async Task<IActionResult> ExportDatastoreCapacityToCsv([FromQuery] DatastoreCapacityFilter? filter = null)
    {
        filter ??= new DatastoreCapacityFilter();
        var data = await _datastoreCapacityService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Datastore_Capacity");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Host Capacity

    [HttpGet("host-capacity/excel")]
    public async Task<IActionResult> ExportHostCapacityToExcel([FromQuery] HostCapacityFilter? filter = null)
    {
        filter ??= new HostCapacityFilter();
        var data = await _hostCapacityService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Host_Capacity");
        var content = _exportService.ExportToExcel(data, fileName, "Host Capacity");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("host-capacity/csv")]
    public async Task<IActionResult> ExportHostCapacityToCsv([FromQuery] HostCapacityFilter? filter = null)
    {
        filter ??= new HostCapacityFilter();
        var data = await _hostCapacityService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Host_Capacity");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region VM Resource Allocation

    [HttpGet("vm-resource-allocation/excel")]
    public async Task<IActionResult> ExportVMResourceAllocationToExcel([FromQuery] VMResourceAllocationFilter? filter = null)
    {
        filter ??= new VMResourceAllocationFilter();
        var data = await _vmResourceAllocationService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Resource_Allocation");
        var content = _exportService.ExportToExcel(data, fileName, "VM Resource Allocation");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("vm-resource-allocation/csv")]
    public async Task<IActionResult> ExportVMResourceAllocationToCsv([FromQuery] VMResourceAllocationFilter? filter = null)
    {
        filter ??= new VMResourceAllocationFilter();
        var data = await _vmResourceAllocationService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Resource_Allocation");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region VM Right-Sizing

    [HttpGet("vm-rightsizing/excel")]
    public async Task<IActionResult> ExportVMRightSizingToExcel([FromQuery] VMRightSizingFilter? filter = null)
    {
        filter ??= new VMRightSizingFilter();
        var data = await _vmRightSizingService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_RightSizing");
        var content = _exportService.ExportToExcel(data, fileName, "VM Right-Sizing");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("vm-rightsizing/csv")]
    public async Task<IActionResult> ExportVMRightSizingToCsv([FromQuery] VMRightSizingFilter? filter = null)
    {
        filter ??= new VMRightSizingFilter();
        var data = await _vmRightSizingService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_RightSizing");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region VM Count Trend

    [HttpGet("vm-count-trend/excel")]
    public async Task<IActionResult> ExportVMCountTrendToExcel([FromQuery] VMCountTrendFilter? filter = null)
    {
        filter ??= new VMCountTrendFilter();
        var data = await _vmCountTrendService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Count_Trend");
        var content = _exportService.ExportToExcel(data, fileName, "VM Count Trend");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("vm-count-trend/csv")]
    public async Task<IActionResult> ExportVMCountTrendToCsv([FromQuery] VMCountTrendFilter? filter = null)
    {
        filter ??= new VMCountTrendFilter();
        var data = await _vmCountTrendService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Count_Trend");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Storage Growth

    [HttpGet("storage-growth/excel")]
    public async Task<IActionResult> ExportStorageGrowthToExcel([FromQuery] StorageGrowthFilter? filter = null)
    {
        filter ??= new StorageGrowthFilter();
        var data = await _storageGrowthService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Storage_Growth");
        var content = _exportService.ExportToExcel(data, fileName, "Storage Growth");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("storage-growth/csv")]
    public async Task<IActionResult> ExportStorageGrowthToCsv([FromQuery] StorageGrowthFilter? filter = null)
    {
        filter ??= new StorageGrowthFilter();
        var data = await _storageGrowthService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Storage_Growth");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Datastore Capacity Trend

    [HttpGet("datastore-capacity-trend/excel")]
    public async Task<IActionResult> ExportDatastoreCapacityTrendToExcel([FromQuery] DatastoreCapacityTrendFilter? filter = null)
    {
        filter ??= new DatastoreCapacityTrendFilter();
        var data = await _datastoreCapacityTrendService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Datastore_Capacity_Trend");
        var content = _exportService.ExportToExcel(data, fileName, "Datastore Capacity Trend");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("datastore-capacity-trend/csv")]
    public async Task<IActionResult> ExportDatastoreCapacityTrendToCsv([FromQuery] DatastoreCapacityTrendFilter? filter = null)
    {
        filter ??= new DatastoreCapacityTrendFilter();
        var data = await _datastoreCapacityTrendService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Datastore_Capacity_Trend");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region Host Utilization

    [HttpGet("host-utilization/excel")]
    public async Task<IActionResult> ExportHostUtilizationToExcel([FromQuery] HostUtilizationFilter? filter = null)
    {
        filter ??= new HostUtilizationFilter();
        var data = await _hostUtilizationService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Host_Utilization");
        var content = _exportService.ExportToExcel(data, fileName, "Host Utilization");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("host-utilization/csv")]
    public async Task<IActionResult> ExportHostUtilizationToCsv([FromQuery] HostUtilizationFilter? filter = null)
    {
        filter ??= new HostUtilizationFilter();
        var data = await _hostUtilizationService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("Host_Utilization");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region VM Config Changes

    [HttpGet("vm-config-changes/excel")]
    public async Task<IActionResult> ExportVMConfigChangesToExcel([FromQuery] VMConfigChangesFilter? filter = null)
    {
        filter ??= new VMConfigChangesFilter();
        var data = await _vmConfigChangesService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Config_Changes");
        var content = _exportService.ExportToExcel(data, fileName, "VM Config Changes");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("vm-config-changes/csv")]
    public async Task<IActionResult> ExportVMConfigChangesToCsv([FromQuery] VMConfigChangesFilter? filter = null)
    {
        filter ??= new VMConfigChangesFilter();
        var data = await _vmConfigChangesService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Config_Changes");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    #region VM Lifecycle

    [HttpGet("vm-lifecycle/excel")]
    public async Task<IActionResult> ExportVMLifecycleToExcel([FromQuery] VMLifecycleFilter? filter = null)
    {
        filter ??= new VMLifecycleFilter();
        var data = await _vmLifecycleService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Lifecycle");
        var content = _exportService.ExportToExcel(data, fileName, "VM Lifecycle");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    [HttpGet("vm-lifecycle/csv")]
    public async Task<IActionResult> ExportVMLifecycleToCsv([FromQuery] VMLifecycleFilter? filter = null)
    {
        filter ??= new VMLifecycleFilter();
        var data = await _vmLifecycleService.GetReportDataAsync(filter);
        var fileName = GenerateFileName("VM_Lifecycle");
        var content = _exportService.ExportToCsv(data, fileName);
        return File(content, "text/csv", $"{fileName}.csv");
    }

    #endregion

    private static string GenerateFileName(string reportName)
    {
        return $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}";
    }
}
