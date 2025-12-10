using RVToolsWeb.Configuration;
using RVToolsWeb.Data;
using RVToolsWeb.Data.Repositories;
using RVToolsWeb.Middleware;
using RVToolsWeb.Services;
using RVToolsWeb.Services.Capacity;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Home;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;
using RVToolsWeb.Services.Logging;
using RVToolsWeb.Services.Trends;
using RVToolsWeb.Services.Admin;

var builder = WebApplication.CreateBuilder(args);

// Configuration - bind appsettings.json to strongly-typed classes
builder.Services.Configure<AppSettings>(builder.Configuration);

// Data Layer - Dapper connection factory (singleton for connection string reuse)
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

// Repositories
builder.Services.AddScoped<FilterRepository>();

// Caching - for filter dropdown values
builder.Services.AddMemoryCache();

// Services
builder.Services.AddScoped<IFilterService, FilterService>();
builder.Services.AddScoped<IExportService, ExportService>();

// Logging Service
builder.Services.AddScoped<IWebLoggingService, WebLoggingService>();

// Dashboard Service
builder.Services.AddScoped<DashboardService>();

// Report Services - Inventory
builder.Services.AddScoped<VMInventoryService>();
builder.Services.AddScoped<HostInventoryService>();
builder.Services.AddScoped<ClusterSummaryService>();
builder.Services.AddScoped<DatastoreInventoryService>();
builder.Services.AddScoped<EnterpriseSummaryService>();
builder.Services.AddScoped<NetworkTopologyService>();
builder.Services.AddScoped<LicenseComplianceService>();
builder.Services.AddScoped<ResourcePoolService>();

// Report Services - Health
builder.Services.AddScoped<HealthIssuesService>();
builder.Services.AddScoped<CertificateExpirationService>();
builder.Services.AddScoped<SnapshotAgingService>();
builder.Services.AddScoped<ConfigurationComplianceService>();
builder.Services.AddScoped<OrphanedFilesService>();
builder.Services.AddScoped<ToolsStatusService>();

// Report Services - Capacity
builder.Services.AddScoped<DatastoreCapacityService>();
builder.Services.AddScoped<HostCapacityService>();
builder.Services.AddScoped<VMResourceAllocationService>();
builder.Services.AddScoped<VMRightSizingService>();

// Report Services - Trends
builder.Services.AddScoped<VMCountTrendService>();
builder.Services.AddScoped<StorageGrowthService>();
builder.Services.AddScoped<DatastoreCapacityTrendService>();
builder.Services.AddScoped<HostUtilizationService>();
builder.Services.AddScoped<VMConfigChangesService>();
builder.Services.AddScoped<VMLifecycleService>();

// Admin Services
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<ITableRetentionService, TableRetentionService>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.AddScoped<IDatabaseStatusService, DatabaseStatusService>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global exception handler - must be first to catch all exceptions
app.UseGlobalExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    // Keep default exception handler as fallback for edge cases
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
