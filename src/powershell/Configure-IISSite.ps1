#Requires -Version 5.1
#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Configures IIS for hosting a .NET web application.

.DESCRIPTION
    This script creates or updates an IIS Application Pool and Website
    for hosting a .NET Core/8 web application.

.PARAMETER SiteName
    Name of the IIS website to create.

.PARAMETER AppPoolName
    Name of the application pool. Defaults to SiteName if not specified.

.PARAMETER PhysicalPath
    Physical path to the published web application files.

.PARAMETER Port
    Port number for the website binding. Default is 80.

.PARAMETER HostHeader
    Optional host header (hostname) for the binding.

.PARAMETER UseSsl
    If specified, creates an HTTPS binding on port 443 (or specified port).

.PARAMETER AppPoolIdentity
    Application pool identity type. Default is ApplicationPoolIdentity.
    Options: LocalSystem, LocalService, NetworkService, ApplicationPoolIdentity, SpecificUser

.PARAMETER Force
    If specified, removes and recreates existing site/app pool.

.EXAMPLE
    .\Configure-IISSite.ps1 -SiteName "MyWebApp" -PhysicalPath "C:\inetpub\wwwroot\MyApp" -Port 8080

.EXAMPLE
    .\Configure-IISSite.ps1 -SiteName "MyWebApp" -PhysicalPath "C:\inetpub\wwwroot\MyApp" -HostHeader "myapp.local" -Force
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SiteName,

    [Parameter(Mandatory = $false)]
    [string]$AppPoolName,

    [Parameter(Mandatory = $true)]
    [string]$PhysicalPath,

    [Parameter(Mandatory = $false)]
    [int]$Port = 80,

    [Parameter(Mandatory = $false)]
    [string]$HostHeader = "",

    [Parameter(Mandatory = $false)]
    [switch]$UseSsl,

    [Parameter(Mandatory = $false)]
    [ValidateSet("LocalSystem", "LocalService", "NetworkService", "ApplicationPoolIdentity", "SpecificUser")]
    [string]$AppPoolIdentity = "ApplicationPoolIdentity",

    [Parameter(Mandatory = $false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Default app pool name to site name
if (-not $AppPoolName) {
    $AppPoolName = $SiteName
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  IIS Site Configuration Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Import IIS module
Write-Host "[1/6] Loading IIS module..." -ForegroundColor Yellow

# Check if IIS is installed
$iisFeature = Get-WindowsFeature -Name Web-Server -ErrorAction SilentlyContinue
if ($null -eq $iisFeature) {
    # Try Windows 10/11 method
    $iisFeature = Get-WindowsOptionalFeature -Online -FeatureName IIS-WebServer -ErrorAction SilentlyContinue
    if ($null -eq $iisFeature -or $iisFeature.State -ne "Enabled") {
        Write-Error "IIS is not installed. Please install IIS first."
        exit 1
    }
}

Import-Module WebAdministration -ErrorAction Stop
Write-Host "      IIS module loaded." -ForegroundColor Green

# Step 2: Validate physical path
Write-Host "[2/6] Validating physical path..." -ForegroundColor Yellow
if (-not (Test-Path $PhysicalPath)) {
    Write-Host "      Creating directory: $PhysicalPath" -ForegroundColor Gray
    New-Item -Path $PhysicalPath -ItemType Directory -Force | Out-Null
}
Write-Host "      Path validated: $PhysicalPath" -ForegroundColor Green

# Step 3: Create or update Application Pool
Write-Host "[3/6] Configuring Application Pool: $AppPoolName" -ForegroundColor Yellow

$appPoolPath = "IIS:\AppPools\$AppPoolName"
$appPoolExists = Test-Path $appPoolPath

if ($appPoolExists -and $Force) {
    Write-Host "      Removing existing app pool..." -ForegroundColor Gray
    
    # Stop the app pool first
    if ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Stopped") {
        Stop-WebAppPool -Name $AppPoolName
        Start-Sleep -Seconds 2
    }
    
    Remove-WebAppPool -Name $AppPoolName
    $appPoolExists = $false
}

if (-not $appPoolExists) {
    Write-Host "      Creating new application pool..." -ForegroundColor Gray
    New-WebAppPool -Name $AppPoolName | Out-Null
}

# Configure app pool for .NET Core/8 (No Managed Code)
Set-ItemProperty -Path $appPoolPath -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path $appPoolPath -Name "managedPipelineMode" -Value "Integrated"
Set-ItemProperty -Path $appPoolPath -Name "startMode" -Value "OnDemand"
Set-ItemProperty -Path $appPoolPath -Name "processModel.identityType" -Value $AppPoolIdentity

# Enable 32-bit applications if needed (usually not for .NET 8)
Set-ItemProperty -Path $appPoolPath -Name "enable32BitAppOnWin64" -Value $false

Write-Host "      Application pool configured." -ForegroundColor Green

# Step 4: Create or update Website
Write-Host "[4/6] Configuring Website: $SiteName" -ForegroundColor Yellow

$sitePath = "IIS:\Sites\$SiteName"
$siteExists = Test-Path $sitePath

if ($siteExists -and $Force) {
    Write-Host "      Removing existing site..." -ForegroundColor Gray
    Remove-WebSite -Name $SiteName
    $siteExists = $false
}

$protocol = if ($UseSsl) { "https" } else { "http" }
$bindingInfo = "*:${Port}:${HostHeader}"

if (-not $siteExists) {
    Write-Host "      Creating new website..." -ForegroundColor Gray
    
    $newSiteParams = @{
        Name            = $SiteName
        PhysicalPath    = $PhysicalPath
        ApplicationPool = $AppPoolName
    }
    
    New-WebSite @newSiteParams | Out-Null
    
    # Remove default binding and add our custom one
    Remove-WebBinding -Name $SiteName -BindingInformation "*:80:" -ErrorAction SilentlyContinue
    New-WebBinding -Name $SiteName -Protocol $protocol -Port $Port -HostHeader $HostHeader
}
else {
    # Update existing site
    Set-ItemProperty -Path $sitePath -Name "physicalPath" -Value $PhysicalPath
    Set-ItemProperty -Path $sitePath -Name "applicationPool" -Value $AppPoolName
}

Write-Host "      Website configured." -ForegroundColor Green

# Step 5: Set folder permissions
Write-Host "[5/6] Setting folder permissions..." -ForegroundColor Yellow

$acl = Get-Acl $PhysicalPath

# Add IIS_IUSRS read/execute permission
$iisUsersRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "IIS_IUSRS",
    "ReadAndExecute",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)
$acl.SetAccessRule($iisUsersRule)

# Add app pool identity permission if using ApplicationPoolIdentity
if ($AppPoolIdentity -eq "ApplicationPoolIdentity") {
    $appPoolSid = "IIS AppPool\$AppPoolName"
    $appPoolRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        $appPoolSid,
        "ReadAndExecute",
        "ContainerInherit,ObjectInherit",
        "None",
        "Allow"
    )
    $acl.SetAccessRule($appPoolRule)
}

Set-Acl -Path $PhysicalPath -AclObject $acl
Write-Host "      Permissions set." -ForegroundColor Green

# Step 6: Start the site and app pool
Write-Host "[6/6] Starting site and application pool..." -ForegroundColor Yellow

# Start app pool
if ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Started") {
    Start-WebAppPool -Name $AppPoolName
}

# Start website
if ((Get-WebsiteState -Name $SiteName).Value -ne "Started") {
    Start-Website -Name $SiteName
}

Write-Host "      Site started." -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Configuration Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Site Name:      $SiteName" -ForegroundColor White
Write-Host "  App Pool:       $AppPoolName" -ForegroundColor White
Write-Host "  Physical Path:  $PhysicalPath" -ForegroundColor White
Write-Host "  Binding:        ${protocol}://localhost:$Port" -ForegroundColor White
if ($HostHeader) {
    Write-Host "  Host Header:    $HostHeader" -ForegroundColor White
}
Write-Host ""
Write-Host "  Browse: ${protocol}://localhost:$Port" -ForegroundColor Cyan
Write-Host ""