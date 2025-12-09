/*
    RVTools Data Warehouse - Current Tables

    Purpose: Creates all 27 current tables with typed columns
             These hold the latest snapshot of each entity

    Notes:
    - Proper data types for each column
    - Natural keys defined (VM_UUID + VI_SDK_Server for VMs)
    - ImportBatchId tracks which import provided the data
    - LastModifiedDate tracks when record was last updated

    Usage: Execute against RVToolsDW database
           sqlcmd -S localhost -d RVToolsDW -i 001_AllCurrentTables.sql
*/

USE [RVToolsDW]
GO

-- ============================================================================
-- Current.vInfo (Primary VM inventory)
-- ============================================================================
IF OBJECT_ID('Current.vInfo', 'U') IS NOT NULL DROP TABLE [Current].[vInfo]
GO

CREATE TABLE [Current].[vInfo] (
    vInfoId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    -- VM Identity (Natural Key: VM_UUID + VI_SDK_Server)
    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [SMBIOS_UUID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,
    [VI_SDK_Server_type] NVARCHAR(50) NULL,
    [VI_SDK_API_Version] NVARCHAR(20) NULL,

    -- Power State
    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,
    [Config_status] NVARCHAR(50) NULL,
    [Connection_state] NVARCHAR(50) NULL,
    [Guest_state] NVARCHAR(50) NULL,
    [Heartbeat] NVARCHAR(50) NULL,
    [Consolidation_Needed] BIT NULL,

    -- Power Dates
    [PowerOn] DATETIME2 NULL,
    [Suspended_To_Memory] BIT NULL,
    [Suspend_time] DATETIME2 NULL,
    [Suspend_Interval] NVARCHAR(100) NULL,
    [Creation_date] DATETIME2 NULL,
    [Change_Version] NVARCHAR(100) NULL,

    -- Compute Resources
    [CPUs] INT NULL,
    [Overall_Cpu_Readiness] DECIMAL(10,2) NULL,
    [Memory] BIGINT NULL,  -- MiB
    [Active_Memory] BIGINT NULL,
    [NICs] INT NULL,
    [Disks] INT NULL,
    [Total_disk_capacity_MiB] BIGINT NULL,

    -- Advanced Settings
    [Fixed_Passthru_HotPlug] BIT NULL,
    [min_Required_EVC_Mode_Key] NVARCHAR(100) NULL,
    [Latency_Sensitivity] NVARCHAR(50) NULL,
    [Op_Notification_Timeout] INT NULL,
    [EnableUUID] BIT NULL,
    [CBT] BIT NULL,

    -- Network
    [Primary_IP_Address] NVARCHAR(50) NULL,
    [Network_1] NVARCHAR(255) NULL,
    [Network_2] NVARCHAR(255) NULL,
    [Network_3] NVARCHAR(255) NULL,
    [Network_4] NVARCHAR(255) NULL,
    [Network_5] NVARCHAR(255) NULL,
    [Network_6] NVARCHAR(255) NULL,
    [Network_7] NVARCHAR(255) NULL,
    [Network_8] NVARCHAR(255) NULL,

    -- Display
    [Num_Monitors] INT NULL,
    [Video_Ram_KiB] INT NULL,

    -- Organization
    [Resource_pool] NVARCHAR(255) NULL,
    [Folder_ID] NVARCHAR(100) NULL,
    [Folder] NVARCHAR(500) NULL,
    [vApp] NVARCHAR(255) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,

    -- Fault Tolerance
    [DAS_protection] NVARCHAR(50) NULL,
    [FT_State] NVARCHAR(50) NULL,
    [FT_Role] NVARCHAR(50) NULL,
    [FT_Latency] INT NULL,
    [FT_Bandwidth] INT NULL,
    [FT_Sec_Latency] INT NULL,
    [Vm_Failover_In_Progress] BIT NULL,

    -- Storage
    [Provisioned_MiB] BIGINT NULL,
    [In_Use_MiB] BIGINT NULL,
    [Unshared_MiB] BIGINT NULL,

    -- HA Settings
    [HA_Restart_Priority] NVARCHAR(50) NULL,
    [HA_Isolation_Response] NVARCHAR(50) NULL,
    [HA_VM_Monitoring] NVARCHAR(50) NULL,
    [Cluster_rules] NVARCHAR(500) NULL,
    [Cluster_rule_names] NVARCHAR(500) NULL,

    -- Boot Settings
    [Boot_Required] BIT NULL,
    [Boot_delay] INT NULL,
    [Boot_retry_delay] INT NULL,
    [Boot_retry_enabled] BIT NULL,
    [Boot_BIOS_setup] BIT NULL,
    [Reboot_PowerOff] BIT NULL,
    [EFI_Secure_boot] BIT NULL,
    [Firmware] NVARCHAR(20) NULL,

    -- Hardware
    [HW_version] NVARCHAR(20) NULL,
    [HW_upgrade_status] NVARCHAR(50) NULL,
    [HW_upgrade_policy] NVARCHAR(50) NULL,
    [HW_target] NVARCHAR(20) NULL,

    -- Paths
    [Path] NVARCHAR(1000) NULL,
    [Log_directory] NVARCHAR(1000) NULL,
    [Snapshot_directory] NVARCHAR(1000) NULL,
    [Suspend_directory] NVARCHAR(1000) NULL,

    -- Annotation and Custom Fields
    [Annotation] NVARCHAR(MAX) NULL,

    -- OS Info
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,
    [Customization_Info] NVARCHAR(255) NULL,
    [Guest_Detailed_Data] NVARCHAR(MAX) NULL,
    [DNS_Name] NVARCHAR(255) NULL,

    -- Constraints
    CONSTRAINT UQ_Current_vInfo_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server)
)
GO

CREATE NONCLUSTERED INDEX IX_Current_vInfo_VM ON [Current].[vInfo](VM)
CREATE NONCLUSTERED INDEX IX_Current_vInfo_Datacenter ON [Current].[vInfo](Datacenter, Cluster)
CREATE NONCLUSTERED INDEX IX_Current_vInfo_Host ON [Current].[vInfo](Host)
CREATE NONCLUSTERED INDEX IX_Current_vInfo_Powerstate ON [Current].[vInfo](Powerstate)
GO

PRINT 'Created [Current].[vInfo]'
GO

-- ============================================================================
-- Current.vCPU
-- ============================================================================
IF OBJECT_ID('Current.vCPU', 'U') IS NOT NULL DROP TABLE [Current].[vCPU]
GO

CREATE TABLE [Current].[vCPU] (
    vCPUId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,

    [CPUs] INT NULL,
    [Sockets] INT NULL,
    [Cores_per_socket] INT NULL,
    [Max] BIGINT NULL,
    [Overall] BIGINT NULL,
    [Level] NVARCHAR(20) NULL,
    [Shares] INT NULL,
    [Reservation] BIGINT NULL,
    [Entitlement] BIGINT NULL,
    [DRS_Entitlement] BIGINT NULL,
    [Limit] BIGINT NULL,
    [Hot_Add] BIT NULL,
    [Hot_Remove] BIT NULL,
    [Numa_Hotadd_Exposed] BIT NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_vCPU_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server)
)
GO

PRINT 'Created [Current].[vCPU]'
GO

-- ============================================================================
-- Current.vMemory
-- ============================================================================
IF OBJECT_ID('Current.vMemory', 'U') IS NOT NULL DROP TABLE [Current].[vMemory]
GO

CREATE TABLE [Current].[vMemory] (
    vMemoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,

    [Size_MiB] BIGINT NULL,
    [Memory_Reservation_Locked_To_Max] BIT NULL,
    [Overhead] BIGINT NULL,
    [Max] BIGINT NULL,
    [Consumed] BIGINT NULL,
    [Consumed_Overhead] BIGINT NULL,
    [Private] BIGINT NULL,
    [Shared] BIGINT NULL,
    [Swapped] BIGINT NULL,
    [Ballooned] BIGINT NULL,
    [Active] BIGINT NULL,
    [Entitlement] BIGINT NULL,
    [DRS_Entitlement] BIGINT NULL,
    [Level] NVARCHAR(20) NULL,
    [Shares] INT NULL,
    [Reservation] BIGINT NULL,
    [Limit] BIGINT NULL,
    [Hot_Add] BIT NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_vMemory_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server)
)
GO

PRINT 'Created [Current].[vMemory]'
GO

-- ============================================================================
-- Current.vDisk
-- ============================================================================
IF OBJECT_ID('Current.vDisk', 'U') IS NOT NULL DROP TABLE [Current].[vDisk]
GO

CREATE TABLE [Current].[vDisk] (
    vDiskId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,

    [Disk] NVARCHAR(255) NULL,
    [Disk_Key] INT NULL,
    [Disk_UUID] NVARCHAR(100) NULL,
    [Disk_Path] NVARCHAR(1000) NULL,
    [Capacity_MiB] BIGINT NULL,
    [Raw] BIT NULL,
    [Disk_Mode] NVARCHAR(50) NULL,
    [Sharing_mode] NVARCHAR(50) NULL,
    [Thin] BIT NULL,
    [Eagerly_Scrub] BIT NULL,
    [Split] BIT NULL,
    [Write_Through] BIT NULL,
    [Level] NVARCHAR(20) NULL,
    [Shares] INT NULL,
    [Reservation] BIGINT NULL,
    [Limit] BIGINT NULL,
    [Controller] NVARCHAR(100) NULL,
    [Label] NVARCHAR(255) NULL,
    [SCSI_Unit_Num] INT NULL,
    [Unit_Num] INT NULL,
    [Shared_Bus] NVARCHAR(50) NULL,
    [Path] NVARCHAR(1000) NULL,
    [Raw_LUN_ID] NVARCHAR(100) NULL,
    [Raw_Comp_Mode] NVARCHAR(50) NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,

    -- Natural key includes Disk_Key for multiple disks per VM
    CONSTRAINT UQ_Current_vDisk_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server, Disk_Key)
)
GO

PRINT 'Created [Current].[vDisk]'
GO

-- ============================================================================
-- Current.vPartition
-- ============================================================================
IF OBJECT_ID('Current.vPartition', 'U') IS NOT NULL DROP TABLE [Current].[vPartition]
GO

CREATE TABLE [Current].[vPartition] (
    vPartitionId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,

    [Disk_Key] INT NULL,
    [Disk] NVARCHAR(500) NULL,
    [Capacity_MiB] BIGINT NULL,
    [Consumed_MiB] BIGINT NULL,
    [Free_MiB] BIGINT NULL,
    [Free_Percent] DECIMAL(5,2) NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_vPartition_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server, Disk)
)
GO

PRINT 'Created [Current].[vPartition]'
GO

-- ============================================================================
-- Current.vNetwork
-- ============================================================================
IF OBJECT_ID('Current.vNetwork', 'U') IS NOT NULL DROP TABLE [Current].[vNetwork]
GO

CREATE TABLE [Current].[vNetwork] (
    vNetworkId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,

    [NIC_label] NVARCHAR(100) NULL,
    [Adapter] NVARCHAR(100) NULL,
    [Network] NVARCHAR(255) NULL,
    [Switch] NVARCHAR(255) NULL,
    [Connected] BIT NULL,
    [Starts_Connected] BIT NULL,
    [Mac_Address] NVARCHAR(50) NULL,
    [Type] NVARCHAR(50) NULL,
    [IPv4_Address] NVARCHAR(50) NULL,
    [IPv6_Address] NVARCHAR(100) NULL,
    [Direct_Path_IO] BIT NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_vNetwork_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server, NIC_label)
)
GO

PRINT 'Created [Current].[vNetwork]'
GO

-- ============================================================================
-- Current.vCD
-- ============================================================================
IF OBJECT_ID('Current.vCD', 'U') IS NOT NULL DROP TABLE [Current].[vCD]
GO

CREATE TABLE [Current].[vCD] (
    vCDId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VMRef] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,

    [Device_Node] NVARCHAR(100) NULL,
    [Connected] BIT NULL,
    [Starts_Connected] BIT NULL,
    [Device_Type] NVARCHAR(100) NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_vCD_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server, Device_Node)
)
GO

PRINT 'Created [Current].[vCD]'
GO

-- ============================================================================
-- Current.vUSB
-- ============================================================================
IF OBJECT_ID('Current.vUSB', 'U') IS NOT NULL DROP TABLE [Current].[vUSB]
GO

CREATE TABLE [Current].[vUSB] (
    vUSBId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VMRef] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,

    [Device_Node] NVARCHAR(100) NULL,
    [Device_Type] NVARCHAR(100) NULL,
    [Connected] BIT NULL,
    [Family] NVARCHAR(50) NULL,
    [Speed] NVARCHAR(50) NULL,
    [EHCI_enabled] BIT NULL,
    [Auto_connect] BIT NULL,
    [Bus_number] INT NULL,
    [Unit_number] INT NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_tools] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_vUSB_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server, Device_Node)
)
GO

PRINT 'Created [Current].[vUSB]'
GO

-- ============================================================================
-- Current.vSnapshot
-- ============================================================================
IF OBJECT_ID('Current.vSnapshot', 'U') IS NOT NULL DROP TABLE [Current].[vSnapshot]
GO

CREATE TABLE [Current].[vSnapshot] (
    vSnapshotId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,

    [Name] NVARCHAR(500) NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Date_time] DATETIME2 NULL,
    [Filename] NVARCHAR(500) NULL,
    [Size_MiB_vmsn] BIGINT NULL,
    [Size_MiB_total] BIGINT NULL,
    [Quiesced] BIT NULL,
    [State] NVARCHAR(50) NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,

    -- Multiple snapshots per VM, so include Name in key
    CONSTRAINT UQ_Current_vSnapshot_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server, Name, Date_time)
)
GO

CREATE NONCLUSTERED INDEX IX_Current_vSnapshot_Date ON [Current].[vSnapshot](Date_time)
GO

PRINT 'Created [Current].[vSnapshot]'
GO

-- ============================================================================
-- Current.vTools
-- ============================================================================
IF OBJECT_ID('Current.vTools', 'U') IS NOT NULL DROP TABLE [Current].[vTools]
GO

CREATE TABLE [Current].[vTools] (
    vToolsId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [VMRef] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,

    [VM_Version] NVARCHAR(20) NULL,
    [Tools] NVARCHAR(50) NULL,
    [Tools_Version] NVARCHAR(50) NULL,
    [Required_Version] NVARCHAR(50) NULL,
    [Upgradeable] BIT NULL,
    [Upgrade_Policy] NVARCHAR(50) NULL,
    [Sync_time] BIT NULL,
    [App_status] NVARCHAR(50) NULL,
    [Heartbeat_status] NVARCHAR(50) NULL,
    [Kernel_Crash_state] NVARCHAR(50) NULL,
    [Operation_Ready] BIT NULL,
    [State_change_support] NVARCHAR(50) NULL,
    [Interactive_Guest] BIT NULL,

    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [Folder] NVARCHAR(500) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_vTools_NaturalKey UNIQUE (VM_UUID, VI_SDK_Server)
)
GO

PRINT 'Created [Current].[vTools]'
GO

-- ============================================================================
-- Current.vSource
-- ============================================================================
IF OBJECT_ID('Current.vSource', 'U') IS NOT NULL DROP TABLE [Current].[vSource]
GO

CREATE TABLE [Current].[vSource] (
    vSourceId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Name] NVARCHAR(255) NULL,
    [OS_type] NVARCHAR(100) NULL,
    [API_type] NVARCHAR(50) NULL,
    [API_version] NVARCHAR(20) NULL,
    [Version] NVARCHAR(50) NULL,
    [Patch_level] NVARCHAR(50) NULL,
    [Build] NVARCHAR(50) NULL,
    [Fullname] NVARCHAR(500) NULL,
    [Product_name] NVARCHAR(255) NULL,
    [Product_version] NVARCHAR(50) NULL,
    [Product_line] NVARCHAR(100) NULL,
    [Vendor] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    CONSTRAINT UQ_Current_vSource_NaturalKey UNIQUE (VI_SDK_Server, VI_SDK_UUID)
)
GO

PRINT 'Created [Current].[vSource]'
GO

-- ============================================================================
-- Current.vRP (Resource Pools)
-- ============================================================================
IF OBJECT_ID('Current.vRP', 'U') IS NOT NULL DROP TABLE [Current].[vRP]
GO

CREATE TABLE [Current].[vRP] (
    vRPId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Resource_Pool_name] NVARCHAR(255) NULL,
    [Resource_Pool_path] NVARCHAR(1000) NULL,
    [Object_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Status] NVARCHAR(50) NULL,
    [VMs_total] INT NULL,
    [VMs] INT NULL,
    [vCPUs] INT NULL,

    -- CPU Settings
    [CPU_limit] BIGINT NULL,
    [CPU_overheadLimit] BIGINT NULL,
    [CPU_reservation] BIGINT NULL,
    [CPU_level] NVARCHAR(20) NULL,
    [CPU_shares] INT NULL,
    [CPU_expandableReservation] BIT NULL,
    [CPU_maxUsage] BIGINT NULL,
    [CPU_overallUsage] BIGINT NULL,
    [CPU_reservationUsed] BIGINT NULL,
    [CPU_reservationUsedForVm] BIGINT NULL,
    [CPU_unreservedForPool] BIGINT NULL,
    [CPU_unreservedForVm] BIGINT NULL,

    -- Memory Settings
    [Mem_Configured] BIGINT NULL,
    [Mem_limit] BIGINT NULL,
    [Mem_overheadLimit] BIGINT NULL,
    [Mem_reservation] BIGINT NULL,
    [Mem_level] NVARCHAR(20) NULL,
    [Mem_shares] INT NULL,
    [Mem_expandableReservation] BIT NULL,
    [Mem_maxUsage] BIGINT NULL,
    [Mem_overallUsage] BIGINT NULL,
    [Mem_reservationUsed] BIGINT NULL,
    [Mem_reservationUsedForVm] BIGINT NULL,
    [Mem_unreservedForPool] BIGINT NULL,
    [Mem_unreservedForVm] BIGINT NULL,

    -- Quick Stats
    [QS_overallCpuDemand] BIGINT NULL,
    [QS_overallCpuUsage] BIGINT NULL,
    [QS_staticCpuEntitlement] BIGINT NULL,
    [QS_distributedCpuEntitlement] BIGINT NULL,
    [QS_balloonedMemory] BIGINT NULL,
    [QS_compressedMemory] BIGINT NULL,
    [QS_consumedOverheadMemory] BIGINT NULL,
    [QS_distributedMemoryEntitlement] BIGINT NULL,
    [QS_guestMemoryUsage] BIGINT NULL,
    [QS_hostMemoryUsage] BIGINT NULL,
    [QS_overheadMemory] BIGINT NULL,
    [QS_privateMemory] BIGINT NULL,
    [QS_sharedMemory] BIGINT NULL,
    [QS_staticMemoryEntitlement] BIGINT NULL,
    [QS_swappedMemory] BIGINT NULL,

    CONSTRAINT UQ_Current_vRP_NaturalKey UNIQUE (Object_ID, VI_SDK_Server)
)
GO

PRINT 'Created [Current].[vRP]'
GO

-- ============================================================================
-- Current.vCluster
-- ============================================================================
IF OBJECT_ID('Current.vCluster', 'U') IS NOT NULL DROP TABLE [Current].[vCluster]
GO

CREATE TABLE [Current].[vCluster] (
    vClusterId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Name] NVARCHAR(255) NULL,
    [Object_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Config_status] NVARCHAR(50) NULL,
    [OverallStatus] NVARCHAR(50) NULL,
    [NumHosts] INT NULL,
    [numEffectiveHosts] INT NULL,
    [TotalCpu] BIGINT NULL,
    [NumCpuCores] INT NULL,
    [NumCpuThreads] INT NULL,
    [Effective_Cpu] BIGINT NULL,
    [TotalMemory] BIGINT NULL,
    [Effective_Memory] BIGINT NULL,
    [Num_VMotions] INT NULL,

    -- HA Settings
    [HA_enabled] BIT NULL,
    [Failover_Level] INT NULL,
    [AdmissionControlEnabled] BIT NULL,
    [Host_monitoring] NVARCHAR(50) NULL,
    [HB_Datastore_Candidate_Policy] NVARCHAR(100) NULL,
    [Isolation_Response] NVARCHAR(50) NULL,
    [Restart_Priority] NVARCHAR(50) NULL,
    [Cluster_Settings] NVARCHAR(500) NULL,
    [Max_Failures] INT NULL,
    [Max_Failure_Window] INT NULL,
    [Failure_Interval] INT NULL,
    [Min_Up_Time] INT NULL,
    [VM_Monitoring] NVARCHAR(50) NULL,

    -- DRS Settings
    [DRS_enabled] BIT NULL,
    [DRS_default_VM_behavior] NVARCHAR(50) NULL,
    [DRS_vmotion_rate] INT NULL,
    [DPM_enabled] BIT NULL,
    [DPM_default_behavior] NVARCHAR(50) NULL,
    [DPM_Host_Power_Action_Rate] INT NULL,

    CONSTRAINT UQ_Current_vCluster_NaturalKey UNIQUE (Name, VI_SDK_Server)
)
GO

PRINT 'Created [Current].[vCluster]'
GO

-- ============================================================================
-- Current.vHost
-- ============================================================================
IF OBJECT_ID('Current.vHost', 'U') IS NOT NULL DROP TABLE [Current].[vHost]
GO

CREATE TABLE [Current].[vHost] (
    vHostId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Host] NVARCHAR(255) NULL,
    [Object_ID] NVARCHAR(100) NULL,
    [UUID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Config_status] NVARCHAR(50) NULL,
    [Compliance_Check_State] NVARCHAR(50) NULL,
    [in_Maintenance_Mode] BIT NULL,
    [in_Quarantine_Mode] BIT NULL,
    [vSAN_Fault_Domain_Name] NVARCHAR(255) NULL,

    -- CPU
    [CPU_Model] NVARCHAR(255) NULL,
    [Speed] INT NULL,
    [HT_Available] BIT NULL,
    [HT_Active] BIT NULL,
    [Num_CPU] INT NULL,
    [Cores_per_CPU] INT NULL,
    [Num_Cores] INT NULL,
    [CPU_usage_Percent] DECIMAL(5,2) NULL,

    -- Memory
    [Num_Memory] BIGINT NULL,
    [Memory_Tiering_Type] NVARCHAR(50) NULL,
    [Memory_usage_Percent] DECIMAL(5,2) NULL,
    [Console] BIGINT NULL,

    -- Hardware
    [Num_NICs] INT NULL,
    [Num_HBAs] INT NULL,

    -- VMs
    [Num_VMs_total] INT NULL,
    [Num_VMs] INT NULL,
    [VMs_per_Core] DECIMAL(5,2) NULL,
    [Num_vCPUs] INT NULL,
    [vCPUs_per_Core] DECIMAL(5,2) NULL,
    [vRAM] BIGINT NULL,
    [VM_Used_memory] BIGINT NULL,
    [VM_Memory_Swapped] BIGINT NULL,
    [VM_Memory_Ballooned] BIGINT NULL,

    -- Features
    [VMotion_support] BIT NULL,
    [Storage_VMotion_support] BIT NULL,
    [Current_EVC] NVARCHAR(100) NULL,
    [Max_EVC] NVARCHAR(100) NULL,
    [Assigned_Licenses] NVARCHAR(500) NULL,
    [ATS_Heartbeat] NVARCHAR(50) NULL,
    [ATS_Locking] NVARCHAR(50) NULL,

    -- Power
    [Current_CPU_power_man_policy] NVARCHAR(100) NULL,
    [Supported_CPU_power_man] NVARCHAR(255) NULL,
    [Host_Power_Policy] NVARCHAR(100) NULL,

    -- Version
    [ESX_Version] NVARCHAR(100) NULL,
    [Boot_time] DATETIME2 NULL,

    -- Network
    [DNS_Servers] NVARCHAR(500) NULL,
    [DHCP] BIT NULL,
    [Domain] NVARCHAR(255) NULL,
    [Domain_List] NVARCHAR(500) NULL,
    [DNS_Search_Order] NVARCHAR(500) NULL,
    [NTP_Servers] NVARCHAR(500) NULL,
    [NTPD_running] BIT NULL,
    [Time_Zone] NVARCHAR(50) NULL,
    [Time_Zone_Name] NVARCHAR(100) NULL,
    [GMT_Offset] NVARCHAR(20) NULL,

    -- Hardware Info
    [Vendor] NVARCHAR(100) NULL,
    [Model] NVARCHAR(255) NULL,
    [Serial_number] NVARCHAR(100) NULL,
    [Service_tag] NVARCHAR(100) NULL,
    [OEM_specific_string] NVARCHAR(500) NULL,
    [BIOS_Vendor] NVARCHAR(100) NULL,
    [BIOS_Version] NVARCHAR(50) NULL,
    [BIOS_Date] NVARCHAR(50) NULL,

    -- Certificate
    [Certificate_Issuer] NVARCHAR(500) NULL,
    [Certificate_Start_Date] DATETIME2 NULL,
    [Certificate_Expiry_Date] DATETIME2 NULL,
    [Certificate_Status] NVARCHAR(50) NULL,
    [Certificate_Subject] NVARCHAR(500) NULL,

    CONSTRAINT UQ_Current_vHost_NaturalKey UNIQUE (Host, VI_SDK_Server)
)
GO

CREATE NONCLUSTERED INDEX IX_Current_vHost_Cluster ON [Current].[vHost](Datacenter, Cluster)
GO

PRINT 'Created [Current].[vHost]'
GO

-- ============================================================================
-- Current.vHBA
-- ============================================================================
IF OBJECT_ID('Current.vHBA', 'U') IS NOT NULL DROP TABLE [Current].[vHBA]
GO

CREATE TABLE [Current].[vHBA] (
    vHBAId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Host] NVARCHAR(255) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Device] NVARCHAR(100) NULL,
    [Type] NVARCHAR(50) NULL,
    [Status] NVARCHAR(50) NULL,
    [Bus] NVARCHAR(50) NULL,
    [Pci] NVARCHAR(50) NULL,
    [Driver] NVARCHAR(100) NULL,
    [Model] NVARCHAR(255) NULL,
    [WWN] NVARCHAR(100) NULL,

    CONSTRAINT UQ_Current_vHBA_NaturalKey UNIQUE (Host, VI_SDK_Server, Device)
)
GO

PRINT 'Created [Current].[vHBA]'
GO

-- ============================================================================
-- Current.vNIC
-- ============================================================================
IF OBJECT_ID('Current.vNIC', 'U') IS NOT NULL DROP TABLE [Current].[vNIC]
GO

CREATE TABLE [Current].[vNIC] (
    vNICId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Host] NVARCHAR(255) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Network_Device] NVARCHAR(100) NULL,
    [Driver] NVARCHAR(100) NULL,
    [Speed] INT NULL,
    [Duplex] NVARCHAR(20) NULL,
    [MAC] NVARCHAR(50) NULL,
    [Switch] NVARCHAR(255) NULL,
    [Uplink_port] NVARCHAR(100) NULL,
    [PCI] NVARCHAR(50) NULL,
    [WakeOn] NVARCHAR(50) NULL,

    CONSTRAINT UQ_Current_vNIC_NaturalKey UNIQUE (Host, VI_SDK_Server, Network_Device)
)
GO

PRINT 'Created [Current].[vNIC]'
GO

-- ============================================================================
-- Current.vSwitch
-- ============================================================================
IF OBJECT_ID('Current.vSwitch', 'U') IS NOT NULL DROP TABLE [Current].[vSwitch]
GO

CREATE TABLE [Current].[vSwitch] (
    vSwitchId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Host] NVARCHAR(255) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Switch] NVARCHAR(255) NULL,
    [Num_Ports] INT NULL,
    [Free_Ports] INT NULL,
    [Promiscuous_Mode] BIT NULL,
    [Mac_Changes] BIT NULL,
    [Forged_Transmits] BIT NULL,
    [Traffic_Shaping] BIT NULL,
    [Width] BIGINT NULL,
    [Peak] BIGINT NULL,
    [Burst] BIGINT NULL,
    [Policy] NVARCHAR(50) NULL,
    [Reverse_Policy] BIT NULL,
    [Notify_Switch] BIT NULL,
    [Rolling_Order] BIT NULL,
    [Offload] BIT NULL,
    [TSO] BIT NULL,
    [Zero_Copy_Xmit] BIT NULL,
    [MTU] INT NULL,

    CONSTRAINT UQ_Current_vSwitch_NaturalKey UNIQUE (Host, VI_SDK_Server, Switch)
)
GO

PRINT 'Created [Current].[vSwitch]'
GO

-- ============================================================================
-- Current.vPort
-- ============================================================================
IF OBJECT_ID('Current.vPort', 'U') IS NOT NULL DROP TABLE [Current].[vPort]
GO

CREATE TABLE [Current].[vPort] (
    vPortId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Host] NVARCHAR(255) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Port_Group] NVARCHAR(255) NULL,
    [Switch] NVARCHAR(255) NULL,
    [VLAN] INT NULL,
    [Promiscuous_Mode] BIT NULL,
    [Mac_Changes] BIT NULL,
    [Forged_Transmits] BIT NULL,
    [Traffic_Shaping] BIT NULL,
    [Width] BIGINT NULL,
    [Peak] BIGINT NULL,
    [Burst] BIGINT NULL,
    [Policy] NVARCHAR(50) NULL,
    [Reverse_Policy] BIT NULL,
    [Notify_Switch] BIT NULL,
    [Rolling_Order] BIT NULL,
    [Offload] BIT NULL,
    [TSO] BIT NULL,
    [Zero_Copy_Xmit] BIT NULL,

    CONSTRAINT UQ_Current_vPort_NaturalKey UNIQUE (Host, VI_SDK_Server, Port_Group, Switch)
)
GO

PRINT 'Created [Current].[vPort]'
GO

-- ============================================================================
-- Current.dvSwitch
-- ============================================================================
IF OBJECT_ID('Current.dvSwitch', 'U') IS NOT NULL DROP TABLE [Current].[dvSwitch]
GO

CREATE TABLE [Current].[dvSwitch] (
    dvSwitchId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Switch] NVARCHAR(100) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Name] NVARCHAR(255) NULL,
    [Object_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Vendor] NVARCHAR(100) NULL,
    [Version] NVARCHAR(50) NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Created] DATETIME2 NULL,
    [Host_members] INT NULL,
    [Max_Ports] INT NULL,
    [Num_Ports] INT NULL,
    [Num_VMs] INT NULL,

    -- Traffic Shaping
    [In_Traffic_Shaping] BIT NULL,
    [In_Avg] BIGINT NULL,
    [In_Peak] BIGINT NULL,
    [In_Burst] BIGINT NULL,
    [Out_Traffic_Shaping] BIT NULL,
    [Out_Avg] BIGINT NULL,
    [Out_Peak] BIGINT NULL,
    [Out_Burst] BIGINT NULL,

    -- CDP/LACP
    [CDP_Type] NVARCHAR(50) NULL,
    [CDP_Operation] NVARCHAR(50) NULL,
    [LACP_Name] NVARCHAR(100) NULL,
    [LACP_Mode] NVARCHAR(50) NULL,
    [LACP_Load_Balance_Alg] NVARCHAR(100) NULL,

    [Max_MTU] INT NULL,
    [Contact] NVARCHAR(255) NULL,
    [Admin_Name] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_dvSwitch_NaturalKey UNIQUE (Switch, VI_SDK_Server)
)
GO

PRINT 'Created [Current].[dvSwitch]'
GO

-- ============================================================================
-- Current.dvPort
-- ============================================================================
IF OBJECT_ID('Current.dvPort', 'U') IS NOT NULL DROP TABLE [Current].[dvPort]
GO

CREATE TABLE [Current].[dvPort] (
    dvPortId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Port] NVARCHAR(255) NULL,
    [Switch] NVARCHAR(255) NULL,
    [Object_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Type] NVARCHAR(50) NULL,
    [Num_Ports] INT NULL,
    [VLAN] NVARCHAR(100) NULL,
    [Speed] INT NULL,
    [Full_Duplex] BIT NULL,
    [Blocked] BIT NULL,
    [Allow_Promiscuous] BIT NULL,
    [Mac_Changes] BIT NULL,
    [Active_Uplink] NVARCHAR(500) NULL,
    [Standby_Uplink] NVARCHAR(500) NULL,
    [Policy] NVARCHAR(50) NULL,
    [Forged_Transmits] BIT NULL,

    -- Traffic Shaping
    [In_Traffic_Shaping] BIT NULL,
    [In_Avg] BIGINT NULL,
    [In_Peak] BIGINT NULL,
    [In_Burst] BIGINT NULL,
    [Out_Traffic_Shaping] BIT NULL,
    [Out_Avg] BIGINT NULL,
    [Out_Peak] BIGINT NULL,
    [Out_Burst] BIGINT NULL,

    [Reverse_Policy] BIT NULL,
    [Notify_Switch] BIT NULL,
    [Rolling_Order] BIT NULL,
    [Check_Beacon] BIT NULL,
    [Live_Port_Moving] BIT NULL,
    [Check_Duplex] BIT NULL,
    [Check_Error_Percent] DECIMAL(5,2) NULL,
    [Check_Speed] INT NULL,
    [Percentage] INT NULL,

    -- Overrides
    [Block_Override] BIT NULL,
    [Config_Reset] BIT NULL,
    [Shaping_Override] BIT NULL,
    [Vendor_Config_Override] BIT NULL,
    [Sec_Policy_Override] BIT NULL,
    [Teaming_Override] BIT NULL,
    [Vlan_Override] BIT NULL,

    CONSTRAINT UQ_Current_dvPort_NaturalKey UNIQUE (Port, Switch, VI_SDK_Server)
)
GO

PRINT 'Created [Current].[dvPort]'
GO

-- ============================================================================
-- Current.vSC_VMK
-- ============================================================================
IF OBJECT_ID('Current.vSC_VMK', 'U') IS NOT NULL DROP TABLE [Current].[vSC_VMK]
GO

CREATE TABLE [Current].[vSC_VMK] (
    vSC_VMKId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Host] NVARCHAR(255) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Port_Group] NVARCHAR(255) NULL,
    [Device] NVARCHAR(100) NULL,
    [Mac_Address] NVARCHAR(50) NULL,
    [DHCP] BIT NULL,
    [IP_Address] NVARCHAR(50) NULL,
    [IP_6_Address] NVARCHAR(100) NULL,
    [Subnet_mask] NVARCHAR(50) NULL,
    [Gateway] NVARCHAR(50) NULL,
    [IP_6_Gateway] NVARCHAR(100) NULL,
    [MTU] INT NULL,

    CONSTRAINT UQ_Current_vSC_VMK_NaturalKey UNIQUE (Host, VI_SDK_Server, Device)
)
GO

PRINT 'Created [Current].[vSC_VMK]'
GO

-- ============================================================================
-- Current.vDatastore
-- ============================================================================
IF OBJECT_ID('Current.vDatastore', 'U') IS NOT NULL DROP TABLE [Current].[vDatastore]
GO

CREATE TABLE [Current].[vDatastore] (
    vDatastoreId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Name] NVARCHAR(255) NULL,
    [Object_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Config_status] NVARCHAR(50) NULL,
    [Address] NVARCHAR(500) NULL,
    [Accessible] BIT NULL,
    [Type] NVARCHAR(50) NULL,

    [Num_VMs_total] INT NULL,
    [Num_VMs] INT NULL,
    [Capacity_MiB] BIGINT NULL,
    [Provisioned_MiB] BIGINT NULL,
    [In_Use_MiB] BIGINT NULL,
    [Free_MiB] BIGINT NULL,
    [Free_Percent] DECIMAL(5,2) NULL,

    [SIOC_enabled] BIT NULL,
    [SIOC_Threshold] INT NULL,

    [Num_Hosts] INT NULL,
    [Hosts] NVARCHAR(MAX) NULL,
    [Cluster_name] NVARCHAR(255) NULL,
    [Cluster_capacity_MiB] BIGINT NULL,
    [Cluster_free_space_MiB] BIGINT NULL,

    [Block_size] INT NULL,
    [Max_Blocks] BIGINT NULL,
    [Num_Extents] INT NULL,
    [Major_Version] INT NULL,
    [Version] NVARCHAR(20) NULL,
    [VMFS_Upgradeable] BIT NULL,
    [MHA] NVARCHAR(50) NULL,
    [URL] NVARCHAR(1000) NULL,
    [vSphereReplication] NVARCHAR(50) NULL,

    CONSTRAINT UQ_Current_vDatastore_NaturalKey UNIQUE (Name, VI_SDK_Server)
)
GO

CREATE NONCLUSTERED INDEX IX_Current_vDatastore_Type ON [Current].[vDatastore](Type)
CREATE NONCLUSTERED INDEX IX_Current_vDatastore_FreePercent ON [Current].[vDatastore](Free_Percent)
GO

PRINT 'Created [Current].[vDatastore]'
GO

-- ============================================================================
-- Current.vMultiPath
-- ============================================================================
IF OBJECT_ID('Current.vMultiPath', 'U') IS NOT NULL DROP TABLE [Current].[vMultiPath]
GO

CREATE TABLE [Current].[vMultiPath] (
    vMultiPathId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Host] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Object_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    [Datastore] NVARCHAR(255) NULL,
    [Disk] NVARCHAR(255) NULL,
    [Display_name] NVARCHAR(500) NULL,
    [Policy] NVARCHAR(100) NULL,
    [Oper_State] NVARCHAR(50) NULL,

    -- Paths
    [Path_1] NVARCHAR(255) NULL,
    [Path_1_state] NVARCHAR(50) NULL,
    [Path_2] NVARCHAR(255) NULL,
    [Path_2_state] NVARCHAR(50) NULL,
    [Path_3] NVARCHAR(255) NULL,
    [Path_3_state] NVARCHAR(50) NULL,
    [Path_4] NVARCHAR(255) NULL,
    [Path_4_state] NVARCHAR(50) NULL,
    [Path_5] NVARCHAR(255) NULL,
    [Path_5_state] NVARCHAR(50) NULL,
    [Path_6] NVARCHAR(255) NULL,
    [Path_6_state] NVARCHAR(50) NULL,
    [Path_7] NVARCHAR(255) NULL,
    [Path_7_state] NVARCHAR(50) NULL,
    [Path_8] NVARCHAR(255) NULL,
    [Path_8_state] NVARCHAR(50) NULL,

    [vStorage] BIT NULL,
    [Queue_depth] INT NULL,
    [Vendor] NVARCHAR(100) NULL,
    [Model] NVARCHAR(255) NULL,
    [Revision] NVARCHAR(50) NULL,
    [Level] NVARCHAR(20) NULL,
    [Serial_Num] NVARCHAR(100) NULL,
    [UUID] NVARCHAR(100) NULL,

    CONSTRAINT UQ_Current_vMultiPath_NaturalKey UNIQUE (Host, VI_SDK_Server, Disk)
)
GO

PRINT 'Created [Current].[vMultiPath]'
GO

-- ============================================================================
-- Current.vLicense
-- ============================================================================
IF OBJECT_ID('Current.vLicense', 'U') IS NOT NULL DROP TABLE [Current].[vLicense]
GO

CREATE TABLE [Current].[vLicense] (
    vLicenseId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Name] NVARCHAR(255) NULL,
    [Key] NVARCHAR(100) NULL,
    [Labels] NVARCHAR(500) NULL,
    [Cost_Unit] NVARCHAR(50) NULL,
    [Total] INT NULL,
    [Used] INT NULL,
    [Expiration_Date] DATETIME2 NULL,
    [Features] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    CONSTRAINT UQ_Current_vLicense_NaturalKey UNIQUE ([Key], VI_SDK_Server)
)
GO

PRINT 'Created [Current].[vLicense]'
GO

-- ============================================================================
-- Current.vFileInfo
-- ============================================================================
IF OBJECT_ID('Current.vFileInfo', 'U') IS NOT NULL DROP TABLE [Current].[vFileInfo]
GO

CREATE TABLE [Current].[vFileInfo] (
    vFileInfoId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Friendly_Path_Name] NVARCHAR(1000) NULL,
    [File_Name] NVARCHAR(500) NULL,
    [File_Type] NVARCHAR(100) NULL,
    [File_Size_in_bytes] BIGINT NULL,
    [Path] NVARCHAR(1000) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    CONSTRAINT UQ_Current_vFileInfo_NaturalKey UNIQUE (Path, File_Name, VI_SDK_Server)
)
GO

CREATE NONCLUSTERED INDEX IX_Current_vFileInfo_Type ON [Current].[vFileInfo](File_Type)
GO

PRINT 'Created [Current].[vFileInfo]'
GO

-- ============================================================================
-- Current.vHealth
-- ============================================================================
IF OBJECT_ID('Current.vHealth', 'U') IS NOT NULL DROP TABLE [Current].[vHealth]
GO

CREATE TABLE [Current].[vHealth] (
    vHealthId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [Name] NVARCHAR(500) NULL,
    [Message] NVARCHAR(MAX) NULL,
    [Message_type] NVARCHAR(50) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,

    -- No unique constraint - multiple health messages per server
    CONSTRAINT UQ_Current_vHealth_NaturalKey UNIQUE (Name, Message_type, VI_SDK_Server)
)
GO

CREATE NONCLUSTERED INDEX IX_Current_vHealth_MessageType ON [Current].[vHealth](Message_type)
GO

PRINT 'Created [Current].[vHealth]'
GO

-- ============================================================================
-- Current.vMetaData
-- ============================================================================
IF OBJECT_ID('Current.vMetaData', 'U') IS NOT NULL DROP TABLE [Current].[vMetaData]
GO

CREATE TABLE [Current].[vMetaData] (
    vMetaDataId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    [RVTools_major_version] INT NULL,
    [RVTools_version] NVARCHAR(50) NULL,
    [xlsx_creation_datetime] DATETIME2 NULL,
    [Server] NVARCHAR(255) NULL,

    CONSTRAINT UQ_Current_vMetaData_NaturalKey UNIQUE (Server, xlsx_creation_datetime)
)
GO

PRINT 'Created [Current].[vMetaData]'
GO

PRINT '=============================================='
PRINT 'All 27 current tables created successfully!'
PRINT '=============================================='
GO
