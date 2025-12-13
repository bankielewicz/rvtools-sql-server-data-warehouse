<#
.SYNOPSIS
    Deploys the RVTools Windows Service with proper permissions.

.DESCRIPTION
    This script:
    1. Prompts for service account credentials
    2. Creates/updates the Windows Service
    3. Grants the IIS AppPool account permission to start/stop the service
    4. Grants the service account necessary file system permissions

.PARAMETER ServiceName
    Name of the Windows Service. Default: RVToolsService

.PARAMETER DisplayName
    Display name for the service. Default: RVTools Import Service

.PARAMETER ServicePath
    Path to the service executable. Default: Current directory\RVToolsService.exe

.PARAMETER AppPoolIdentity
    The IIS Application Pool identity that needs start/stop permissions.
    Default: IIS AppPool\RVToolsWeb

.PARAMETER IncomingFolder
    Path to the incoming folder for RVTools imports.
    Default: C:\RVTools\incoming

.EXAMPLE
    .\Deploy-RVToolsService.ps1

.EXAMPLE
    .\Deploy-RVToolsService.ps1 -AppPoolIdentity "IIS AppPool\MyAppPool" -IncomingFolder "D:\Data\incoming"

.NOTES
    Requires Administrator privileges.
    Run from an elevated PowerShell prompt.
#>

[CmdletBinding()]
param(
    [string]$ServiceName = "RVToolsService",
    [string]$DisplayName = "RVTools Import Service",
    [string]$ServicePath = "",
    [string]$AppPoolIdentity = "IIS AppPool\RVToolsWeb",
    [string]$IncomingFolder = "C:\RVTools\incoming"
)

#Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

# Banner
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RVTools Service Deployment Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Determine service executable path
if ([string]::IsNullOrEmpty($ServicePath)) {
    $ServicePath = Join-Path $PSScriptRoot "RVToolsService.exe"
}

if (-not (Test-Path $ServicePath)) {
    Write-Host "ERROR: Service executable not found at: $ServicePath" -ForegroundColor Red
    Write-Host "Please build the service first or specify -ServicePath parameter." -ForegroundColor Yellow
    exit 1
}

Write-Host "Service Executable: $ServicePath" -ForegroundColor Gray
Write-Host "Service Name: $ServiceName" -ForegroundColor Gray
Write-Host "App Pool Identity: $AppPoolIdentity" -ForegroundColor Gray
Write-Host ""

# Prompt for service account credentials
Write-Host "Enter the credentials for the service account:" -ForegroundColor Yellow
Write-Host "(This account will run the Windows Service)" -ForegroundColor Gray
Write-Host ""

$Credential = Get-Credential -Message "Enter service account credentials (DOMAIN\Username)"

if ($null -eq $Credential) {
    Write-Host "ERROR: No credentials provided. Exiting." -ForegroundColor Red
    exit 1
}

$ServiceAccount = $Credential.UserName
$ServicePassword = $Credential.GetNetworkCredential().Password

Write-Host ""
Write-Host "Service Account: $ServiceAccount" -ForegroundColor Gray
Write-Host ""

# Check if service exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($existingService) {
    Write-Host "Existing service found. Stopping..." -ForegroundColor Yellow

    if ($existingService.Status -eq 'Running') {
        Stop-Service -Name $ServiceName -Force
        Start-Sleep -Seconds 2
    }

    Write-Host "Removing existing service..." -ForegroundColor Yellow
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

# Create the service
Write-Host "Creating Windows Service..." -ForegroundColor Cyan

$binPath = "`"$ServicePath`""

# Create service with sc.exe for more control
$result = sc.exe create $ServiceName binPath= $binPath start= auto obj= $ServiceAccount password= $ServicePassword DisplayName= $DisplayName
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create service. Exit code: $LASTEXITCODE" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    exit 1
}

Write-Host "Service created successfully." -ForegroundColor Green

# Set service description
sc.exe description $ServiceName "Automated RVTools data import service for the RVTools Data Warehouse" | Out-Null

# Configure service recovery options (restart on failure)
Write-Host "Configuring service recovery options..." -ForegroundColor Cyan
sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000 | Out-Null
Write-Host "Recovery: Restart after 60 seconds on failure." -ForegroundColor Gray

# Grant IIS AppPool permission to start/stop the service
Write-Host ""
Write-Host "Configuring service permissions for IIS AppPool..." -ForegroundColor Cyan

# Get the service's security descriptor
$sddl = sc.exe sdshow $ServiceName | Where-Object { $_ -match "^D:" }

if ([string]::IsNullOrEmpty($sddl)) {
    Write-Host "ERROR: Could not retrieve service security descriptor." -ForegroundColor Red
    exit 1
}

Write-Host "Current SDDL: $sddl" -ForegroundColor Gray

# We need to get the SID of the AppPool identity
# For virtual accounts like "IIS AppPool\AppPoolName", we use a well-known pattern
# Format: S-1-5-82-<hash of app pool name>

# Function to get AppPool SID
function Get-AppPoolSid {
    param([string]$AppPoolName)

    # Extract just the pool name from "IIS AppPool\PoolName"
    $poolName = $AppPoolName -replace "^IIS AppPool\\", ""

    # Try to get SID using .NET
    try {
        $ntAccount = New-Object System.Security.Principal.NTAccount($AppPoolName)
        $sid = $ntAccount.Translate([System.Security.Principal.SecurityIdentifier])
        return $sid.Value
    }
    catch {
        Write-Host "Could not resolve AppPool SID directly. Using alternative method..." -ForegroundColor Yellow

        # For IIS AppPool virtual accounts, compute the SID
        # The SID is S-1-5-82 followed by SHA1 hash of lowercase pool name
        $sha1 = [System.Security.Cryptography.SHA1]::Create()
        $bytes = [System.Text.Encoding]::Unicode.GetBytes($poolName.ToLower())
        $hash = $sha1.ComputeHash($bytes)

        # Convert hash to 5 DWORD values for SID
        $d1 = [BitConverter]::ToUInt32($hash, 0)
        $d2 = [BitConverter]::ToUInt32($hash, 4)
        $d3 = [BitConverter]::ToUInt32($hash, 8)
        $d4 = [BitConverter]::ToUInt32($hash, 12)
        $d5 = [BitConverter]::ToUInt32($hash, 16)

        return "S-1-5-82-$d1-$d2-$d3-$d4-$d5"
    }
}

$appPoolSid = Get-AppPoolSid -AppPoolName $AppPoolIdentity
Write-Host "AppPool SID: $appPoolSid" -ForegroundColor Gray

# Build ACE for start/stop permission
# Service permissions:
#   RP = SERVICE_START (0x0010)
#   WP = SERVICE_STOP (0x0020)
#   LC = SERVICE_QUERY_STATUS (0x0004)
# We grant: LC (query status) + RP (start) + WP (stop) = LCRPWP

$ace = "(A;;LCRPWP;;;$appPoolSid)"

# Insert the new ACE into the DACL (D: section), not after the SACL (S: section)
# SDDL format: D:(ace1)(ace2)...S:(sacl)
# We need to insert before the S: section if it exists

if ($sddl -match "^(D:.*?)(S:.*)$") {
    # Has both DACL and SACL
    $dacl = $Matches[1]
    $sacl = $Matches[2]
    $newSddl = $dacl + $ace + $sacl
}
elseif ($sddl -match "^(D:.*)$") {
    # Only DACL, no SACL
    $newSddl = $sddl + $ace
}
else {
    Write-Host "ERROR: Unexpected SDDL format." -ForegroundColor Red
    exit 1
}

Write-Host "New SDDL: $newSddl" -ForegroundColor Gray

# Apply the new security descriptor
$result = sc.exe sdset $ServiceName $newSddl
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to set service permissions. Exit code: $LASTEXITCODE" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    Write-Host ""
    Write-Host "You may need to manually grant permissions using:" -ForegroundColor Yellow
    Write-Host "  subinacl /service $ServiceName /grant=$AppPoolIdentity=STOP+START+QUERY" -ForegroundColor Yellow
    exit 1
}

Write-Host "Service permissions granted successfully." -ForegroundColor Green
Write-Host "  - $AppPoolIdentity can now start/stop the service" -ForegroundColor Gray

# Grant service account permissions to incoming folder
Write-Host ""
Write-Host "Configuring file system permissions..." -ForegroundColor Cyan

$foldersToGrant = @(
    @{ Path = $IncomingFolder; Description = "Incoming folder" },
    @{ Path = (Join-Path (Split-Path $IncomingFolder -Parent) "processed"); Description = "Processed folder" },
    @{ Path = (Join-Path (Split-Path $IncomingFolder -Parent) "errors"); Description = "Errors folder" },
    @{ Path = (Join-Path (Split-Path $IncomingFolder -Parent) "logs"); Description = "Logs folder" }
)

foreach ($folder in $foldersToGrant) {
    $folderPath = $folder.Path
    $description = $folder.Description

    if (-not (Test-Path $folderPath)) {
        Write-Host "Creating $description`: $folderPath" -ForegroundColor Gray
        New-Item -Path $folderPath -ItemType Directory -Force | Out-Null
    }

    Write-Host "Granting permissions on $description`: $folderPath" -ForegroundColor Gray

    try {
        $acl = Get-Acl $folderPath

        # Grant Modify permissions (Read, Write, Delete, but not Full Control)
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            $ServiceAccount,
            "Modify",
            "ContainerInherit,ObjectInherit",
            "None",
            "Allow"
        )

        $acl.AddAccessRule($accessRule)
        Set-Acl -Path $folderPath -AclObject $acl

        Write-Host "  Granted Modify permissions to $ServiceAccount" -ForegroundColor Gray
    }
    catch {
        Write-Host "  WARNING: Could not set permissions on $folderPath`: $_" -ForegroundColor Yellow
    }
}

# Grant service account Log on as a service right
Write-Host ""
Write-Host "Granting 'Log on as a service' right..." -ForegroundColor Cyan

# Use secedit to grant the right
$tempFile = [System.IO.Path]::GetTempFileName()
$tempDb = [System.IO.Path]::GetTempFileName()

try {
    # Export current security policy
    secedit /export /cfg $tempFile /quiet

    # Read the file
    $content = Get-Content $tempFile -Raw

    # Get account SID
    try {
        $ntAccount = New-Object System.Security.Principal.NTAccount($ServiceAccount)
        $accountSid = $ntAccount.Translate([System.Security.Principal.SecurityIdentifier]).Value
    }
    catch {
        Write-Host "  WARNING: Could not resolve account SID. Skipping 'Log on as a service' configuration." -ForegroundColor Yellow
        Write-Host "  Please manually grant this right via Local Security Policy." -ForegroundColor Yellow
        $accountSid = $null
    }

    if ($accountSid) {
        # Check if SeServiceLogonRight exists and add our SID
        if ($content -match "SeServiceLogonRight\s*=\s*(.*)") {
            $currentValue = $Matches[1]
            if ($currentValue -notmatch [regex]::Escape($accountSid)) {
                $newValue = "$currentValue,*$accountSid"
                $content = $content -replace "SeServiceLogonRight\s*=\s*.*", "SeServiceLogonRight = $newValue"
            }
        }
        else {
            # Add the setting if it doesn't exist
            $content = $content -replace "(\[Privilege Rights\])", "`$1`r`nSeServiceLogonRight = *$accountSid"
        }

        # Write modified content
        Set-Content -Path $tempFile -Value $content -Force

        # Import the modified policy
        secedit /configure /db $tempDb /cfg $tempFile /quiet

        Write-Host "  'Log on as a service' right granted to $ServiceAccount" -ForegroundColor Gray
    }
}
catch {
    Write-Host "  WARNING: Could not configure 'Log on as a service' right: $_" -ForegroundColor Yellow
    Write-Host "  Please manually grant this right via Local Security Policy." -ForegroundColor Yellow
}
finally {
    # Cleanup temp files
    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    Remove-Item $tempDb -Force -ErrorAction SilentlyContinue
    Remove-Item "$tempDb.log" -Force -ErrorAction SilentlyContinue
    Remove-Item "$tempDb.jfm" -Force -ErrorAction SilentlyContinue
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Deployment Complete" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Service Details:" -ForegroundColor Cyan
Write-Host "  Name: $ServiceName" -ForegroundColor White
Write-Host "  Account: $ServiceAccount" -ForegroundColor White
Write-Host "  Executable: $ServicePath" -ForegroundColor White
Write-Host "  Startup: Automatic" -ForegroundColor White
Write-Host ""
Write-Host "Permissions Granted:" -ForegroundColor Cyan
Write-Host "  - $AppPoolIdentity can start/stop the service" -ForegroundColor White
Write-Host "  - $ServiceAccount has Modify access to data folders" -ForegroundColor White
Write-Host "  - $ServiceAccount has 'Log on as a service' right" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Configure appsettings.json with database connection" -ForegroundColor White
Write-Host "  2. Start the service: Start-Service $ServiceName" -ForegroundColor White
Write-Host "  3. Check logs at: $IncomingFolder\..\logs" -ForegroundColor White
Write-Host ""

# Offer to start the service
$startNow = Read-Host "Start the service now? (Y/N)"
if ($startNow -eq 'Y' -or $startNow -eq 'y') {
    Write-Host "Starting service..." -ForegroundColor Cyan
    Start-Service -Name $ServiceName
    Start-Sleep -Seconds 2

    $svc = Get-Service -Name $ServiceName
    if ($svc.Status -eq 'Running') {
        Write-Host "Service started successfully." -ForegroundColor Green
    }
    else {
        Write-Host "Service status: $($svc.Status)" -ForegroundColor Yellow
        Write-Host "Check Event Viewer for details." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Done." -ForegroundColor Gray
