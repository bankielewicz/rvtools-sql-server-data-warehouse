<#
.SYNOPSIS
    RVTools Import Module for SQL Server Data Warehouse

.DESCRIPTION
    PowerShell module for importing RVTools xlsx exports into SQL Server.
    Implements logging, validation, and error handling.

.NOTES
    Requires: ImportExcel, SqlServer modules
    Install-Module ImportExcel -Scope CurrentUser
    Install-Module SqlServer -Scope CurrentUser
#>

#Requires -Version 5.1

# ============================================================================
# Module Variables
# ============================================================================
$Script:LogLevel = 'Info'
$Script:LogFile = $null
$Script:Connection = $null

# Log level hierarchy
$Script:LogLevels = @{
    'Verbose' = 0
    'Info'    = 1
    'Warning' = 2
    'Error'   = 3
}

# ============================================================================
# Logging Functions
# ============================================================================

function Write-ImportLog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Message,

        [ValidateSet('Verbose', 'Info', 'Warning', 'Error')]
        [string]$Level = 'Info',

        [string]$SheetName = $null
    )

    # Check if we should log this level
    if ($Script:LogLevels[$Level] -lt $Script:LogLevels[$Script:LogLevel]) {
        return
    }

    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss.fff'
    $prefix = if ($SheetName) { "[$SheetName] " } else { "" }
    $logEntry = "$timestamp [$Level] $prefix$Message"

    # Write to console with color
    $color = switch ($Level) {
        'Verbose' { 'Gray' }
        'Info'    { 'White' }
        'Warning' { 'Yellow' }
        'Error'   { 'Red' }
    }
    Write-Host $logEntry -ForegroundColor $color

    # Write to log file if configured
    if ($Script:LogFile) {
        Add-Content -Path $Script:LogFile -Value $logEntry -Encoding UTF8
    }
}

function Initialize-ImportLog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$LogDirectory,

        [ValidateSet('Verbose', 'Info', 'Warning', 'Error')]
        [string]$LogLevel = 'Info'
    )

    $Script:LogLevel = $LogLevel

    # Create log directory if needed
    if (-not (Test-Path $LogDirectory)) {
        New-Item -ItemType Directory -Path $LogDirectory -Force | Out-Null
    }

    # Create log file with timestamp
    $timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
    $Script:LogFile = Join-Path $LogDirectory "RVToolsImport_$timestamp.log"

    Write-ImportLog -Message "Log initialized: $($Script:LogFile)" -Level 'Info'

    return $Script:LogFile
}

# ============================================================================
# Database Connection Functions
# ============================================================================

function Connect-RVToolsDatabase {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ServerInstance,

        [string]$Database = 'RVToolsDW',

        [PSCredential]$Credential = $null
    )

    try {
        $connectionParams = @{
            ServerInstance = $ServerInstance
            Database       = $Database
            TrustServerCertificate = $true
        }

        if ($Credential) {
            $connectionParams['Credential'] = $Credential
            Write-ImportLog -Message "Connecting to $ServerInstance/$Database with SQL authentication" -Level 'Info'
        } else {
            Write-ImportLog -Message "Connecting to $ServerInstance/$Database with Windows authentication" -Level 'Info'
        }

        # Test connection
        $testQuery = "SELECT 1 AS Test"
        $result = Invoke-Sqlcmd @connectionParams -Query $testQuery -ErrorAction Stop

        $Script:ConnectionParams = $connectionParams
        Write-ImportLog -Message "Database connection successful" -Level 'Info'
    }
    catch {
        Write-ImportLog -Message "Database connection failed: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

# ============================================================================
# Import Batch Functions
# ============================================================================

function New-ImportBatch {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$SourceFile,

        [string]$VIServer = $null
    )

    # Escape single quotes for SQL
    $escapedSourceFile = $SourceFile -replace "'", "''"
    $escapedVIServer = if ($VIServer) { "'" + ($VIServer -replace "'", "''") + "'" } else { "NULL" }

    $query = @"
INSERT INTO [Audit].[ImportBatch] (SourceFile, VIServer, ImportStartTime, Status)
OUTPUT INSERTED.ImportBatchId
VALUES (N'$escapedSourceFile', $escapedVIServer, GETUTCDATE(), 'Running')
"@

    try {
        $result = Invoke-Sqlcmd @Script:ConnectionParams -Query $query -ErrorAction Stop

        $batchId = $result.ImportBatchId
        Write-ImportLog -Message "Created import batch: $batchId" -Level 'Info'

        return $batchId
    }
    catch {
        Write-ImportLog -Message "Failed to create import batch: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

function Update-ImportBatch {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [int]$ImportBatchId,

        [string]$Status,

        [int]$TotalSheets = 0,
        [int]$SheetsProcessed = 0,
        [int]$TotalRowsSource = 0,
        [int]$TotalRowsStaged = 0,
        [int]$TotalRowsFailed = 0,

        [string]$ErrorMessage = $null
    )

    $query = @"
UPDATE [Audit].[ImportBatch]
SET ImportEndTime = GETUTCDATE(),
    Status = '$Status',
    TotalSheets = $TotalSheets,
    SheetsProcessed = $SheetsProcessed,
    TotalRowsSource = $TotalRowsSource,
    TotalRowsStaged = $TotalRowsStaged,
    TotalRowsFailed = $TotalRowsFailed,
    ErrorMessage = $(if ($ErrorMessage) { "'$($ErrorMessage -replace "'", "''")'" } else { 'NULL' })
WHERE ImportBatchId = $ImportBatchId
"@

    try {
        Invoke-Sqlcmd @Script:ConnectionParams -Query $query -ErrorAction Stop
        Write-ImportLog -Message "Updated import batch $ImportBatchId to status: $Status" -Level 'Verbose'
    }
    catch {
        Write-ImportLog -Message "Failed to update import batch: $($_.Exception.Message)" -Level 'Warning'
    }
}

# ============================================================================
# Staging Functions
# ============================================================================

function Clear-StagingTable {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$TableName
    )

    $query = "TRUNCATE TABLE [Staging].[$TableName]"

    try {
        Invoke-Sqlcmd @Script:ConnectionParams -Query $query -ErrorAction Stop
        Write-ImportLog -Message "Cleared staging table: $TableName" -Level 'Verbose'
    }
    catch {
        Write-ImportLog -Message "Failed to clear staging table $TableName : $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

function Import-SheetToStaging {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [Parameter(Mandatory)]
        [string]$SheetName,

        [Parameter(Mandatory)]
        [int]$ImportBatchId
    )

    $startTime = Get-Date

    try {
        # Read Excel sheet without headers to handle duplicates
        Write-ImportLog -Message "Reading sheet: $SheetName" -Level 'Verbose' -SheetName $SheetName
        $rawData = Import-Excel -Path $FilePath -WorksheetName $SheetName -NoHeader -ErrorAction Stop

        if ($rawData.Count -le 1) {
            Write-ImportLog -Message "Sheet is empty, skipping" -Level 'Warning' -SheetName $SheetName
            return @{
                SheetName      = $SheetName
                SourceRows     = 0
                StagedRows     = 0
                FailedRows     = 0
                DurationMs     = 0
            }
        }

        # Extract headers from first row and make duplicates unique
        $headerRow = $rawData[0]
        $originalHeaders = @()
        foreach ($prop in $headerRow.PSObject.Properties) {
            $originalHeaders += $prop.Value
        }

        # Make duplicate headers unique by appending _2, _3, etc.
        $uniqueHeaders = @()
        $headerCount = @{}
        foreach ($header in $originalHeaders) {
            $headerStr = if ($header) { $header.ToString().Trim() } else { "Column" }
            if ($headerStr -eq '') { $headerStr = "Column" }
            if ($headerCount.ContainsKey($headerStr)) {
                $headerCount[$headerStr]++
                $uniqueHeaders += "${headerStr}_$($headerCount[$headerStr])"
            } else {
                $headerCount[$headerStr] = 1
                $uniqueHeaders += $headerStr
            }
        }

        # Convert remaining rows to objects with unique headers
        $data = [System.Collections.ArrayList]@()
        for ($i = 1; $i -lt $rawData.Count; $i++) {
            $row = $rawData[$i]
            $obj = [ordered]@{}
            $colIndex = 0
            foreach ($prop in $row.PSObject.Properties) {
                if ($colIndex -lt $uniqueHeaders.Count) {
                    $obj[$uniqueHeaders[$colIndex]] = $prop.Value
                }
                $colIndex++
            }
            [void]$data.Add([PSCustomObject]$obj)
        }

        $sourceRowCount = $data.Count
        Write-ImportLog -Message "Source rows: $sourceRowCount" -Level 'Info' -SheetName $SheetName

        if ($sourceRowCount -eq 0) {
            Write-ImportLog -Message "Sheet is empty, skipping" -Level 'Warning' -SheetName $SheetName
            return @{
                SheetName      = $SheetName
                SourceRows     = 0
                StagedRows     = 0
                FailedRows     = 0
                DurationMs     = 0
            }
        }

        # Clear staging table
        $stagingTableName = $SheetName -replace '[^a-zA-Z0-9_]', '_'
        Clear-StagingTable -TableName $stagingTableName

        # Get staging table columns
        $columnsQuery = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'Staging' AND TABLE_NAME = '$stagingTableName'
AND COLUMN_NAME NOT IN ('StagingId', 'ImportBatchId', 'ImportRowNum')
ORDER BY ORDINAL_POSITION
"@
        $stagingColumns = (Invoke-Sqlcmd @Script:ConnectionParams -Query $columnsQuery).COLUMN_NAME

        # Build column mapping (Excel column -> Staging column)
        $excelColumns = $data[0].PSObject.Properties.Name
        $columnMap = @{}

        foreach ($excelCol in $excelColumns) {
            # Sanitize column name to match staging table
            $sanitized = $excelCol -replace '[^a-zA-Z0-9_]', '_' -replace '__+', '_'
            $sanitized = $sanitized.TrimStart('_').TrimEnd('_')

            # Handle special cases
            $sanitized = $sanitized -replace '^#_', 'Num_'
            $sanitized = $sanitized -replace '_#$', '_Num'

            if ($stagingColumns -contains $sanitized) {
                $columnMap[$excelCol] = $sanitized
            }
        }

        # Bulk insert data
        $rowNum = 0
        $stagedCount = 0
        $failedCount = 0
        $batchSize = 1000
        $batch = @()

        foreach ($row in $data) {
            $rowNum++

            try {
                # Build insert values
                $values = @{
                    ImportBatchId = $ImportBatchId
                    ImportRowNum  = $rowNum
                }

                foreach ($excelCol in $columnMap.Keys) {
                    $stagingCol = $columnMap[$excelCol]
                    $value = $row.$excelCol

                    if ($null -eq $value -or $value -eq '') {
                        $values[$stagingCol] = $null
                    } else {
                        $values[$stagingCol] = $value.ToString()
                    }
                }

                $batch += $values
                $stagedCount++

                # Execute batch insert
                if ($batch.Count -ge $batchSize) {
                    Insert-StagingBatch -TableName $stagingTableName -Batch $batch
                    $batch = @()

                    if ($Script:LogLevel -eq 'Verbose') {
                        Write-ImportLog -Message "Staged $stagedCount rows..." -Level 'Verbose' -SheetName $SheetName
                    }
                }
            }
            catch {
                $failedCount++
                Write-ImportLog -Message "Row $rowNum failed: $($_.Exception.Message)" -Level 'Warning' -SheetName $SheetName
            }
        }

        # Insert remaining batch
        if ($batch.Count -gt 0) {
            Insert-StagingBatch -TableName $stagingTableName -Batch $batch
        }

        $duration = ((Get-Date) - $startTime).TotalMilliseconds

        Write-ImportLog -Message "Completed: $stagedCount staged, $failedCount failed ($([int]$duration)ms)" -Level 'Info' -SheetName $SheetName

        return @{
            SheetName      = $SheetName
            SourceRows     = $sourceRowCount
            StagedRows     = $stagedCount
            FailedRows     = $failedCount
            DurationMs     = [int]$duration
        }
    }
    catch {
        Write-ImportLog -Message "Failed to import sheet: $($_.Exception.Message)" -Level 'Error' -SheetName $SheetName
        throw
    }
}

function Insert-StagingBatch {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$TableName,

        [Parameter(Mandatory)]
        [array]$Batch
    )

    if ($Batch.Count -eq 0) { return }

    # Build bulk insert
    $columns = $Batch[0].Keys | Where-Object { $_ -ne $null }
    $columnList = ($columns | ForEach-Object { "[$_]" }) -join ', '

    $valueRows = @()
    foreach ($row in $Batch) {
        $values = @()
        foreach ($col in $columns) {
            $val = $row[$col]
            if ($null -eq $val) {
                $values += 'NULL'
            } else {
                $escaped = $val -replace "'", "''"
                $values += "N'$escaped'"
            }
        }
        $valueRows += "(" + ($values -join ', ') + ")"
    }

    $query = "INSERT INTO [Staging].[$TableName] ($columnList) VALUES " + ($valueRows -join ",`n")

    try {
        Invoke-Sqlcmd @Script:ConnectionParams -Query $query -QueryTimeout 300 -ErrorAction Stop
    }
    catch {
        Write-ImportLog -Message "Batch insert failed: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

# ============================================================================
# Main Import Function
# ============================================================================

function Import-RVToolsFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [Parameter(Mandatory)]
        [string]$ServerInstance,

        [string]$Database = 'RVToolsDW',

        [PSCredential]$Credential = $null,

        [ValidateSet('Verbose', 'Info', 'Warning', 'Error')]
        [string]$LogLevel = 'Info',

        [string]$ProcessedFolder = $null,
        [string]$ErrorFolder = $null,
        [string]$FailedFolder = $null,
        [string]$LogFolder = $null
    )

    $startTime = Get-Date
    $fileName = Split-Path $FilePath -Leaf
    $basePath = Split-Path $FilePath -Parent

    # Set default folders relative to incoming
    if (-not $ProcessedFolder) { $ProcessedFolder = Join-Path (Split-Path $basePath) 'processed' }
    if (-not $ErrorFolder) { $ErrorFolder = Join-Path (Split-Path $basePath) 'errors' }
    if (-not $FailedFolder) { $FailedFolder = Join-Path (Split-Path $basePath) 'failed' }
    if (-not $LogFolder) { $LogFolder = Join-Path (Split-Path $basePath) 'logs' }

    # Initialize logging
    $logFile = Initialize-ImportLog -LogDirectory $LogFolder -LogLevel $LogLevel
    Write-ImportLog -Message "Starting import of: $fileName" -Level 'Info'

    try {
        # Validate file exists
        if (-not (Test-Path $FilePath)) {
            throw "File not found: $FilePath"
        }

        # Connect to database
        Connect-RVToolsDatabase -ServerInstance $ServerInstance -Database $Database -Credential $Credential

        # Create import batch
        $batchId = New-ImportBatch -SourceFile $fileName

        # Get sheet names from Excel file
        $excelPackage = Open-ExcelPackage -Path $FilePath
        $sheetNames = $excelPackage.Workbook.Worksheets.Name
        Close-ExcelPackage $excelPackage -NoSave | Out-Null

        Write-ImportLog -Message "Found $($sheetNames.Count) sheets: $($sheetNames -join ', ')" -Level 'Info'

        # Define processing order (important sheets first)
        $orderedSheets = @(
            'vInfo', 'vCPU', 'vMemory', 'vDisk', 'vPartition', 'vNetwork',
            'vSnapshot', 'vTools', 'vHost', 'vCluster', 'vDatastore', 'vHealth',
            'vCD', 'vUSB', 'vSource', 'vRP', 'vHBA', 'vNIC', 'vSwitch', 'vPort',
            'dvSwitch', 'dvPort', 'vSC_VMK', 'vMultiPath', 'vLicense', 'vFileInfo', 'vMetaData'
        )

        $results = @()
        $totalSourceRows = 0
        $totalStagedRows = 0
        $totalFailedRows = 0

        foreach ($sheetName in $orderedSheets) {
            if ($sheetNames -contains $sheetName) {
                try {
                    $result = Import-SheetToStaging -FilePath $FilePath -SheetName $sheetName -ImportBatchId $batchId
                    $results += $result
                    $totalSourceRows += $result.SourceRows
                    $totalStagedRows += $result.StagedRows
                    $totalFailedRows += $result.FailedRows
                }
                catch {
                    Write-ImportLog -Message "Sheet $sheetName failed: $($_.Exception.Message)" -Level 'Error'
                    $totalFailedRows++
                }
            }
        }

        # ================================================================
        # Process staged data through stored procedure
        # ================================================================
        if ($totalStagedRows -gt 0) {
            Write-ImportLog -Message "Processing staged data to Current/History tables..." -Level 'Info'

            $mergeQuery = @"
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
EXEC dbo.usp_ProcessImport @ImportBatchId = $batchId, @SourceFile = N'$($fileName -replace "'", "''")'
"@

            try {
                $mergeResult = Invoke-Sqlcmd @Script:ConnectionParams -Query $mergeQuery -QueryTimeout 600 -ErrorAction Stop

                if ($mergeResult) {
                    $mergedCount = $mergeResult.TotalRowsMerged
                    $mergeStatus = $mergeResult.Status
                    Write-ImportLog -Message "Merge complete: Status=$mergeStatus, RowsMerged=$mergedCount" -Level 'Info'
                }
            }
            catch {
                Write-ImportLog -Message "Merge failed: $($_.Exception.Message)" -Level 'Warning'
                # Don't fail the import - staging data is preserved for retry
            }
        }

        # Determine final status
        $failurePercent = if ($totalSourceRows -gt 0) { ($totalFailedRows / $totalSourceRows) * 100 } else { 0 }
        $status = if ($totalFailedRows -eq 0) { 'Success' }
                  elseif ($failurePercent -lt 50) { 'Partial' }
                  else { 'Failed' }

        # Update batch
        Update-ImportBatch -ImportBatchId $batchId `
            -Status $status `
            -TotalSheets $sheetNames.Count `
            -SheetsProcessed $results.Count `
            -TotalRowsSource $totalSourceRows `
            -TotalRowsStaged $totalStagedRows `
            -TotalRowsFailed $totalFailedRows

        # Move file based on status
        $dateSuffix = Get-Date -Format 'yyyyMMdd'
        $newFileName = [System.IO.Path]::GetFileNameWithoutExtension($fileName) + ".$dateSuffix" + [System.IO.Path]::GetExtension($fileName)

        $destinationFolder = switch ($status) {
            'Success' { $ProcessedFolder }
            'Partial' { $ProcessedFolder }
            'Failed'  { $ErrorFolder }
        }

        if (-not (Test-Path $destinationFolder)) {
            New-Item -ItemType Directory -Path $destinationFolder -Force | Out-Null
        }

        $destinationPath = Join-Path $destinationFolder $newFileName
        Move-Item -Path $FilePath -Destination $destinationPath -Force

        $duration = ((Get-Date) - $startTime).TotalSeconds
        Write-ImportLog -Message "Import complete: Status=$status, Duration=$([int]$duration)s, Staged=$totalStagedRows, Failed=$totalFailedRows" -Level 'Info'
        Write-ImportLog -Message "File moved to: $destinationPath" -Level 'Info'

        return [PSCustomObject]@{
            ImportBatchId   = $batchId
            Status          = $status
            FileName        = $fileName
            SheetsProcessed = $results.Count
            TotalSourceRows = $totalSourceRows
            TotalStagedRows = $totalStagedRows
            TotalFailedRows = $totalFailedRows
            DurationSeconds = [int]$duration
            LogFile         = $logFile
            DestinationPath = $destinationPath
        }
    }
    catch {
        Write-ImportLog -Message "Import failed: $($_.Exception.Message)" -Level 'Error'

        # Move to errors folder
        if (-not (Test-Path $ErrorFolder)) {
            New-Item -ItemType Directory -Path $ErrorFolder -Force | Out-Null
        }

        $dateSuffix = Get-Date -Format 'yyyyMMdd'
        $newFileName = [System.IO.Path]::GetFileNameWithoutExtension($fileName) + ".$dateSuffix" + [System.IO.Path]::GetExtension($fileName)
        $errorPath = Join-Path $ErrorFolder $newFileName

        if (Test-Path $FilePath) {
            Move-Item -Path $FilePath -Destination $errorPath -Force
        }

        throw
    }
}

# ============================================================================
# Exported Functions
# ============================================================================

Export-ModuleMember -Function @(
    'Import-RVToolsFile',
    'Connect-RVToolsDatabase',
    'Write-ImportLog',
    'Initialize-ImportLog'
)
