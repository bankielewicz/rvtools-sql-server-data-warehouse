#Requires -Version 5.1
<#
.SYNOPSIS
    Publishes a .NET web application to a local folder for IIS deployment.

.DESCRIPTION
    This script builds and publishes a .NET web application using dotnet publish,
    then copies the output to the IIS web root.

.PARAMETER ProjectPath
    Path to the .csproj file or solution folder.

.PARAMETER Configuration
    Build configuration (Debug/Release). Default is Release.

.PARAMETER OutputPath
    Local publish output folder. Default is .\publish

.PARAMETER IISSitePath
    Destination path for IIS (e.g., C:\inetpub\wwwroot\MyApp)

.PARAMETER Framework
    Target framework (e.g., net8.0). Optional - uses project default if not specified.

.EXAMPLE
    .\Publish-WebApp.ps1 -ProjectPath "D:\Projects\MyApp\MyApp.csproj" -IISSitePath "C:\inetpub\wwwroot\MyApp"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,

    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = ".\publish",

    [Parameter(Mandatory = $true)]
    [string]$IISSitePath,

    [Parameter(Mandatory = $false)]
    [string]$Framework
)

$ErrorActionPreference = "Stop"

# Validate project path
if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project path not found: $ProjectPath"
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  .NET Web Application Publisher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean previous publish output
Write-Host "[1/4] Cleaning previous publish output..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
    Write-Host "      Removed: $OutputPath" -ForegroundColor Gray
}

# Step 2: Build and publish the application
Write-Host "[2/4] Publishing application..." -ForegroundColor Yellow

$publishArgs = @(
    "publish"
    $ProjectPath
    "--configuration", $Configuration
    "--output", $OutputPath
    "--self-contained", "false"
)

if ($Framework) {
    $publishArgs += "--framework", $Framework
}

Write-Host "      Running: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray

& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code $LASTEXITCODE"
    exit 1
}

Write-Host "      Published to: $OutputPath" -ForegroundColor Green

# Step 3: Stop IIS site/app pool if exists (to release file locks)
Write-Host "[3/4] Preparing IIS destination..." -ForegroundColor Yellow

# Create destination if it doesn't exist
if (-not (Test-Path $IISSitePath)) {
    New-Item -Path $IISSitePath -ItemType Directory -Force | Out-Null
    Write-Host "      Created directory: $IISSitePath" -ForegroundColor Gray
}

# Step 4: Copy files to IIS location
Write-Host "[4/4] Copying files to IIS site path..." -ForegroundColor Yellow

# Use robocopy for reliable copying (mirrors source to destination)
$robocopyArgs = @(
    $OutputPath
    $IISSitePath
    "/MIR"      # Mirror directory tree
    "/NFL"      # No file list
    "/NDL"      # No directory list
    "/NJH"      # No job header
    "/NJS"      # No job summary
    "/NC"       # No class
    "/NS"       # No size
    "/NP"       # No progress
)

& robocopy @robocopyArgs

# Robocopy exit codes: 0-7 are success, 8+ are errors
if ($LASTEXITCODE -ge 8) {
    Write-Error "Robocopy failed with exit code $LASTEXITCODE"
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Publish completed successfully!" -ForegroundColor Green
Write-Host "  Location: $IISSitePath" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green