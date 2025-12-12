using RVToolsService;
using RVToolsService.Services;
using RVToolsShared.Data;
using RVToolsShared.Security;
using Microsoft.AspNetCore.DataProtection;

// Check for test mode: dotnet run -- --test-excel /path/to/file.xlsx
if (args.Length >= 2 && args[0] == "--test-excel")
{
    var testFilePath = args[1];
    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    var excelReader = new ExcelReaderService(loggerFactory.CreateLogger<ExcelReaderService>());
    return await TestExcelReader.RunAsync(excelReader, testFilePath);
}

var builder = Host.CreateApplicationBuilder(args);

// Add Windows Service support
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "RVToolsImportService";
});

// Configure Data Protection (shared key store with web app)
var keyStorePath = builder.Configuration["DataProtection:KeyStorePath"]
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RVTools", "keys");

builder.Services.AddDataProtection()
    .SetApplicationName(builder.Configuration["DataProtection:ApplicationName"] ?? "RVTools")
    .PersistKeysToFileSystem(new DirectoryInfo(keyStorePath));

// Register shared services
builder.Services.AddSingleton<ISqlConnectionFactory>(sp =>
    new SqlConnectionFactory(builder.Configuration));
builder.Services.AddSingleton<ICredentialProtectionService, CredentialProtectionService>();

// Register service-specific services
builder.Services.AddSingleton<IExcelReaderService, ExcelReaderService>();

// Register hosted service (Worker)
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
return 0;
