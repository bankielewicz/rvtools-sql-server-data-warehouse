<#
.SYNOPSIS
    Imports historical RVTools Excel files with dates parsed from filenames.

.DESCRIPTION
    Processes files matching pattern: vCenter{xx}_{d_mm_yyyy}.domain.com.xlsx

    Files are sorted by parsed date (oldest first) to maintain correct
    History table chronology. The parsed date becomes ValidFrom in History tables,
    enabling accurate point-in-time queries.

.PARAMETER IncomingFolder
    Folder containing xlsx files to import. Default: ../incoming

.PARAMETER ServerInstance
    SQL Server instance to connect to. Default: localhost

.PARAMETER Database
    Database name. Default: RVToolsDW

.PARAMETER UseSqlAuth
    Use SQL Server authentication instead of Windows authentication.

.PARAMETER Credential
    SQL Server credential (use with -UseSqlAuth or pass directly).

.PARAMETER LogLevel
    Logging verbosity: Verbose, Info, Warning, Error. Default: Info

.PARAMETER WhatIf
    Show what files would be processed without actually importing.

.PARAMETER Force
    Skip confirmation prompt before importing.

.EXAMPLE
    .\Import-RVToolsHistoricalData.ps1 -WhatIf
    # Shows sorted file list without importing

.EXAMPLE
    .\Import-RVToolsHistoricalData.ps1 -IncomingFolder "C:\HistoricalExports" -LogLevel Verbose
    # Imports all matching files from specified folder

.EXAMPLE
    .\Import-RVToolsHistoricalData.ps1 -ServerInstance "server\instance" -UseSqlAuth
    # Prompts for SQL credentials and imports

.NOTES
    Requires: ImportExcel, SqlServer PowerShell modules
    Files must match pattern: vCenter{xx}_{d_mm_yyyy}.domain.com.xlsx

    IMPORTANT: Files are processed in chronological order (oldest first).
    This ensures the History table ValidFrom/ValidTo timeline is correct.
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$IncomingFolder = (Join-Path $PSScriptRoot "../incoming"),

    [string]$ServerInstance = "localhost",

    [string]$Database = "RVToolsDW",

    [switch]$UseSqlAuth,

    [PSCredential]$Credential,

    [ValidateSet('Verbose', 'Info', 'Warning', 'Error')]
    [string]$LogLevel = 'Info',

    [switch]$Force
)

# Import the module
$modulePath = Join-Path $PSScriptRoot "modules/RVToolsImport.psm1"
if (-not (Test-Path $modulePath)) {
    Write-Error "Module not found: $modulePath"
    exit 1
}
Import-Module $modulePath -Force

# Resolve incoming folder path
try {
    $IncomingFolder = Resolve-Path $IncomingFolder -ErrorAction Stop
}
catch {
    Write-Error "Incoming folder not found: $IncomingFolder"
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RVTools Historical Data Import" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Incoming Folder: $IncomingFolder"
Write-Host "Server:          $ServerInstance"
Write-Host "Database:        $Database"
Write-Host ""

# ============================================================================
# Step 1: Scan folder and build hashtable with parsed dates
# ============================================================================
Write-Host "Scanning for xlsx files..." -ForegroundColor Yellow
$files = Get-ChildItem -Path $IncomingFolder -Filter "*.xlsx" |
    Where-Object { $_.Name -notlike '~$*' }

Write-Host "Found $($files.Count) xlsx files"
Write-Host ""

if ($files.Count -eq 0) {
    Write-Host "No xlsx files found in: $IncomingFolder" -ForegroundColor Red
    exit 1
}

$fileTable = @{}
$skippedFiles = @()

foreach ($file in $files) {
    $info = Get-RVToolsExportInfo -FileName $file.Name
    if ($info.Parsed) {
        $fileTable[$file.FullName] = @{
            File       = $file
            ExportDate = $info.ExportDate
            VIServer   = $info.VIServer
        }
    } else {
        $skippedFiles += $file.Name
    }
}

Write-Host "Parsed: $($fileTable.Count) files with valid date patterns" -ForegroundColor Green

if ($skippedFiles.Count -gt 0) {
    Write-Host "Skipped: $($skippedFiles.Count) files (pattern mismatch)" -ForegroundColor Yellow
    foreach ($skipped in $skippedFiles | Select-Object -First 5) {
        Write-Host "  - $skipped" -ForegroundColor Yellow
    }
    if ($skippedFiles.Count -gt 5) {
        Write-Host "  ... and $($skippedFiles.Count - 5) more" -ForegroundColor Yellow
    }
}

if ($fileTable.Count -eq 0) {
    Write-Host ""
    Write-Host "No files match the expected pattern: vCenter{xx}_{d_mm_yyyy}.domain.com.xlsx" -ForegroundColor Red
    Write-Host "Please verify your filenames match this pattern." -ForegroundColor Red
    exit 1
}

# ============================================================================
# Step 2: Sort by ExportDate (oldest first)
# ============================================================================
Write-Host ""
Write-Host "Sorting files by export date (oldest first)..." -ForegroundColor Yellow
$sortedFiles = $fileTable.GetEnumerator() | Sort-Object { $_.Value.ExportDate }

# Show date range
$firstDate = ($sortedFiles | Select-Object -First 1).Value.ExportDate
$lastDate = ($sortedFiles | Select-Object -Last 1).Value.ExportDate
Write-Host "Date range: $($firstDate.ToString('yyyy-MM-dd')) to $($lastDate.ToString('yyyy-MM-dd'))"

# Count files per vCenter
$vCenterCounts = $fileTable.Values | Group-Object VIServer | Sort-Object Name
Write-Host ""
Write-Host "Files per vCenter:" -ForegroundColor Cyan
foreach ($group in $vCenterCounts) {
    Write-Host "  $($group.Name): $($group.Count) files"
}

# ============================================================================
# Step 3: WhatIf mode - just show what would be processed
# ============================================================================
if ($WhatIfPreference) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  WhatIf Mode - Files would be imported in this order:" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""

    $i = 1
    foreach ($entry in $sortedFiles) {
        $info = $entry.Value
        Write-Host "$i. $($info.File.Name)"
        Write-Host "   Date: $($info.ExportDate.ToString('yyyy-MM-dd')), VIServer: $($info.VIServer)" -ForegroundColor Gray
        $i++
    }

    Write-Host ""
    Write-Host "Total: $($sortedFiles.Count) files would be imported" -ForegroundColor Cyan
    Write-Host "Run without -WhatIf to perform the actual import." -ForegroundColor Yellow
    exit 0
}

# ============================================================================
# Step 4: Prompt for confirmation
# ============================================================================
if (-not $Force) {
    Write-Host ""
    Write-Host "Ready to import $($fileTable.Count) files." -ForegroundColor Cyan
    Write-Host "This will update History tables with ValidFrom dates from the filenames." -ForegroundColor Yellow
    Write-Host ""
    $confirm = Read-Host "Continue? (Y/N)"
    if ($confirm -notmatch '^[Yy]') {
        Write-Host "Import cancelled by user." -ForegroundColor Yellow
        exit 0
    }
}

# ============================================================================
# Step 5: Handle credentials
# ============================================================================
$credParams = @{}
if ($UseSqlAuth -or $Credential) {
    if (-not $Credential) {
        $Credential = Get-Credential -Message "Enter SQL Server credentials for $ServerInstance"
    }
    $credParams['Credential'] = $Credential
}

# ============================================================================
# Step 6: Process files in chronological order
# ============================================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Starting Import..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

$importStartTime = Get-Date
$successCount = 0
$failCount = 0
$totalStaged = 0
$i = 1
$total = $sortedFiles.Count

foreach ($entry in $sortedFiles) {
    $filePath = $entry.Key
    $info = $entry.Value

    Write-Host "[$i/$total] $($info.File.Name)" -ForegroundColor Cyan
    Write-Host "         Date: $($info.ExportDate.ToString('yyyy-MM-dd')), VIServer: $($info.VIServer)"

    try {
        $result = Import-RVToolsFile `
            -FilePath $filePath `
            -ServerInstance $ServerInstance `
            -Database $Database `
            -LogLevel $LogLevel `
            -VIServer $info.VIServer `
            -RVToolsExportDate $info.ExportDate `
            @credParams

        Write-Host "         Status: $($result.Status), Staged: $($result.TotalStagedRows), Duration: $($result.DurationSeconds)s" -ForegroundColor Green
        $successCount++
        $totalStaged += $result.TotalStagedRows
    }
    catch {
        Write-Host "         FAILED: $($_.Exception.Message)" -ForegroundColor Red
        $failCount++
    }

    $i++
}

# ============================================================================
# Step 7: Summary
# ============================================================================
$importDuration = (Get-Date) - $importStartTime

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Import Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Duration:     $($importDuration.ToString('hh\:mm\:ss'))"
Write-Host "Files:        $total total"
Write-Host "  Successful: $successCount" -ForegroundColor Green
Write-Host "  Failed:     $failCount" -ForegroundColor $(if ($failCount -gt 0) { 'Red' } else { 'Green' })
Write-Host "Total Staged: $totalStaged rows"
Write-Host ""

if ($failCount -gt 0) {
    Write-Host "Some files failed to import. Check the logs folder for details." -ForegroundColor Yellow
    Write-Host "Failed files are moved to the 'errors' folder." -ForegroundColor Yellow
}

# Return summary object for scripting
[PSCustomObject]@{
    TotalFiles     = $total
    SuccessCount   = $successCount
    FailCount      = $failCount
    TotalStaged    = $totalStaged
    DurationSeconds = [int]$importDuration.TotalSeconds
    DateRange      = @{
        Start = $firstDate
        End   = $lastDate
    }
}
