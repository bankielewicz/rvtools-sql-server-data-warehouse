using Microsoft.AspNetCore.Authentication.Cookies;
using RVToolsWeb.Configuration;
using RVToolsWeb.Data;
using RVToolsWeb.Data.Repositories;
using RVToolsWeb.Middleware;
using RVToolsWeb.Services;
using RVToolsWeb.Services.Admin;
using RVToolsWeb.Services.Auth;
using RVToolsWeb.Services.Capacity;
using RVToolsWeb.Services.Health;
using RVToolsWeb.Services.Home;
using RVToolsWeb.Services.Interfaces;
using RVToolsWeb.Services.Inventory;
using RVToolsWeb.Services.Logging;
using RVToolsWeb.Services.Trends;

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

// Data Protection - for encrypting sensitive credentials
builder.Services.AddDataProtection();

// Authentication Services
builder.Services.AddSingleton<ICredentialProtectionService, CredentialProtectionService>();
builder.Services.AddSingleton<ILdapConnectionPool, LdapConnectionPool>(); // Singleton for connection pooling
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILdapService, LdapService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = builder.Configuration["Authentication:CookieName"] ?? "RVToolsDW.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(
            builder.Configuration.GetValue<int>("Authentication:CookieExpirationMinutes", 480));
        options.SlidingExpiration = builder.Configuration.GetValue<bool>("Authentication:SlidingExpiration", true);
    });

// Anti-forgery - configure to accept tokens from headers for AJAX requests
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

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
app.UseAuthentication();
app.UseAuthorization();
app.UseFirstTimeSetup();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
