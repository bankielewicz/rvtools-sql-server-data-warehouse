<#
.SYNOPSIS
    Import RVTools xlsx exports into SQL Server Data Warehouse

.DESCRIPTION
    Main script for importing RVTools exports. Processes all xlsx files
    in the incoming folder and loads them into the RVToolsDW database.

.PARAMETER ServerInstance
    SQL Server instance name (default: localhost)

.PARAMETER Database
    Database name (default: RVToolsDW)

.PARAMETER Credential
    SQL Server credential (optional, uses Windows auth if not specified)

.PARAMETER UseSqlAuth
    Use SQL Server authentication (will prompt for credentials if not provided)

.PARAMETER IncomingFolder
    Folder containing xlsx files to import (default: ../incoming)

.PARAMETER LogLevel
    Logging level: Verbose, Info, Warning, Error (default: Info)

.PARAMETER SingleFile
    Process only this specific file

.EXAMPLE
    # Import all files with Windows auth
    .\Import-RVToolsData.ps1 -ServerInstance "localhost"

.EXAMPLE
    # Import with SQL auth (will prompt for credentials)
    .\Import-RVToolsData.ps1 -ServerInstance "localhost" -UseSqlAuth

.EXAMPLE
    # Import with SQL auth using pre-defined credential
    $cred = Get-Credential
    .\Import-RVToolsData.ps1 -ServerInstance "sqlserver.domain.com" -UseSqlAuth -Credential $cred

.EXAMPLE
    # Import single file with verbose logging
    .\Import-RVToolsData.ps1 -SingleFile "C:\incoming\export.xlsx" -LogLevel Verbose

.NOTES
    Requires: ImportExcel, SqlServer PowerShell modules
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$ServerInstance = "localhost",

    [Parameter()]
    [string]$Database = "RVToolsDW",

    [Parameter()]
    [PSCredential]$Credential = $null,

    [Parameter()]
    [switch]$UseSqlAuth,

    [Parameter()]
    [string]$IncomingFolder = $null,

    [Parameter()]
    [ValidateSet('Verbose', 'Info', 'Warning', 'Error')]
    [string]$LogLevel = 'Info',

    [Parameter()]
    [string]$SingleFile = $null
)

# ============================================================================
# Setup
# ============================================================================

$ErrorActionPreference = 'Stop'
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)

# Import module
$modulePath = Join-Path $scriptPath "modules\RVToolsImport.psm1"
if (-not (Test-Path $modulePath)) {
    Write-Error "Module not found: $modulePath"
    exit 1
}
Import-Module $modulePath -Force

# Set default incoming folder
if (-not $IncomingFolder) {
    $IncomingFolder = Join-Path $projectRoot "incoming"
}

# Handle SQL authentication
if ($UseSqlAuth) {
    if (-not $Credential) {
        Write-Host "SQL Authentication selected. Please enter credentials." -ForegroundColor Cyan
        $Credential = Get-Credential -Message "Enter SQL Server credentials (username and password)"

        if (-not $Credential) {
            Write-Error "SQL Authentication requires credentials. Operation cancelled."
            exit 1
        }
    }
}

# Validate dependencies
$requiredModules = @('ImportExcel', 'SqlServer')
foreach ($module in $requiredModules) {
    if (-not (Get-Module -ListAvailable -Name $module)) {
        Write-Host "Required module not installed: $module" -ForegroundColor Yellow
        $response = Read-Host "Would you like to install it now? (Y/N)"
        if ($response -eq 'Y' -or $response -eq 'y') {
            try {
                Write-Host "Installing $module..." -ForegroundColor Cyan
                Install-Module -Name $module -Scope CurrentUser -Force -AllowClobber
                Write-Host "$module installed successfully." -ForegroundColor Green
            }
            catch {
                Write-Error "Failed to install $module. Please install manually: Install-Module $module -Scope CurrentUser"
                exit 1
            }
        }
        else {
            Write-Error "Required module $module is not installed. Exiting."
            exit 1
        }
    }
}

# ============================================================================
# Main Execution
# ============================================================================

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "RVTools Data Warehouse Import" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Server:    $ServerInstance" -ForegroundColor Gray
Write-Host "Database:  $Database" -ForegroundColor Gray
Write-Host "Auth:      $(if ($UseSqlAuth -or $Credential) { 'SQL' } else { 'Windows' })" -ForegroundColor Gray
Write-Host "LogLevel:  $LogLevel" -ForegroundColor Gray
Write-Host ""

# Get files to process
if ($SingleFile) {
    if (-not (Test-Path $SingleFile)) {
        Write-Error "File not found: $SingleFile"
        exit 1
    }
    $filesToProcess = @(Get-Item $SingleFile)
} else {
    if (-not (Test-Path $IncomingFolder)) {
        Write-Warning "Incoming folder does not exist: $IncomingFolder"
        exit 0
    }
    $filesToProcess = Get-ChildItem -Path $IncomingFolder -Filter "*.xlsx" | Where-Object { $_.Name -notlike '~$*' }
}

if ($filesToProcess.Count -eq 0) {
    Write-Host "No xlsx files found to process." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($filesToProcess.Count) file(s) to process:" -ForegroundColor Green
$filesToProcess | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
Write-Host ""

# Process each file
$results = @()
$successCount = 0
$failCount = 0

foreach ($file in $filesToProcess) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan
    Write-Host ("-" * 50) -ForegroundColor Gray

    try {
        $result = Import-RVToolsFile `
            -FilePath $file.FullName `
            -ServerInstance $ServerInstance `
            -Database $Database `
            -Credential $Credential `
            -LogLevel $LogLevel

        $results += $result

        if ($result.Status -eq 'Success') {
            $successCount++
            Write-Host "SUCCESS: $($result.TotalStagedRows) rows staged" -ForegroundColor Green
        } elseif ($result.Status -eq 'Partial') {
            $successCount++
            Write-Host "PARTIAL: $($result.TotalStagedRows) staged, $($result.TotalFailedRows) failed" -ForegroundColor Yellow
        } else {
            $failCount++
            Write-Host "FAILED" -ForegroundColor Red
        }
    }
    catch {
        $failCount++
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }

    Write-Host ""
}

# Summary
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Import Summary" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Files Processed: $($filesToProcess.Count)" -ForegroundColor White
Write-Host "Successful:      $successCount" -ForegroundColor Green
Write-Host "Failed:          $failCount" -ForegroundColor $(if ($failCount -gt 0) { 'Red' } else { 'White' })
Write-Host ""

if ($results.Count -gt 0) {
    Write-Host "Details:" -ForegroundColor Gray
    $results | Format-Table -Property @(
        @{Name='File'; Expression={$_.FileName}; Width=40},
        @{Name='Status'; Expression={$_.Status}; Width=10},
        @{Name='Sheets'; Expression={$_.SheetsProcessed}; Width=8},
        @{Name='Staged'; Expression={$_.TotalStagedRows}; Width=10},
        @{Name='Failed'; Expression={$_.TotalFailedRows}; Width=8},
        @{Name='Duration'; Expression={"$($_.DurationSeconds)s"}; Width=10}
    ) -AutoSize
}

# Exit with appropriate code
if ($failCount -gt 0) {
    exit 1
} else {
    exit 0
}
