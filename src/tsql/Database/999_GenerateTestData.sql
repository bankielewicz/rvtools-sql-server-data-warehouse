/*
    RVTools Data Warehouse - Generate Test Data for Screenshots
    Purpose: Create realistic fake VMware data for documentation screenshots

    WARNING: This script generates fake data for testing/documentation only.
    DO NOT run in production environments.

    Usage: Execute against RVToolsDW database
*/

USE [RVToolsDW]
GO

PRINT 'Generating test data for RVTools Data Warehouse...'
GO

-- ============================================================================
-- Helper Functions
-- ============================================================================

-- Generate random string
CREATE OR ALTER FUNCTION dbo.fn_RandomString(@length INT)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @chars NVARCHAR(62) = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    DECLARE @result NVARCHAR(MAX) = '';
    DECLARE @i INT = 1;

    WHILE @i <= @length
    BEGIN
        SET @result = @result + SUBSTRING(@chars, CAST(RAND() * 62 AS INT) + 1, 1);
        SET @i = @i + 1;
    END

    RETURN @result;
END
GO

-- ============================================================================
-- Generate Test Data
-- ============================================================================

-- Variables
DECLARE @vCenter NVARCHAR(100) = 'vcenter01.company.com';
DECLARE @Datacenter NVARCHAR(100);
DECLARE @Cluster NVARCHAR(100);
DECLARE @Host NVARCHAR(100);
DECLARE @Datastore NVARCHAR(100);
DECLARE @VM NVARCHAR(100);
DECLARE @i INT;
DECLARE @j INT;
DECLARE @k INT;

-- ============================================================================
-- 1. Generate Hosts (20 hosts across 2 datacenters, 4 clusters)
-- ============================================================================

PRINT 'Generating hosts...';

SET @i = 1;
WHILE @i <= 20
BEGIN
    SET @Datacenter = CASE WHEN @i <= 10 THEN 'DC-East' ELSE 'DC-West' END;
    SET @Cluster = CASE
        WHEN @i <= 5 THEN 'Cluster-Prod-01'
        WHEN @i <= 10 THEN 'Cluster-Prod-02'
        WHEN @i <= 15 THEN 'Cluster-Dev-01'
        ELSE 'Cluster-Dev-02'
    END;

    SET @Host = 'esxi' + RIGHT('00' + CAST(@i AS NVARCHAR), 2) + '.company.com';

    INSERT INTO [Current].[vHost] (
        Host, Datacenter, Cluster, VI_SDK_Server,
        [# CPU], Cores, [# Memory], [Max EVC mode],
        Manufacturer, Model, [CPU Model], Speed,
        [# VMs], Version, Build
    )
    VALUES (
        @Host, @Datacenter, @Cluster, @vCenter,
        2, -- CPUs
        16 + (@i % 8) * 4, -- Cores (16-44)
        256 + (@i % 4) * 128, -- Memory GB (256-640)
        'intel-skylake',
        'Dell Inc.', 'PowerEdge R640', 'Intel(R) Xeon(R) Gold 6130',
        2300 + (@i % 10) * 100, -- MHz
        10 + (@i % 5) * 5, -- VMs per host
        '8.0.2', '21203435'
    );

    SET @i = @i + 1;
END

PRINT 'Generated 20 hosts';
GO

-- ============================================================================
-- 2. Generate Datastores (10 datastores)
-- ============================================================================

PRINT 'Generating datastores...';

DECLARE @dsNames TABLE (Name NVARCHAR(100), CapacityMB BIGINT, Type NVARCHAR(20));
INSERT INTO @dsNames VALUES
('DS-Prod-SSD-01', 10240000, 'VMFS'),
('DS-Prod-SSD-02', 10240000, 'VMFS'),
('DS-Prod-NVMe-01', 20480000, 'VMFS'),
('DS-Dev-SAS-01', 5120000, 'VMFS'),
('DS-Dev-SAS-02', 5120000, 'VMFS'),
('DS-Backup-01', 51200000, 'NFS'),
('DS-ISO-01', 512000, 'NFS'),
('DS-Prod-vSAN-01', 40960000, 'vsan'),
('DS-Dev-vSAN-01', 20480000, 'vsan'),
('DS-Archive-01', 102400000, 'NFS');

DECLARE @dsName NVARCHAR(100), @dsCapacity BIGINT, @dsType NVARCHAR(20);
DECLARE ds_cursor CURSOR FOR SELECT Name, CapacityMB, Type FROM @dsNames;
OPEN ds_cursor;
FETCH NEXT FROM ds_cursor INTO @dsName, @dsCapacity, @dsType;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @usedPercent FLOAT = 0.4 + (RAND() * 0.4); -- 40-80% used
    DECLARE @freeMB BIGINT = CAST(@dsCapacity * (1 - @usedPercent) AS BIGINT);

    INSERT INTO [Current].[vDatastore] (
        Name, Datacenter, Type, VI_SDK_Server,
        [Capacity MB], [Free MB], [Provisioned MB],
        [# VMs], [# Hosts]
    )
    VALUES (
        @dsName,
        CASE WHEN @dsName LIKE '%Prod%' THEN 'DC-East' ELSE 'DC-West' END,
        @dsType,
        @vCenter,
        @dsCapacity,
        @freeMB,
        @dsCapacity - @freeMB + CAST(@dsCapacity * 0.2 * RAND() AS BIGINT), -- Some overprovisioning
        15 + CAST(RAND() * 20 AS INT), -- VMs
        2 + CAST(RAND() * 4 AS INT) -- Hosts
    );

    FETCH NEXT FROM ds_cursor INTO @dsName, @dsCapacity, @dsType;
END

CLOSE ds_cursor;
DEALLOCATE ds_cursor;

PRINT 'Generated 10 datastores';
GO

-- ============================================================================
-- 3. Generate Clusters (4 clusters)
-- ============================================================================

PRINT 'Generating clusters...';

INSERT INTO [Current].[vCluster] (
    Name, Datacenter, VI_SDK_Server,
    [# Hosts], [# CPU], [Total CPU], [# Memory], [Total Memory],
    [# VMs], DRS, HA, EVC
)
VALUES
('Cluster-Prod-01', 'DC-East', 'vcenter01.company.com', 5, 10, 160, 5, 1280, 75, 'FullyAutomated', 'Enabled', 'intel-skylake'),
('Cluster-Prod-02', 'DC-East', 'vcenter01.company.com', 5, 10, 160, 5, 1280, 70, 'FullyAutomated', 'Enabled', 'intel-skylake'),
('Cluster-Dev-01', 'DC-West', 'vcenter01.company.com', 5, 10, 160, 5, 1280, 40, 'PartiallyAutomated', 'Enabled', 'intel-haswell'),
('Cluster-Dev-02', 'DC-West', 'vcenter01.company.com', 5, 10, 160, 5, 1280, 35, 'PartiallyAutomated', 'Disabled', 'intel-haswell');

PRINT 'Generated 4 clusters';
GO

-- ============================================================================
-- 4. Generate VMs (300 VMs)
-- ============================================================================

PRINT 'Generating VMs (this may take a minute)...';

DECLARE @vmNumber INT = 1;
DECLARE @powerStates TABLE (State NVARCHAR(20), Probability FLOAT);
INSERT INTO @powerStates VALUES ('poweredOn', 0.75), ('poweredOff', 0.20), ('suspended', 0.05);

DECLARE @osTypes TABLE (OS NVARCHAR(100));
INSERT INTO @osTypes VALUES
('Microsoft Windows Server 2019 (64-bit)'),
('Microsoft Windows Server 2022 (64-bit)'),
('Microsoft Windows Server 2016 (64-bit)'),
('Ubuntu Linux (64-bit)'),
('Red Hat Enterprise Linux 8 (64-bit)'),
('CentOS 7 (64-bit)'),
('Microsoft Windows 10 (64-bit)'),
('Microsoft Windows 11 (64-bit)');

WHILE @vmNumber <= 300
BEGIN
    -- Determine environment
    DECLARE @env NVARCHAR(10) = CASE WHEN @vmNumber <= 200 THEN 'prod' ELSE 'dev' END;

    -- Assign to cluster/datacenter
    SET @Datacenter = CASE WHEN @vmNumber <= 150 THEN 'DC-East' ELSE 'DC-West' END;
    SET @Cluster = CASE
        WHEN @vmNumber <= 75 THEN 'Cluster-Prod-01'
        WHEN @vmNumber <= 150 THEN 'Cluster-Prod-02'
        WHEN @vmNumber <= 225 THEN 'Cluster-Dev-01'
        ELSE 'Cluster-Dev-02'
    END;

    -- Random host in range
    DECLARE @hostNum INT = ((@vmNumber - 1) % 20) + 1;
    SET @Host = 'esxi' + RIGHT('00' + CAST(@hostNum AS NVARCHAR), 2) + '.company.com';

    -- Random datastore
    DECLARE @dsNum INT = (@vmNumber % 10) + 1;
    SET @Datastore = CASE @dsNum
        WHEN 1 THEN 'DS-Prod-SSD-01'
        WHEN 2 THEN 'DS-Prod-SSD-02'
        WHEN 3 THEN 'DS-Prod-NVMe-01'
        WHEN 4 THEN 'DS-Dev-SAS-01'
        WHEN 5 THEN 'DS-Dev-SAS-02'
        WHEN 6 THEN 'DS-Backup-01'
        WHEN 7 THEN 'DS-Prod-vSAN-01'
        WHEN 8 THEN 'DS-Dev-vSAN-01'
        WHEN 9 THEN 'DS-ISO-01'
        ELSE 'DS-Archive-01'
    END;

    -- Random OS
    DECLARE @OS NVARCHAR(100);
    SELECT TOP 1 @OS = OS FROM @osTypes ORDER BY NEWID();

    -- Power state (75% on, 20% off, 5% suspended)
    DECLARE @rand FLOAT = RAND();
    DECLARE @PowerState NVARCHAR(20) = CASE
        WHEN @rand < 0.75 THEN 'poweredOn'
        WHEN @rand < 0.95 THEN 'poweredOff'
        ELSE 'suspended'
    END;

    -- VM sizing
    DECLARE @CPUs INT = CASE
        WHEN @vmNumber % 10 = 0 THEN 8 -- 10% large
        WHEN @vmNumber % 5 = 0 THEN 4  -- 20% medium
        ELSE 2 -- 70% small
    END;

    DECLARE @MemoryMB INT = @CPUs * 4096; -- 4GB per vCPU
    DECLARE @DiskMB BIGINT = CASE
        WHEN @OS LIKE '%Windows%' THEN 102400 + (RAND() * 409600) -- 100-500GB
        WHEN @OS LIKE '%Linux%' THEN 51200 + (RAND() * 204800) -- 50-250GB
        ELSE 40960
    END;

    SET @VM = @env + '-vm-' + RIGHT('000' + CAST(@vmNumber AS NVARCHAR), 3);

    INSERT INTO [Current].[vInfo] (
        VM, Powerstate, Template, Host, Datacenter, Cluster,
        VI_SDK_Server, DNS_Name, OS, CPUs, Memory, NICs, Disks,
        [Primary IP Address], [Provisioned MB], [In Use MB],
        [HW version], [VM UUID], Annotation, Folder
    )
    VALUES (
        @VM,
        @PowerState,
        0, -- Not a template
        @Host,
        @Datacenter,
        @Cluster,
        @vCenter,
        @VM + '.company.com',
        @OS,
        @CPUs,
        @MemoryMB,
        1, -- NICs
        CASE WHEN @CPUs > 4 THEN 3 ELSE 1 END, -- Disks
        '10.0.' + CAST((@vmNumber / 256) AS NVARCHAR) + '.' + CAST((@vmNumber % 256) AS NVARCHAR),
        @DiskMB,
        CAST(@DiskMB * (0.3 + RAND() * 0.5) AS BIGINT), -- 30-80% disk usage
        'vmx-19',
        NEWID(),
        CASE WHEN @vmNumber % 20 = 0 THEN 'Application: Web Server' ELSE NULL END,
        CASE WHEN @env = 'prod' THEN 'Production' ELSE 'Development' END
    );

    SET @vmNumber = @vmNumber + 1;
END

PRINT 'Generated 300 VMs';
GO

-- ============================================================================
-- 5. Generate vCPU data (for each VM)
-- ============================================================================

PRINT 'Generating vCPU data...';

INSERT INTO [Current].[vCPU] (
    VM, Powerstate, Host, Datacenter, Cluster, VI_SDK_Server,
    CPUs, Sockets, [Cores p/s], Reservation, Limit, Shares
)
SELECT
    VM, Powerstate, Host, Datacenter, Cluster, VI_SDK_Server,
    CPUs,
    CASE WHEN CPUs = 2 THEN 1 WHEN CPUs = 4 THEN 2 ELSE 4 END, -- Sockets
    CASE WHEN CPUs = 2 THEN 2 WHEN CPUs = 4 THEN 2 ELSE 2 END, -- Cores per socket
    0, -- Reservation
    -1, -- Unlimited
    'Normal' -- Shares
FROM [Current].[vInfo];

PRINT 'Generated vCPU data for all VMs';
GO

-- ============================================================================
-- 6. Generate vMemory data (for each VM)
-- ============================================================================

PRINT 'Generating vMemory data...';

INSERT INTO [Current].[vMemory] (
    VM, Powerstate, Host, Datacenter, Cluster, VI_SDK_Server,
    [Size MiB], Consumed, Active, Ballooned, Reservation, Limit, Shares
)
SELECT
    VM, Powerstate, Host, Datacenter, Cluster, VI_SDK_Server,
    Memory,
    CASE WHEN Powerstate = 'poweredOn' THEN CAST(Memory * (0.5 + RAND() * 0.3) AS INT) ELSE 0 END, -- Consumed
    CASE WHEN Powerstate = 'poweredOn' THEN CAST(Memory * (0.3 + RAND() * 0.2) AS INT) ELSE 0 END, -- Active
    0, -- Ballooned
    0, -- Reservation
    -1, -- Unlimited
    'Normal' -- Shares
FROM [Current].[vInfo];

PRINT 'Generated vMemory data for all VMs';
GO

-- ============================================================================
-- 7. Generate vSnapshot data (some VMs have snapshots)
-- ============================================================================

PRINT 'Generating snapshot data...';

INSERT INTO [Current].[vSnapshot] (
    VM, Powerstate, Host, Datacenter, VI_SDK_Server,
    Snapshot, [Snapshot Created], [Snapshot Consolidation Needed],
    [Snapshot Size], Description
)
SELECT TOP 30
    VM, Powerstate, Host, Datacenter, VI_SDK_Server,
    'Snapshot-' + CAST(ROW_NUMBER() OVER (ORDER BY VM) AS NVARCHAR),
    DATEADD(DAY, -1 * CAST(RAND() * 90 AS INT), GETUTCDATE()), -- Random date within last 90 days
    CASE WHEN RAND() > 0.9 THEN 1 ELSE 0 END, -- 10% need consolidation
    CAST(1024 + RAND() * 10240 AS BIGINT), -- 1-10 GB
    CASE
        WHEN ROW_NUMBER() OVER (ORDER BY VM) % 5 = 0 THEN 'Pre-patching snapshot'
        WHEN ROW_NUMBER() OVER (ORDER BY VM) % 5 = 1 THEN 'Before upgrade'
        WHEN ROW_NUMBER() OVER (ORDER BY VM) % 5 = 2 THEN 'Backup point'
        ELSE NULL
    END
FROM [Current].[vInfo]
WHERE Powerstate = 'poweredOn'
ORDER BY NEWID();

PRINT 'Generated 30 snapshots';
GO

-- ============================================================================
-- 8. Generate vHealth data (some health issues)
-- ============================================================================

PRINT 'Generating health issues...';

INSERT INTO [Current].[vHealth] (
    Name, VI_SDK_Server, Message_type, Message,
    [Acknowledged], [Acknowledged by], [Acknowledged time]
)
VALUES
('esxi01.company.com', 'vcenter01.company.com', 'warning', 'Host connection state is not responding', 0, NULL, NULL),
('esxi05.company.com', 'vcenter01.company.com', 'error', 'Host has lost connectivity to storage', 0, NULL, NULL),
('prod-vm-042', 'vcenter01.company.com', 'warning', 'VMware Tools is not installed', 0, NULL, NULL),
('prod-vm-087', 'vcenter01.company.com', 'warning', 'Snapshot is more than 3 days old', 0, NULL, NULL),
('esxi12.company.com', 'vcenter01.company.com', 'warning', 'CPU usage is high', 0, NULL, NULL);

PRINT 'Generated 5 health issues';
GO

-- ============================================================================
-- 9. Generate vTools data (VMware Tools status)
-- ============================================================================

PRINT 'Generating VMware Tools data...';

INSERT INTO [Current].[vTools] (
    VM, Powerstate, Host, Datacenter, Cluster, VI_SDK_Server,
    [VM Tools], [Tools status], [Tools version], [Tools running status],
    [Tools version status]
)
SELECT
    VM, Powerstate, Host, Datacenter, Cluster, VI_SDK_Server,
    CASE WHEN ROW_NUMBER() OVER (ORDER BY VM) % 10 = 0 THEN 'Not installed' ELSE 'Installed' END,
    CASE
        WHEN ROW_NUMBER() OVER (ORDER BY VM) % 10 = 0 THEN 'toolsNotInstalled'
        WHEN ROW_NUMBER() OVER (ORDER BY VM) % 8 = 0 THEN 'toolsOld'
        ELSE 'toolsOk'
    END,
    CASE WHEN ROW_NUMBER() OVER (ORDER BY VM) % 10 = 0 THEN NULL ELSE '12.1.5' END,
    CASE WHEN Powerstate = 'poweredOn' THEN 'guestToolsRunning' ELSE 'guestToolsNotRunning' END,
    CASE
        WHEN ROW_NUMBER() OVER (ORDER BY VM) % 8 = 0 THEN 'guestToolsNeedUpgrade'
        ELSE 'guestToolsCurrent'
    END
FROM [Current].[vInfo]
WHERE Template = 0;

PRINT 'Generated VMware Tools data for all VMs';
GO

-- ============================================================================
-- 10. Create Import Batch Record
-- ============================================================================

PRINT 'Creating import batch record...';

INSERT INTO [Audit].[ImportBatch] (
    FileName, FilePath, VIServer, RVToolsExportDate,
    ImportStartTime, ImportEndTime, Status,
    TotalSheets, SheetsProcessed,
    TotalRowsSource, TotalRowsStaged, TotalRowsFailed
)
VALUES (
    'TestData_Generated.xlsx',
    '/generated/test/data',
    'vcenter01.company.com',
    GETUTCDATE(),
    DATEADD(MINUTE, -5, GETUTCDATE()),
    GETUTCDATE(),
    'Success',
    10, 10,
    500, 500, 0
);

PRINT 'Created import batch record';
GO

-- ============================================================================
-- Verification
-- ============================================================================

PRINT '';
PRINT '============================================================';
PRINT 'TEST DATA GENERATION COMPLETE';
PRINT '============================================================';
PRINT '';
PRINT 'Record Counts:';

SELECT 'Hosts' AS Entity, COUNT(*) AS Count FROM [Current].[vHost]
UNION ALL SELECT 'Datastores', COUNT(*) FROM [Current].[vDatastore]
UNION ALL SELECT 'Clusters', COUNT(*) FROM [Current].[vCluster]
UNION ALL SELECT 'VMs', COUNT(*) FROM [Current].[vInfo]
UNION ALL SELECT 'Snapshots', COUNT(*) FROM [Current].[vSnapshot]
UNION ALL SELECT 'Health Issues', COUNT(*) FROM [Current].[vHealth]
UNION ALL SELECT 'VMware Tools', COUNT(*) FROM [Current].[vTools]
ORDER BY Entity;

PRINT '';
PRINT 'VM Power State Distribution:';
SELECT Powerstate, COUNT(*) AS Count
FROM [Current].[vInfo]
GROUP BY Powerstate
ORDER BY Powerstate;

PRINT '';
PRINT 'VMs per Datacenter:';
SELECT Datacenter, COUNT(*) AS VMCount
FROM [Current].[vInfo]
GROUP BY Datacenter;

PRINT '';
PRINT 'VMs per Cluster:';
SELECT Cluster, COUNT(*) AS VMCount
FROM [Current].[vInfo]
GROUP BY Cluster
ORDER BY VMCount DESC;

PRINT '';
PRINT '============================================================';
PRINT 'You can now navigate to the web dashboard to take screenshots!';
PRINT 'The dashboard will show:';
PRINT '  - 2 vCenters, 2 Datacenters, 4 Clusters';
PRINT '  - 20 ESXi Hosts, 10 Datastores, 300 Virtual Machines';
PRINT '  - Power state distribution, resource allocation';
PRINT '  - Health issues, snapshots, VMware Tools status';
PRINT '============================================================';
GO

-- Cleanup helper function
DROP FUNCTION IF EXISTS dbo.fn_RandomString;
GO
