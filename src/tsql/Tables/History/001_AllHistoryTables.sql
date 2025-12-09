/*
    RVTools Data Warehouse - History Tables

    Purpose: Creates all 27 history tables for SCD Type 2 tracking
             These hold all historical versions of each entity

    Notes:
    - Same structure as Current tables
    - Added: ValidFrom, ValidTo, SourceFile for SCD Type 2
    - ValidTo = NULL means this is the current version
    - No unique constraint on natural key (allows multiple versions)
    - Indexes optimized for historical queries

    Usage: Execute against RVToolsDW database
           sqlcmd -S localhost -d RVToolsDW -i 001_AllHistoryTables.sql
*/

USE [RVToolsDW]
GO

-- ============================================================================
-- History.vInfo
-- ============================================================================
IF OBJECT_ID('History.vInfo', 'U') IS NOT NULL DROP TABLE [History].[vInfo]
GO

CREATE TABLE [History].[vInfo] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,  -- NULL = current version
    SourceFile NVARCHAR(500) NULL,

    -- All columns from Current.vInfo
    [VM] NVARCHAR(255) NULL,
    [VM_UUID] NVARCHAR(100) NULL,
    [VM_ID] NVARCHAR(100) NULL,
    [SMBIOS_UUID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,
    [VI_SDK_Server_type] NVARCHAR(50) NULL,
    [VI_SDK_API_Version] NVARCHAR(20) NULL,
    [Powerstate] NVARCHAR(20) NULL,
    [Template] BIT NULL,
    [SRM_Placeholder] BIT NULL,
    [Config_status] NVARCHAR(50) NULL,
    [Connection_state] NVARCHAR(50) NULL,
    [Guest_state] NVARCHAR(50) NULL,
    [Heartbeat] NVARCHAR(50) NULL,
    [Consolidation_Needed] BIT NULL,
    [PowerOn] DATETIME2 NULL,
    [Suspended_To_Memory] BIT NULL,
    [Suspend_time] DATETIME2 NULL,
    [Suspend_Interval] NVARCHAR(100) NULL,
    [Creation_date] DATETIME2 NULL,
    [Change_Version] NVARCHAR(100) NULL,
    [CPUs] INT NULL,
    [Overall_Cpu_Readiness] DECIMAL(10,2) NULL,
    [Memory] BIGINT NULL,
    [Active_Memory] BIGINT NULL,
    [NICs] INT NULL,
    [Disks] INT NULL,
    [Total_disk_capacity_MiB] BIGINT NULL,
    [Fixed_Passthru_HotPlug] BIT NULL,
    [min_Required_EVC_Mode_Key] NVARCHAR(100) NULL,
    [Latency_Sensitivity] NVARCHAR(50) NULL,
    [Op_Notification_Timeout] INT NULL,
    [EnableUUID] BIT NULL,
    [CBT] BIT NULL,
    [Primary_IP_Address] NVARCHAR(50) NULL,
    [Network_1] NVARCHAR(255) NULL,
    [Network_2] NVARCHAR(255) NULL,
    [Network_3] NVARCHAR(255) NULL,
    [Network_4] NVARCHAR(255) NULL,
    [Network_5] NVARCHAR(255) NULL,
    [Network_6] NVARCHAR(255) NULL,
    [Network_7] NVARCHAR(255) NULL,
    [Network_8] NVARCHAR(255) NULL,
    [Num_Monitors] INT NULL,
    [Video_Ram_KiB] INT NULL,
    [Resource_pool] NVARCHAR(255) NULL,
    [Folder_ID] NVARCHAR(100) NULL,
    [Folder] NVARCHAR(500) NULL,
    [vApp] NVARCHAR(255) NULL,
    [Datacenter] NVARCHAR(255) NULL,
    [Cluster] NVARCHAR(255) NULL,
    [Host] NVARCHAR(255) NULL,
    [DAS_protection] NVARCHAR(50) NULL,
    [FT_State] NVARCHAR(50) NULL,
    [FT_Role] NVARCHAR(50) NULL,
    [FT_Latency] INT NULL,
    [FT_Bandwidth] INT NULL,
    [FT_Sec_Latency] INT NULL,
    [Vm_Failover_In_Progress] BIT NULL,
    [Provisioned_MiB] BIGINT NULL,
    [In_Use_MiB] BIGINT NULL,
    [Unshared_MiB] BIGINT NULL,
    [HA_Restart_Priority] NVARCHAR(50) NULL,
    [HA_Isolation_Response] NVARCHAR(50) NULL,
    [HA_VM_Monitoring] NVARCHAR(50) NULL,
    [Cluster_rules] NVARCHAR(500) NULL,
    [Cluster_rule_names] NVARCHAR(500) NULL,
    [Boot_Required] BIT NULL,
    [Boot_delay] INT NULL,
    [Boot_retry_delay] INT NULL,
    [Boot_retry_enabled] BIT NULL,
    [Boot_BIOS_setup] BIT NULL,
    [Reboot_PowerOff] BIT NULL,
    [EFI_Secure_boot] BIT NULL,
    [Firmware] NVARCHAR(20) NULL,
    [HW_version] NVARCHAR(20) NULL,
    [HW_upgrade_status] NVARCHAR(50) NULL,
    [HW_upgrade_policy] NVARCHAR(50) NULL,
    [HW_target] NVARCHAR(20) NULL,
    [Path] NVARCHAR(1000) NULL,
    [Log_directory] NVARCHAR(1000) NULL,
    [Snapshot_directory] NVARCHAR(1000) NULL,
    [Suspend_directory] NVARCHAR(1000) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(255) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL,
    [Customization_Info] NVARCHAR(255) NULL,
    [Guest_Detailed_Data] NVARCHAR(MAX) NULL,
    [DNS_Name] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vInfo_NaturalKey ON [History].[vInfo](VM_UUID, VI_SDK_Server)
CREATE NONCLUSTERED INDEX IX_History_vInfo_ValidTo ON [History].[vInfo](ValidTo) WHERE ValidTo IS NULL
CREATE NONCLUSTERED INDEX IX_History_vInfo_ValidFrom ON [History].[vInfo](ValidFrom)
CREATE NONCLUSTERED INDEX IX_History_vInfo_ImportBatch ON [History].[vInfo](ImportBatchId)
GO

PRINT 'Created [History].[vInfo]'
GO

-- ============================================================================
-- History.vCPU
-- ============================================================================
IF OBJECT_ID('History.vCPU', 'U') IS NOT NULL DROP TABLE [History].[vCPU]
GO

CREATE TABLE [History].[vCPU] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vCPU_NaturalKey ON [History].[vCPU](VM_UUID, VI_SDK_Server)
CREATE NONCLUSTERED INDEX IX_History_vCPU_ValidTo ON [History].[vCPU](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vCPU]'
GO

-- ============================================================================
-- History.vMemory
-- ============================================================================
IF OBJECT_ID('History.vMemory', 'U') IS NOT NULL DROP TABLE [History].[vMemory]
GO

CREATE TABLE [History].[vMemory] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vMemory_NaturalKey ON [History].[vMemory](VM_UUID, VI_SDK_Server)
CREATE NONCLUSTERED INDEX IX_History_vMemory_ValidTo ON [History].[vMemory](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vMemory]'
GO

-- ============================================================================
-- History.vDisk
-- ============================================================================
IF OBJECT_ID('History.vDisk', 'U') IS NOT NULL DROP TABLE [History].[vDisk]
GO

CREATE TABLE [History].[vDisk] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vDisk_NaturalKey ON [History].[vDisk](VM_UUID, VI_SDK_Server, Disk_Key)
CREATE NONCLUSTERED INDEX IX_History_vDisk_ValidTo ON [History].[vDisk](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vDisk]'
GO

-- ============================================================================
-- History.vPartition
-- ============================================================================
IF OBJECT_ID('History.vPartition', 'U') IS NOT NULL DROP TABLE [History].[vPartition]
GO

CREATE TABLE [History].[vPartition] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vPartition_NaturalKey ON [History].[vPartition](VM_UUID, VI_SDK_Server, Disk)
CREATE NONCLUSTERED INDEX IX_History_vPartition_ValidTo ON [History].[vPartition](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vPartition]'
GO

-- ============================================================================
-- History.vNetwork
-- ============================================================================
IF OBJECT_ID('History.vNetwork', 'U') IS NOT NULL DROP TABLE [History].[vNetwork]
GO

CREATE TABLE [History].[vNetwork] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vNetwork_NaturalKey ON [History].[vNetwork](VM_UUID, VI_SDK_Server, NIC_label)
CREATE NONCLUSTERED INDEX IX_History_vNetwork_ValidTo ON [History].[vNetwork](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vNetwork]'
GO

-- ============================================================================
-- History.vCD
-- ============================================================================
IF OBJECT_ID('History.vCD', 'U') IS NOT NULL DROP TABLE [History].[vCD]
GO

CREATE TABLE [History].[vCD] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vCD_NaturalKey ON [History].[vCD](VM_UUID, VI_SDK_Server, Device_Node)
GO

PRINT 'Created [History].[vCD]'
GO

-- ============================================================================
-- History.vUSB
-- ============================================================================
IF OBJECT_ID('History.vUSB', 'U') IS NOT NULL DROP TABLE [History].[vUSB]
GO

CREATE TABLE [History].[vUSB] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vUSB_NaturalKey ON [History].[vUSB](VM_UUID, VI_SDK_Server, Device_Node)
GO

PRINT 'Created [History].[vUSB]'
GO

-- ============================================================================
-- History.vSnapshot
-- ============================================================================
IF OBJECT_ID('History.vSnapshot', 'U') IS NOT NULL DROP TABLE [History].[vSnapshot]
GO

CREATE TABLE [History].[vSnapshot] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vSnapshot_NaturalKey ON [History].[vSnapshot](VM_UUID, VI_SDK_Server, Name)
CREATE NONCLUSTERED INDEX IX_History_vSnapshot_ValidTo ON [History].[vSnapshot](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vSnapshot]'
GO

-- ============================================================================
-- History.vTools
-- ============================================================================
IF OBJECT_ID('History.vTools', 'U') IS NOT NULL DROP TABLE [History].[vTools]
GO

CREATE TABLE [History].[vTools] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [OS_according_to_the_VMware_Tools] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vTools_NaturalKey ON [History].[vTools](VM_UUID, VI_SDK_Server)
GO

PRINT 'Created [History].[vTools]'
GO

-- ============================================================================
-- History.vSource
-- ============================================================================
IF OBJECT_ID('History.vSource', 'U') IS NOT NULL DROP TABLE [History].[vSource]
GO

CREATE TABLE [History].[vSource] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [VI_SDK_UUID] NVARCHAR(100) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vSource_NaturalKey ON [History].[vSource](VI_SDK_Server, VI_SDK_UUID)
GO

PRINT 'Created [History].[vSource]'
GO

-- ============================================================================
-- History.vRP
-- ============================================================================
IF OBJECT_ID('History.vRP', 'U') IS NOT NULL DROP TABLE [History].[vRP]
GO

CREATE TABLE [History].[vRP] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

    [Resource_Pool_name] NVARCHAR(255) NULL,
    [Resource_Pool_path] NVARCHAR(1000) NULL,
    [Object_ID] NVARCHAR(100) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL,
    [Status] NVARCHAR(50) NULL,
    [VMs_total] INT NULL,
    [VMs] INT NULL,
    [vCPUs] INT NULL,
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
    [QS_swappedMemory] BIGINT NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vRP_NaturalKey ON [History].[vRP](Object_ID, VI_SDK_Server)
GO

PRINT 'Created [History].[vRP]'
GO

-- ============================================================================
-- History.vCluster
-- ============================================================================
IF OBJECT_ID('History.vCluster', 'U') IS NOT NULL DROP TABLE [History].[vCluster]
GO

CREATE TABLE [History].[vCluster] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [DRS_enabled] BIT NULL,
    [DRS_default_VM_behavior] NVARCHAR(50) NULL,
    [DRS_vmotion_rate] INT NULL,
    [DPM_enabled] BIT NULL,
    [DPM_default_behavior] NVARCHAR(50) NULL,
    [DPM_Host_Power_Action_Rate] INT NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vCluster_NaturalKey ON [History].[vCluster](Name, VI_SDK_Server)
GO

PRINT 'Created [History].[vCluster]'
GO

-- ============================================================================
-- History.vHost
-- ============================================================================
IF OBJECT_ID('History.vHost', 'U') IS NOT NULL DROP TABLE [History].[vHost]
GO

CREATE TABLE [History].[vHost] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [CPU_Model] NVARCHAR(255) NULL,
    [Speed] INT NULL,
    [HT_Available] BIT NULL,
    [HT_Active] BIT NULL,
    [Num_CPU] INT NULL,
    [Cores_per_CPU] INT NULL,
    [Num_Cores] INT NULL,
    [CPU_usage_Percent] DECIMAL(5,2) NULL,
    [Num_Memory] BIGINT NULL,
    [Memory_Tiering_Type] NVARCHAR(50) NULL,
    [Memory_usage_Percent] DECIMAL(5,2) NULL,
    [Console] BIGINT NULL,
    [Num_NICs] INT NULL,
    [Num_HBAs] INT NULL,
    [Num_VMs_total] INT NULL,
    [Num_VMs] INT NULL,
    [VMs_per_Core] DECIMAL(5,2) NULL,
    [Num_vCPUs] INT NULL,
    [vCPUs_per_Core] DECIMAL(5,2) NULL,
    [vRAM] BIGINT NULL,
    [VM_Used_memory] BIGINT NULL,
    [VM_Memory_Swapped] BIGINT NULL,
    [VM_Memory_Ballooned] BIGINT NULL,
    [VMotion_support] BIT NULL,
    [Storage_VMotion_support] BIT NULL,
    [Current_EVC] NVARCHAR(100) NULL,
    [Max_EVC] NVARCHAR(100) NULL,
    [Assigned_Licenses] NVARCHAR(500) NULL,
    [ATS_Heartbeat] NVARCHAR(50) NULL,
    [ATS_Locking] NVARCHAR(50) NULL,
    [Current_CPU_power_man_policy] NVARCHAR(100) NULL,
    [Supported_CPU_power_man] NVARCHAR(255) NULL,
    [Host_Power_Policy] NVARCHAR(100) NULL,
    [ESX_Version] NVARCHAR(100) NULL,
    [Boot_time] DATETIME2 NULL,
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
    [Vendor] NVARCHAR(100) NULL,
    [Model] NVARCHAR(255) NULL,
    [Serial_number] NVARCHAR(100) NULL,
    [Service_tag] NVARCHAR(100) NULL,
    [OEM_specific_string] NVARCHAR(500) NULL,
    [BIOS_Vendor] NVARCHAR(100) NULL,
    [BIOS_Version] NVARCHAR(50) NULL,
    [BIOS_Date] NVARCHAR(50) NULL,
    [Certificate_Issuer] NVARCHAR(500) NULL,
    [Certificate_Start_Date] DATETIME2 NULL,
    [Certificate_Expiry_Date] DATETIME2 NULL,
    [Certificate_Status] NVARCHAR(50) NULL,
    [Certificate_Subject] NVARCHAR(500) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vHost_NaturalKey ON [History].[vHost](Host, VI_SDK_Server)
CREATE NONCLUSTERED INDEX IX_History_vHost_ValidTo ON [History].[vHost](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vHost]'
GO

-- ============================================================================
-- History.vHBA
-- ============================================================================
IF OBJECT_ID('History.vHBA', 'U') IS NOT NULL DROP TABLE [History].[vHBA]
GO

CREATE TABLE [History].[vHBA] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [WWN] NVARCHAR(100) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vHBA_NaturalKey ON [History].[vHBA](Host, VI_SDK_Server, Device)
GO

PRINT 'Created [History].[vHBA]'
GO

-- ============================================================================
-- History.vNIC
-- ============================================================================
IF OBJECT_ID('History.vNIC', 'U') IS NOT NULL DROP TABLE [History].[vNIC]
GO

CREATE TABLE [History].[vNIC] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [WakeOn] NVARCHAR(50) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vNIC_NaturalKey ON [History].[vNIC](Host, VI_SDK_Server, Network_Device)
GO

PRINT 'Created [History].[vNIC]'
GO

-- ============================================================================
-- History.vSwitch
-- ============================================================================
IF OBJECT_ID('History.vSwitch', 'U') IS NOT NULL DROP TABLE [History].[vSwitch]
GO

CREATE TABLE [History].[vSwitch] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [MTU] INT NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vSwitch_NaturalKey ON [History].[vSwitch](Host, VI_SDK_Server, Switch)
GO

PRINT 'Created [History].[vSwitch]'
GO

-- ============================================================================
-- History.vPort
-- ============================================================================
IF OBJECT_ID('History.vPort', 'U') IS NOT NULL DROP TABLE [History].[vPort]
GO

CREATE TABLE [History].[vPort] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [Zero_Copy_Xmit] BIT NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vPort_NaturalKey ON [History].[vPort](Host, VI_SDK_Server, Port_Group, Switch)
GO

PRINT 'Created [History].[vPort]'
GO

-- ============================================================================
-- History.dvSwitch
-- ============================================================================
IF OBJECT_ID('History.dvSwitch', 'U') IS NOT NULL DROP TABLE [History].[dvSwitch]
GO

CREATE TABLE [History].[dvSwitch] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [In_Traffic_Shaping] BIT NULL,
    [In_Avg] BIGINT NULL,
    [In_Peak] BIGINT NULL,
    [In_Burst] BIGINT NULL,
    [Out_Traffic_Shaping] BIT NULL,
    [Out_Avg] BIGINT NULL,
    [Out_Peak] BIGINT NULL,
    [Out_Burst] BIGINT NULL,
    [CDP_Type] NVARCHAR(50) NULL,
    [CDP_Operation] NVARCHAR(50) NULL,
    [LACP_Name] NVARCHAR(100) NULL,
    [LACP_Mode] NVARCHAR(50) NULL,
    [LACP_Load_Balance_Alg] NVARCHAR(100) NULL,
    [Max_MTU] INT NULL,
    [Contact] NVARCHAR(255) NULL,
    [Admin_Name] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_dvSwitch_NaturalKey ON [History].[dvSwitch](Switch, VI_SDK_Server)
GO

PRINT 'Created [History].[dvSwitch]'
GO

-- ============================================================================
-- History.dvPort
-- ============================================================================
IF OBJECT_ID('History.dvPort', 'U') IS NOT NULL DROP TABLE [History].[dvPort]
GO

CREATE TABLE [History].[dvPort] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [Block_Override] BIT NULL,
    [Config_Reset] BIT NULL,
    [Shaping_Override] BIT NULL,
    [Vendor_Config_Override] BIT NULL,
    [Sec_Policy_Override] BIT NULL,
    [Teaming_Override] BIT NULL,
    [Vlan_Override] BIT NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_dvPort_NaturalKey ON [History].[dvPort](Port, Switch, VI_SDK_Server)
GO

PRINT 'Created [History].[dvPort]'
GO

-- ============================================================================
-- History.vSC_VMK
-- ============================================================================
IF OBJECT_ID('History.vSC_VMK', 'U') IS NOT NULL DROP TABLE [History].[vSC_VMK]
GO

CREATE TABLE [History].[vSC_VMK] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [MTU] INT NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vSC_VMK_NaturalKey ON [History].[vSC_VMK](Host, VI_SDK_Server, Device)
GO

PRINT 'Created [History].[vSC_VMK]'
GO

-- ============================================================================
-- History.vDatastore
-- ============================================================================
IF OBJECT_ID('History.vDatastore', 'U') IS NOT NULL DROP TABLE [History].[vDatastore]
GO

CREATE TABLE [History].[vDatastore] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [vSphereReplication] NVARCHAR(50) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vDatastore_NaturalKey ON [History].[vDatastore](Name, VI_SDK_Server)
CREATE NONCLUSTERED INDEX IX_History_vDatastore_ValidTo ON [History].[vDatastore](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vDatastore]'
GO

-- ============================================================================
-- History.vMultiPath
-- ============================================================================
IF OBJECT_ID('History.vMultiPath', 'U') IS NOT NULL DROP TABLE [History].[vMultiPath]
GO

CREATE TABLE [History].[vMultiPath] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

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
    [UUID] NVARCHAR(100) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vMultiPath_NaturalKey ON [History].[vMultiPath](Host, VI_SDK_Server, Disk)
GO

PRINT 'Created [History].[vMultiPath]'
GO

-- ============================================================================
-- History.vLicense
-- ============================================================================
IF OBJECT_ID('History.vLicense', 'U') IS NOT NULL DROP TABLE [History].[vLicense]
GO

CREATE TABLE [History].[vLicense] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

    [Name] NVARCHAR(255) NULL,
    [Key] NVARCHAR(100) NULL,
    [Labels] NVARCHAR(500) NULL,
    [Cost_Unit] NVARCHAR(50) NULL,
    [Total] INT NULL,
    [Used] INT NULL,
    [Expiration_Date] DATETIME2 NULL,
    [Features] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vLicense_NaturalKey ON [History].[vLicense]([Key], VI_SDK_Server)
GO

PRINT 'Created [History].[vLicense]'
GO

-- ============================================================================
-- History.vFileInfo
-- ============================================================================
IF OBJECT_ID('History.vFileInfo', 'U') IS NOT NULL DROP TABLE [History].[vFileInfo]
GO

CREATE TABLE [History].[vFileInfo] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

    [Friendly_Path_Name] NVARCHAR(1000) NULL,
    [File_Name] NVARCHAR(500) NULL,
    [File_Type] NVARCHAR(100) NULL,
    [File_Size_in_bytes] BIGINT NULL,
    [Path] NVARCHAR(1000) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vFileInfo_NaturalKey ON [History].[vFileInfo](Path, File_Name, VI_SDK_Server)
GO

PRINT 'Created [History].[vFileInfo]'
GO

-- ============================================================================
-- History.vHealth
-- ============================================================================
IF OBJECT_ID('History.vHealth', 'U') IS NOT NULL DROP TABLE [History].[vHealth]
GO

CREATE TABLE [History].[vHealth] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

    [Name] NVARCHAR(500) NULL,
    [Message] NVARCHAR(MAX) NULL,
    [Message_type] NVARCHAR(50) NULL,
    [VI_SDK_Server] NVARCHAR(255) NULL,
    [VI_SDK_UUID] NVARCHAR(100) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vHealth_NaturalKey ON [History].[vHealth](Name, Message_type, VI_SDK_Server)
CREATE NONCLUSTERED INDEX IX_History_vHealth_ValidTo ON [History].[vHealth](ValidTo) WHERE ValidTo IS NULL
GO

PRINT 'Created [History].[vHealth]'
GO

-- ============================================================================
-- History.vMetaData
-- ============================================================================
IF OBJECT_ID('History.vMetaData', 'U') IS NOT NULL DROP TABLE [History].[vMetaData]
GO

CREATE TABLE [History].[vMetaData] (
    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo DATETIME2 NULL,
    SourceFile NVARCHAR(500) NULL,

    [RVTools_major_version] INT NULL,
    [RVTools_version] NVARCHAR(50) NULL,
    [xlsx_creation_datetime] DATETIME2 NULL,
    [Server] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_History_vMetaData_NaturalKey ON [History].[vMetaData](Server, xlsx_creation_datetime)
GO

PRINT 'Created [History].[vMetaData]'
GO

PRINT '=============================================='
PRINT 'All 27 history tables created successfully!'
PRINT '=============================================='
GO
