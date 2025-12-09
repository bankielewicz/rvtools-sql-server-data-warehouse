/*
    RVTools Data Warehouse - Staging Tables

    Purpose: Creates all 27 staging tables for RVTools xlsx import
             All columns are NVARCHAR(MAX) to prevent import failures

    Notes:
    - Staging tables are truncated before each import
    - ImportBatchId and ImportRowNum added for tracking
    - Column names sanitized (spaces replaced with underscores)

    Usage: Execute against RVToolsDW database
           sqlcmd -S localhost -d RVToolsDW -i 001_AllStagingTables.sql
*/

USE [RVToolsDW]
GO

-- ============================================================================
-- Staging.vInfo (98 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vInfo', 'U') IS NOT NULL DROP TABLE [Staging].[vInfo]
GO

CREATE TABLE [Staging].[vInfo] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [Config_status] NVARCHAR(MAX) NULL,
    [DNS_Name] NVARCHAR(MAX) NULL,
    [Connection_state] NVARCHAR(MAX) NULL,
    [Guest_state] NVARCHAR(MAX) NULL,
    [Heartbeat] NVARCHAR(MAX) NULL,
    [Consolidation_Needed] NVARCHAR(MAX) NULL,
    [PowerOn] NVARCHAR(MAX) NULL,
    [Suspended_To_Memory] NVARCHAR(MAX) NULL,
    [Suspend_time] NVARCHAR(MAX) NULL,
    [Suspend_Interval] NVARCHAR(MAX) NULL,
    [Creation_date] NVARCHAR(MAX) NULL,
    [Change_Version] NVARCHAR(MAX) NULL,
    [CPUs] NVARCHAR(MAX) NULL,
    [Overall_Cpu_Readiness] NVARCHAR(MAX) NULL,
    [Memory] NVARCHAR(MAX) NULL,
    [Active_Memory] NVARCHAR(MAX) NULL,
    [NICs] NVARCHAR(MAX) NULL,
    [Disks] NVARCHAR(MAX) NULL,
    [Total_disk_capacity_MiB] NVARCHAR(MAX) NULL,
    [Fixed_Passthru_HotPlug] NVARCHAR(MAX) NULL,
    [min_Required_EVC_Mode_Key] NVARCHAR(MAX) NULL,
    [Latency_Sensitivity] NVARCHAR(MAX) NULL,
    [Op_Notification_Timeout] NVARCHAR(MAX) NULL,
    [EnableUUID] NVARCHAR(MAX) NULL,
    [CBT] NVARCHAR(MAX) NULL,
    [Primary_IP_Address] NVARCHAR(MAX) NULL,
    [Network_1] NVARCHAR(MAX) NULL,
    [Network_2] NVARCHAR(MAX) NULL,
    [Network_3] NVARCHAR(MAX) NULL,
    [Network_4] NVARCHAR(MAX) NULL,
    [Network_5] NVARCHAR(MAX) NULL,
    [Network_6] NVARCHAR(MAX) NULL,
    [Network_7] NVARCHAR(MAX) NULL,
    [Network_8] NVARCHAR(MAX) NULL,
    [Num_Monitors] NVARCHAR(MAX) NULL,
    [Video_Ram_KiB] NVARCHAR(MAX) NULL,
    [Resource_pool] NVARCHAR(MAX) NULL,
    [Folder_ID] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [vApp] NVARCHAR(MAX) NULL,
    [DAS_protection] NVARCHAR(MAX) NULL,
    [FT_State] NVARCHAR(MAX) NULL,
    [FT_Role] NVARCHAR(MAX) NULL,
    [FT_Latency] NVARCHAR(MAX) NULL,
    [FT_Bandwidth] NVARCHAR(MAX) NULL,
    [FT_Sec_Latency] NVARCHAR(MAX) NULL,
    [Vm_Failover_In_Progress] NVARCHAR(MAX) NULL,
    [Provisioned_MiB] NVARCHAR(MAX) NULL,
    [In_Use_MiB] NVARCHAR(MAX) NULL,
    [Unshared_MiB] NVARCHAR(MAX) NULL,
    [HA_Restart_Priority] NVARCHAR(MAX) NULL,
    [HA_Isolation_Response] NVARCHAR(MAX) NULL,
    [HA_VM_Monitoring] NVARCHAR(MAX) NULL,
    [Cluster_rules] NVARCHAR(MAX) NULL,
    [Cluster_rule_names] NVARCHAR(MAX) NULL,
    [Boot_Required] NVARCHAR(MAX) NULL,
    [Boot_delay] NVARCHAR(MAX) NULL,
    [Boot_retry_delay] NVARCHAR(MAX) NULL,
    [Boot_retry_enabled] NVARCHAR(MAX) NULL,
    [Boot_BIOS_setup] NVARCHAR(MAX) NULL,
    [Reboot_PowerOff] NVARCHAR(MAX) NULL,
    [EFI_Secure_boot] NVARCHAR(MAX) NULL,
    [Firmware] NVARCHAR(MAX) NULL,
    [HW_version] NVARCHAR(MAX) NULL,
    [HW_upgrade_status] NVARCHAR(MAX) NULL,
    [HW_upgrade_policy] NVARCHAR(MAX) NULL,
    [HW_target] NVARCHAR(MAX) NULL,
    [Path] NVARCHAR(MAX) NULL,
    [Log_directory] NVARCHAR(MAX) NULL,
    [Snapshot_directory] NVARCHAR(MAX) NULL,
    [Suspend_directory] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [Customization_Info] NVARCHAR(MAX) NULL,
    [Guest_Detailed_Data] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [SMBIOS_UUID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server_type] NVARCHAR(MAX) NULL,
    [VI_SDK_API_Version] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vInfo]'
GO

-- ============================================================================
-- Staging.vCPU (37 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vCPU', 'U') IS NOT NULL DROP TABLE [Staging].[vCPU]
GO

CREATE TABLE [Staging].[vCPU] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [CPUs] NVARCHAR(MAX) NULL,
    [Sockets] NVARCHAR(MAX) NULL,
    [Cores_per_socket] NVARCHAR(MAX) NULL,
    [Max] NVARCHAR(MAX) NULL,
    [Overall] NVARCHAR(MAX) NULL,
    [Level] NVARCHAR(MAX) NULL,
    [Shares] NVARCHAR(MAX) NULL,
    [Reservation] NVARCHAR(MAX) NULL,
    [Entitlement] NVARCHAR(MAX) NULL,
    [DRS_Entitlement] NVARCHAR(MAX) NULL,
    [Limit] NVARCHAR(MAX) NULL,
    [Hot_Add] NVARCHAR(MAX) NULL,
    [Hot_Remove] NVARCHAR(MAX) NULL,
    [Numa_Hotadd_Exposed] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vCPU]'
GO

-- ============================================================================
-- Staging.vMemory (41 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vMemory', 'U') IS NOT NULL DROP TABLE [Staging].[vMemory]
GO

CREATE TABLE [Staging].[vMemory] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [Size_MiB] NVARCHAR(MAX) NULL,
    [Memory_Reservation_Locked_To_Max] NVARCHAR(MAX) NULL,
    [Overhead] NVARCHAR(MAX) NULL,
    [Max] NVARCHAR(MAX) NULL,
    [Consumed] NVARCHAR(MAX) NULL,
    [Consumed_Overhead] NVARCHAR(MAX) NULL,
    [Private] NVARCHAR(MAX) NULL,
    [Shared] NVARCHAR(MAX) NULL,
    [Swapped] NVARCHAR(MAX) NULL,
    [Ballooned] NVARCHAR(MAX) NULL,
    [Active] NVARCHAR(MAX) NULL,
    [Entitlement] NVARCHAR(MAX) NULL,
    [DRS_Entitlement] NVARCHAR(MAX) NULL,
    [Level] NVARCHAR(MAX) NULL,
    [Shares] NVARCHAR(MAX) NULL,
    [Reservation] NVARCHAR(MAX) NULL,
    [Limit] NVARCHAR(MAX) NULL,
    [Hot_Add] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vMemory]'
GO

-- ============================================================================
-- Staging.vDisk (48 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vDisk', 'U') IS NOT NULL DROP TABLE [Staging].[vDisk]
GO

CREATE TABLE [Staging].[vDisk] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [Disk] NVARCHAR(MAX) NULL,
    [Disk_Key] NVARCHAR(MAX) NULL,
    [Disk_UUID] NVARCHAR(MAX) NULL,
    [Disk_Path] NVARCHAR(MAX) NULL,
    [Capacity_MiB] NVARCHAR(MAX) NULL,
    [Raw] NVARCHAR(MAX) NULL,
    [Disk_Mode] NVARCHAR(MAX) NULL,
    [Sharing_mode] NVARCHAR(MAX) NULL,
    [Thin] NVARCHAR(MAX) NULL,
    [Eagerly_Scrub] NVARCHAR(MAX) NULL,
    [Split] NVARCHAR(MAX) NULL,
    [Write_Through] NVARCHAR(MAX) NULL,
    [Level] NVARCHAR(MAX) NULL,
    [Shares] NVARCHAR(MAX) NULL,
    [Reservation] NVARCHAR(MAX) NULL,
    [Limit] NVARCHAR(MAX) NULL,
    [Controller] NVARCHAR(MAX) NULL,
    [Label] NVARCHAR(MAX) NULL,
    [SCSI_Unit_Num] NVARCHAR(MAX) NULL,
    [Unit_Num] NVARCHAR(MAX) NULL,
    [Shared_Bus] NVARCHAR(MAX) NULL,
    [Path] NVARCHAR(MAX) NULL,
    [Raw_LUN_ID] NVARCHAR(MAX) NULL,
    [Raw_Comp_Mode] NVARCHAR(MAX) NULL,
    [Internal_Sort_Column] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vDisk]'
GO

-- ============================================================================
-- Staging.vPartition (30 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vPartition', 'U') IS NOT NULL DROP TABLE [Staging].[vPartition]
GO

CREATE TABLE [Staging].[vPartition] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [Disk_Key] NVARCHAR(MAX) NULL,
    [Disk] NVARCHAR(MAX) NULL,
    [Capacity_MiB] NVARCHAR(MAX) NULL,
    [Consumed_MiB] NVARCHAR(MAX) NULL,
    [Free_MiB] NVARCHAR(MAX) NULL,
    [Free_Percent] NVARCHAR(MAX) NULL,
    [Internal_Sort_Column] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vPartition]'
GO

-- ============================================================================
-- Staging.vNetwork (35 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vNetwork', 'U') IS NOT NULL DROP TABLE [Staging].[vNetwork]
GO

CREATE TABLE [Staging].[vNetwork] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [NIC_label] NVARCHAR(MAX) NULL,
    [Adapter] NVARCHAR(MAX) NULL,
    [Network] NVARCHAR(MAX) NULL,
    [Switch] NVARCHAR(MAX) NULL,
    [Connected] NVARCHAR(MAX) NULL,
    [Starts_Connected] NVARCHAR(MAX) NULL,
    [Mac_Address] NVARCHAR(MAX) NULL,
    [Type] NVARCHAR(MAX) NULL,
    [IPv4_Address] NVARCHAR(MAX) NULL,
    [IPv6_Address] NVARCHAR(MAX) NULL,
    [Direct_Path_IO] NVARCHAR(MAX) NULL,
    [Internal_Sort_Column] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vNetwork]'
GO

-- ============================================================================
-- Staging.vCD (28 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vCD', 'U') IS NOT NULL DROP TABLE [Staging].[vCD]
GO

CREATE TABLE [Staging].[vCD] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [Device_Node] NVARCHAR(MAX) NULL,
    [Connected] NVARCHAR(MAX) NULL,
    [Starts_Connected] NVARCHAR(MAX) NULL,
    [Device_Type] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [VMRef] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vCD]'
GO

-- ============================================================================
-- Staging.vUSB (33 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vUSB', 'U') IS NOT NULL DROP TABLE [Staging].[vUSB]
GO

CREATE TABLE [Staging].[vUSB] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [Device_Node] NVARCHAR(MAX) NULL,
    [Device_Type] NVARCHAR(MAX) NULL,
    [Connected] NVARCHAR(MAX) NULL,
    [Family] NVARCHAR(MAX) NULL,
    [Speed] NVARCHAR(MAX) NULL,
    [EHCI_enabled] NVARCHAR(MAX) NULL,
    [Auto_connect] NVARCHAR(MAX) NULL,
    [Bus_number] NVARCHAR(MAX) NULL,
    [Unit_number] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_tools] NVARCHAR(MAX) NULL,
    [VMRef] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vUSB]'
GO

-- ============================================================================
-- Staging.vSnapshot (29 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vSnapshot', 'U') IS NOT NULL DROP TABLE [Staging].[vSnapshot]
GO

CREATE TABLE [Staging].[vSnapshot] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Date_time] NVARCHAR(MAX) NULL,
    [Filename] NVARCHAR(MAX) NULL,
    [Size_MiB_vmsn] NVARCHAR(MAX) NULL,
    [Size_MiB_total] NVARCHAR(MAX) NULL,
    [Quiesced] NVARCHAR(MAX) NULL,
    [State] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vSnapshot]'
GO

-- ============================================================================
-- Staging.vTools (37 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vTools', 'U') IS NOT NULL DROP TABLE [Staging].[vTools]
GO

CREATE TABLE [Staging].[vTools] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [VM] NVARCHAR(MAX) NULL,
    [Powerstate] NVARCHAR(MAX) NULL,
    [Template] NVARCHAR(MAX) NULL,
    [SRM_Placeholder] NVARCHAR(MAX) NULL,
    [VM_Version] NVARCHAR(MAX) NULL,
    [Tools] NVARCHAR(MAX) NULL,
    [Tools_Version] NVARCHAR(MAX) NULL,
    [Required_Version] NVARCHAR(MAX) NULL,
    [Upgradeable] NVARCHAR(MAX) NULL,
    [Upgrade_Policy] NVARCHAR(MAX) NULL,
    [Sync_time] NVARCHAR(MAX) NULL,
    [App_status] NVARCHAR(MAX) NULL,
    [Heartbeat_status] NVARCHAR(MAX) NULL,
    [Kernel_Crash_state] NVARCHAR(MAX) NULL,
    [Operation_Ready] NVARCHAR(MAX) NULL,
    [State_change_support] NVARCHAR(MAX) NULL,
    [Interactive_Guest] NVARCHAR(MAX) NULL,
    [Annotation] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Folder] NVARCHAR(MAX) NULL,
    [OS_according_to_the_configuration_file] NVARCHAR(MAX) NULL,
    [OS_according_to_the_VMware_Tools] NVARCHAR(MAX) NULL,
    [VMRef] NVARCHAR(MAX) NULL,
    [VM_ID] NVARCHAR(MAX) NULL,
    [VM_UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vTools]'
GO

-- ============================================================================
-- Staging.vSource (14 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vSource', 'U') IS NOT NULL DROP TABLE [Staging].[vSource]
GO

CREATE TABLE [Staging].[vSource] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Name] NVARCHAR(MAX) NULL,
    [OS_type] NVARCHAR(MAX) NULL,
    [API_type] NVARCHAR(MAX) NULL,
    [API_version] NVARCHAR(MAX) NULL,
    [Version] NVARCHAR(MAX) NULL,
    [Patch_level] NVARCHAR(MAX) NULL,
    [Build] NVARCHAR(MAX) NULL,
    [Fullname] NVARCHAR(MAX) NULL,
    [Product_name] NVARCHAR(MAX) NULL,
    [Product_version] NVARCHAR(MAX) NULL,
    [Product_line] NVARCHAR(MAX) NULL,
    [Vendor] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vSource]'
GO

-- ============================================================================
-- Staging.vRP (49 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vRP', 'U') IS NOT NULL DROP TABLE [Staging].[vRP]
GO

CREATE TABLE [Staging].[vRP] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Resource_Pool_name] NVARCHAR(MAX) NULL,
    [Resource_Pool_path] NVARCHAR(MAX) NULL,
    [Status] NVARCHAR(MAX) NULL,
    [VMs_total] NVARCHAR(MAX) NULL,
    [VMs] NVARCHAR(MAX) NULL,
    [vCPUs] NVARCHAR(MAX) NULL,
    [CPU_limit] NVARCHAR(MAX) NULL,
    [CPU_overheadLimit] NVARCHAR(MAX) NULL,
    [CPU_reservation] NVARCHAR(MAX) NULL,
    [CPU_level] NVARCHAR(MAX) NULL,
    [CPU_shares] NVARCHAR(MAX) NULL,
    [CPU_expandableReservation] NVARCHAR(MAX) NULL,
    [CPU_maxUsage] NVARCHAR(MAX) NULL,
    [CPU_overallUsage] NVARCHAR(MAX) NULL,
    [CPU_reservationUsed] NVARCHAR(MAX) NULL,
    [CPU_reservationUsedForVm] NVARCHAR(MAX) NULL,
    [CPU_unreservedForPool] NVARCHAR(MAX) NULL,
    [CPU_unreservedForVm] NVARCHAR(MAX) NULL,
    [Mem_Configured] NVARCHAR(MAX) NULL,
    [Mem_limit] NVARCHAR(MAX) NULL,
    [Mem_overheadLimit] NVARCHAR(MAX) NULL,
    [Mem_reservation] NVARCHAR(MAX) NULL,
    [Mem_level] NVARCHAR(MAX) NULL,
    [Mem_shares] NVARCHAR(MAX) NULL,
    [Mem_expandableReservation] NVARCHAR(MAX) NULL,
    [Mem_maxUsage] NVARCHAR(MAX) NULL,
    [Mem_overallUsage] NVARCHAR(MAX) NULL,
    [Mem_reservationUsed] NVARCHAR(MAX) NULL,
    [Mem_reservationUsedForVm] NVARCHAR(MAX) NULL,
    [Mem_unreservedForPool] NVARCHAR(MAX) NULL,
    [Mem_unreservedForVm] NVARCHAR(MAX) NULL,
    [QS_overallCpuDemand] NVARCHAR(MAX) NULL,
    [QS_overallCpuUsage] NVARCHAR(MAX) NULL,
    [QS_staticCpuEntitlement] NVARCHAR(MAX) NULL,
    [QS_distributedCpuEntitlement] NVARCHAR(MAX) NULL,
    [QS_balloonedMemory] NVARCHAR(MAX) NULL,
    [QS_compressedMemory] NVARCHAR(MAX) NULL,
    [QS_consumedOverheadMemory] NVARCHAR(MAX) NULL,
    [QS_distributedMemoryEntitlement] NVARCHAR(MAX) NULL,
    [QS_guestMemoryUsage] NVARCHAR(MAX) NULL,
    [QS_hostMemoryUsage] NVARCHAR(MAX) NULL,
    [QS_overheadMemory] NVARCHAR(MAX) NULL,
    [QS_privateMemory] NVARCHAR(MAX) NULL,
    [QS_sharedMemory] NVARCHAR(MAX) NULL,
    [QS_staticMemoryEntitlement] NVARCHAR(MAX) NULL,
    [QS_swappedMemory] NVARCHAR(MAX) NULL,
    [Object_ID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vRP]'
GO

-- ============================================================================
-- Staging.vCluster (36 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vCluster', 'U') IS NOT NULL DROP TABLE [Staging].[vCluster]
GO

CREATE TABLE [Staging].[vCluster] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Config_status] NVARCHAR(MAX) NULL,
    [OverallStatus] NVARCHAR(MAX) NULL,
    [NumHosts] NVARCHAR(MAX) NULL,
    [numEffectiveHosts] NVARCHAR(MAX) NULL,
    [TotalCpu] NVARCHAR(MAX) NULL,
    [NumCpuCores] NVARCHAR(MAX) NULL,
    [NumCpuThreads] NVARCHAR(MAX) NULL,
    [Effective_Cpu] NVARCHAR(MAX) NULL,
    [TotalMemory] NVARCHAR(MAX) NULL,
    [Effective_Memory] NVARCHAR(MAX) NULL,
    [Num_VMotions] NVARCHAR(MAX) NULL,
    [HA_enabled] NVARCHAR(MAX) NULL,
    [Failover_Level] NVARCHAR(MAX) NULL,
    [AdmissionControlEnabled] NVARCHAR(MAX) NULL,
    [Host_monitoring] NVARCHAR(MAX) NULL,
    [HB_Datastore_Candidate_Policy] NVARCHAR(MAX) NULL,
    [Isolation_Response] NVARCHAR(MAX) NULL,
    [Restart_Priority] NVARCHAR(MAX) NULL,
    [Cluster_Settings] NVARCHAR(MAX) NULL,
    [Max_Failures] NVARCHAR(MAX) NULL,
    [Max_Failure_Window] NVARCHAR(MAX) NULL,
    [Failure_Interval] NVARCHAR(MAX) NULL,
    [Min_Up_Time] NVARCHAR(MAX) NULL,
    [VM_Monitoring] NVARCHAR(MAX) NULL,
    [DRS_enabled] NVARCHAR(MAX) NULL,
    [DRS_default_VM_behavior] NVARCHAR(MAX) NULL,
    [DRS_vmotion_rate] NVARCHAR(MAX) NULL,
    [DPM_enabled] NVARCHAR(MAX) NULL,
    [DPM_default_behavior] NVARCHAR(MAX) NULL,
    [DPM_Host_Power_Action_Rate] NVARCHAR(MAX) NULL,
    [Object_ID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vCluster]'
GO

-- ============================================================================
-- Staging.vHost (71 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vHost', 'U') IS NOT NULL DROP TABLE [Staging].[vHost]
GO

CREATE TABLE [Staging].[vHost] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Config_status] NVARCHAR(MAX) NULL,
    [Compliance_Check_State] NVARCHAR(MAX) NULL,
    [in_Maintenance_Mode] NVARCHAR(MAX) NULL,
    [in_Quarantine_Mode] NVARCHAR(MAX) NULL,
    [vSAN_Fault_Domain_Name] NVARCHAR(MAX) NULL,
    [CPU_Model] NVARCHAR(MAX) NULL,
    [Speed] NVARCHAR(MAX) NULL,
    [HT_Available] NVARCHAR(MAX) NULL,
    [HT_Active] NVARCHAR(MAX) NULL,
    [Num_CPU] NVARCHAR(MAX) NULL,
    [Cores_per_CPU] NVARCHAR(MAX) NULL,
    [Num_Cores] NVARCHAR(MAX) NULL,
    [CPU_usage_Percent] NVARCHAR(MAX) NULL,
    [Num_Memory] NVARCHAR(MAX) NULL,
    [Memory_Tiering_Type] NVARCHAR(MAX) NULL,
    [Memory_usage_Percent] NVARCHAR(MAX) NULL,
    [Console] NVARCHAR(MAX) NULL,
    [Num_NICs] NVARCHAR(MAX) NULL,
    [Num_HBAs] NVARCHAR(MAX) NULL,
    [Num_VMs_total] NVARCHAR(MAX) NULL,
    [Num_VMs] NVARCHAR(MAX) NULL,
    [VMs_per_Core] NVARCHAR(MAX) NULL,
    [Num_vCPUs] NVARCHAR(MAX) NULL,
    [vCPUs_per_Core] NVARCHAR(MAX) NULL,
    [vRAM] NVARCHAR(MAX) NULL,
    [VM_Used_memory] NVARCHAR(MAX) NULL,
    [VM_Memory_Swapped] NVARCHAR(MAX) NULL,
    [VM_Memory_Ballooned] NVARCHAR(MAX) NULL,
    [VMotion_support] NVARCHAR(MAX) NULL,
    [Storage_VMotion_support] NVARCHAR(MAX) NULL,
    [Current_EVC] NVARCHAR(MAX) NULL,
    [Max_EVC] NVARCHAR(MAX) NULL,
    [Assigned_Licenses] NVARCHAR(MAX) NULL,
    [ATS_Heartbeat] NVARCHAR(MAX) NULL,
    [ATS_Locking] NVARCHAR(MAX) NULL,
    [Current_CPU_power_man_policy] NVARCHAR(MAX) NULL,
    [Supported_CPU_power_man] NVARCHAR(MAX) NULL,
    [Host_Power_Policy] NVARCHAR(MAX) NULL,
    [ESX_Version] NVARCHAR(MAX) NULL,
    [Boot_time] NVARCHAR(MAX) NULL,
    [DNS_Servers] NVARCHAR(MAX) NULL,
    [DHCP] NVARCHAR(MAX) NULL,
    [Domain] NVARCHAR(MAX) NULL,
    [Domain_List] NVARCHAR(MAX) NULL,
    [DNS_Search_Order] NVARCHAR(MAX) NULL,
    [NTP_Servers] NVARCHAR(MAX) NULL,
    [NTPD_running] NVARCHAR(MAX) NULL,
    [Time_Zone] NVARCHAR(MAX) NULL,
    [Time_Zone_Name] NVARCHAR(MAX) NULL,
    [GMT_Offset] NVARCHAR(MAX) NULL,
    [Vendor] NVARCHAR(MAX) NULL,
    [Model] NVARCHAR(MAX) NULL,
    [Serial_number] NVARCHAR(MAX) NULL,
    [Service_tag] NVARCHAR(MAX) NULL,
    [OEM_specific_string] NVARCHAR(MAX) NULL,
    [BIOS_Vendor] NVARCHAR(MAX) NULL,
    [BIOS_Version] NVARCHAR(MAX) NULL,
    [BIOS_Date] NVARCHAR(MAX) NULL,
    [Certificate_Issuer] NVARCHAR(MAX) NULL,
    [Certificate_Start_Date] NVARCHAR(MAX) NULL,
    [Certificate_Expiry_Date] NVARCHAR(MAX) NULL,
    [Certificate_Status] NVARCHAR(MAX) NULL,
    [Certificate_Subject] NVARCHAR(MAX) NULL,
    [Object_ID] NVARCHAR(MAX) NULL,
    [UUID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vHost]'
GO

-- ============================================================================
-- Staging.vHBA (13 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vHBA', 'U') IS NOT NULL DROP TABLE [Staging].[vHBA]
GO

CREATE TABLE [Staging].[vHBA] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Device] NVARCHAR(MAX) NULL,
    [Type] NVARCHAR(MAX) NULL,
    [Status] NVARCHAR(MAX) NULL,
    [Bus] NVARCHAR(MAX) NULL,
    [Pci] NVARCHAR(MAX) NULL,
    [Driver] NVARCHAR(MAX) NULL,
    [Model] NVARCHAR(MAX) NULL,
    [WWN] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vHBA]'
GO

-- ============================================================================
-- Staging.vNIC (14 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vNIC', 'U') IS NOT NULL DROP TABLE [Staging].[vNIC]
GO

CREATE TABLE [Staging].[vNIC] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Network_Device] NVARCHAR(MAX) NULL,
    [Driver] NVARCHAR(MAX) NULL,
    [Speed] NVARCHAR(MAX) NULL,
    [Duplex] NVARCHAR(MAX) NULL,
    [MAC] NVARCHAR(MAX) NULL,
    [Switch] NVARCHAR(MAX) NULL,
    [Uplink_port] NVARCHAR(MAX) NULL,
    [PCI] NVARCHAR(MAX) NULL,
    [WakeOn] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vNIC]'
GO

-- ============================================================================
-- Staging.vSwitch (23 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vSwitch', 'U') IS NOT NULL DROP TABLE [Staging].[vSwitch]
GO

CREATE TABLE [Staging].[vSwitch] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Switch] NVARCHAR(MAX) NULL,
    [Num_Ports] NVARCHAR(MAX) NULL,
    [Free_Ports] NVARCHAR(MAX) NULL,
    [Promiscuous_Mode] NVARCHAR(MAX) NULL,
    [Mac_Changes] NVARCHAR(MAX) NULL,
    [Forged_Transmits] NVARCHAR(MAX) NULL,
    [Traffic_Shaping] NVARCHAR(MAX) NULL,
    [Width] NVARCHAR(MAX) NULL,
    [Peak] NVARCHAR(MAX) NULL,
    [Burst] NVARCHAR(MAX) NULL,
    [Policy] NVARCHAR(MAX) NULL,
    [Reverse_Policy] NVARCHAR(MAX) NULL,
    [Notify_Switch] NVARCHAR(MAX) NULL,
    [Rolling_Order] NVARCHAR(MAX) NULL,
    [Offload] NVARCHAR(MAX) NULL,
    [TSO] NVARCHAR(MAX) NULL,
    [Zero_Copy_Xmit] NVARCHAR(MAX) NULL,
    [MTU] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vSwitch]'
GO

-- ============================================================================
-- Staging.vPort (22 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vPort', 'U') IS NOT NULL DROP TABLE [Staging].[vPort]
GO

CREATE TABLE [Staging].[vPort] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Port_Group] NVARCHAR(MAX) NULL,
    [Switch] NVARCHAR(MAX) NULL,
    [VLAN] NVARCHAR(MAX) NULL,
    [Promiscuous_Mode] NVARCHAR(MAX) NULL,
    [Mac_Changes] NVARCHAR(MAX) NULL,
    [Forged_Transmits] NVARCHAR(MAX) NULL,
    [Traffic_Shaping] NVARCHAR(MAX) NULL,
    [Width] NVARCHAR(MAX) NULL,
    [Peak] NVARCHAR(MAX) NULL,
    [Burst] NVARCHAR(MAX) NULL,
    [Policy] NVARCHAR(MAX) NULL,
    [Reverse_Policy] NVARCHAR(MAX) NULL,
    [Notify_Switch] NVARCHAR(MAX) NULL,
    [Rolling_Order] NVARCHAR(MAX) NULL,
    [Offload] NVARCHAR(MAX) NULL,
    [TSO] NVARCHAR(MAX) NULL,
    [Zero_Copy_Xmit] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vPort]'
GO

-- ============================================================================
-- Staging.dvSwitch (30 columns)
-- ============================================================================
IF OBJECT_ID('Staging.dvSwitch', 'U') IS NOT NULL DROP TABLE [Staging].[dvSwitch]
GO

CREATE TABLE [Staging].[dvSwitch] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Switch] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Vendor] NVARCHAR(MAX) NULL,
    [Version] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Created] NVARCHAR(MAX) NULL,
    [Host_members] NVARCHAR(MAX) NULL,
    [Max_Ports] NVARCHAR(MAX) NULL,
    [Num_Ports] NVARCHAR(MAX) NULL,
    [Num_VMs] NVARCHAR(MAX) NULL,
    [In_Traffic_Shaping] NVARCHAR(MAX) NULL,
    [In_Avg] NVARCHAR(MAX) NULL,
    [In_Peak] NVARCHAR(MAX) NULL,
    [In_Burst] NVARCHAR(MAX) NULL,
    [Out_Traffic_Shaping] NVARCHAR(MAX) NULL,
    [Out_Avg] NVARCHAR(MAX) NULL,
    [Out_Peak] NVARCHAR(MAX) NULL,
    [Out_Burst] NVARCHAR(MAX) NULL,
    [CDP_Type] NVARCHAR(MAX) NULL,
    [CDP_Operation] NVARCHAR(MAX) NULL,
    [LACP_Name] NVARCHAR(MAX) NULL,
    [LACP_Mode] NVARCHAR(MAX) NULL,
    [LACP_Load_Balance_Alg] NVARCHAR(MAX) NULL,
    [Max_MTU] NVARCHAR(MAX) NULL,
    [Contact] NVARCHAR(MAX) NULL,
    [Admin_Name] NVARCHAR(MAX) NULL,
    [Object_ID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[dvSwitch]'
GO

-- ============================================================================
-- Staging.dvPort (41 columns)
-- ============================================================================
IF OBJECT_ID('Staging.dvPort', 'U') IS NOT NULL DROP TABLE [Staging].[dvPort]
GO

CREATE TABLE [Staging].[dvPort] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Port] NVARCHAR(MAX) NULL,
    [Switch] NVARCHAR(MAX) NULL,
    [Type] NVARCHAR(MAX) NULL,
    [Num_Ports] NVARCHAR(MAX) NULL,
    [VLAN] NVARCHAR(MAX) NULL,
    [Speed] NVARCHAR(MAX) NULL,
    [Full_Duplex] NVARCHAR(MAX) NULL,
    [Blocked] NVARCHAR(MAX) NULL,
    [Allow_Promiscuous] NVARCHAR(MAX) NULL,
    [Mac_Changes] NVARCHAR(MAX) NULL,
    [Active_Uplink] NVARCHAR(MAX) NULL,
    [Standby_Uplink] NVARCHAR(MAX) NULL,
    [Policy] NVARCHAR(MAX) NULL,
    [Forged_Transmits] NVARCHAR(MAX) NULL,
    [In_Traffic_Shaping] NVARCHAR(MAX) NULL,
    [In_Avg] NVARCHAR(MAX) NULL,
    [In_Peak] NVARCHAR(MAX) NULL,
    [In_Burst] NVARCHAR(MAX) NULL,
    [Out_Traffic_Shaping] NVARCHAR(MAX) NULL,
    [Out_Avg] NVARCHAR(MAX) NULL,
    [Out_Peak] NVARCHAR(MAX) NULL,
    [Out_Burst] NVARCHAR(MAX) NULL,
    [Reverse_Policy] NVARCHAR(MAX) NULL,
    [Notify_Switch] NVARCHAR(MAX) NULL,
    [Rolling_Order] NVARCHAR(MAX) NULL,
    [Check_Beacon] NVARCHAR(MAX) NULL,
    [Live_Port_Moving] NVARCHAR(MAX) NULL,
    [Check_Duplex] NVARCHAR(MAX) NULL,
    [Check_Error_Percent] NVARCHAR(MAX) NULL,
    [Check_Speed] NVARCHAR(MAX) NULL,
    [Percentage] NVARCHAR(MAX) NULL,
    [Block_Override] NVARCHAR(MAX) NULL,
    [Config_Reset] NVARCHAR(MAX) NULL,
    [Shaping_Override] NVARCHAR(MAX) NULL,
    [Vendor_Config_Override] NVARCHAR(MAX) NULL,
    [Sec_Policy_Override] NVARCHAR(MAX) NULL,
    [Teaming_Override] NVARCHAR(MAX) NULL,
    [Vlan_Override] NVARCHAR(MAX) NULL,
    [Object_ID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[dvPort]'
GO

-- ============================================================================
-- Staging.vSC_VMK (15 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vSC_VMK', 'U') IS NOT NULL DROP TABLE [Staging].[vSC_VMK]
GO

CREATE TABLE [Staging].[vSC_VMK] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Port_Group] NVARCHAR(MAX) NULL,
    [Device] NVARCHAR(MAX) NULL,
    [Mac_Address] NVARCHAR(MAX) NULL,
    [DHCP] NVARCHAR(MAX) NULL,
    [IP_Address] NVARCHAR(MAX) NULL,
    [IP_6_Address] NVARCHAR(MAX) NULL,
    [Subnet_mask] NVARCHAR(MAX) NULL,
    [Gateway] NVARCHAR(MAX) NULL,
    [IP_6_Gateway] NVARCHAR(MAX) NULL,
    [MTU] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vSC_VMK]'
GO

-- ============================================================================
-- Staging.vDatastore (31 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vDatastore', 'U') IS NOT NULL DROP TABLE [Staging].[vDatastore]
GO

CREATE TABLE [Staging].[vDatastore] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Config_status] NVARCHAR(MAX) NULL,
    [Address] NVARCHAR(MAX) NULL,
    [Accessible] NVARCHAR(MAX) NULL,
    [Type] NVARCHAR(MAX) NULL,
    [Num_VMs_total] NVARCHAR(MAX) NULL,
    [Num_VMs] NVARCHAR(MAX) NULL,
    [Capacity_MiB] NVARCHAR(MAX) NULL,
    [Provisioned_MiB] NVARCHAR(MAX) NULL,
    [In_Use_MiB] NVARCHAR(MAX) NULL,
    [Free_MiB] NVARCHAR(MAX) NULL,
    [Free_Percent] NVARCHAR(MAX) NULL,
    [SIOC_enabled] NVARCHAR(MAX) NULL,
    [SIOC_Threshold] NVARCHAR(MAX) NULL,
    [Num_Hosts] NVARCHAR(MAX) NULL,
    [Hosts] NVARCHAR(MAX) NULL,
    [Cluster_name] NVARCHAR(MAX) NULL,
    [Cluster_capacity_MiB] NVARCHAR(MAX) NULL,
    [Cluster_free_space_MiB] NVARCHAR(MAX) NULL,
    [Block_size] NVARCHAR(MAX) NULL,
    [Max_Blocks] NVARCHAR(MAX) NULL,
    [Num_Extents] NVARCHAR(MAX) NULL,
    [Major_Version] NVARCHAR(MAX) NULL,
    [Version] NVARCHAR(MAX) NULL,
    [VMFS_Upgradeable] NVARCHAR(MAX) NULL,
    [MHA] NVARCHAR(MAX) NULL,
    [URL] NVARCHAR(MAX) NULL,
    [Object_ID] NVARCHAR(MAX) NULL,
    [vSphereReplication] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vDatastore]'
GO

-- ============================================================================
-- Staging.vMultiPath (35 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vMultiPath', 'U') IS NOT NULL DROP TABLE [Staging].[vMultiPath]
GO

CREATE TABLE [Staging].[vMultiPath] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Host] NVARCHAR(MAX) NULL,
    [Cluster] NVARCHAR(MAX) NULL,
    [Datacenter] NVARCHAR(MAX) NULL,
    [Datastore] NVARCHAR(MAX) NULL,
    [Disk] NVARCHAR(MAX) NULL,
    [Display_name] NVARCHAR(MAX) NULL,
    [Policy] NVARCHAR(MAX) NULL,
    [Oper_State] NVARCHAR(MAX) NULL,
    [Path_1] NVARCHAR(MAX) NULL,
    [Path_1_state] NVARCHAR(MAX) NULL,
    [Path_2] NVARCHAR(MAX) NULL,
    [Path_2_state] NVARCHAR(MAX) NULL,
    [Path_3] NVARCHAR(MAX) NULL,
    [Path_3_state] NVARCHAR(MAX) NULL,
    [Path_4] NVARCHAR(MAX) NULL,
    [Path_4_state] NVARCHAR(MAX) NULL,
    [Path_5] NVARCHAR(MAX) NULL,
    [Path_5_state] NVARCHAR(MAX) NULL,
    [Path_6] NVARCHAR(MAX) NULL,
    [Path_6_state] NVARCHAR(MAX) NULL,
    [Path_7] NVARCHAR(MAX) NULL,
    [Path_7_state] NVARCHAR(MAX) NULL,
    [Path_8] NVARCHAR(MAX) NULL,
    [Path_8_state] NVARCHAR(MAX) NULL,
    [vStorage] NVARCHAR(MAX) NULL,
    [Queue_depth] NVARCHAR(MAX) NULL,
    [Vendor] NVARCHAR(MAX) NULL,
    [Model] NVARCHAR(MAX) NULL,
    [Revision] NVARCHAR(MAX) NULL,
    [Level] NVARCHAR(MAX) NULL,
    [Serial_Num] NVARCHAR(MAX) NULL,
    [UUID] NVARCHAR(MAX) NULL,
    [Object_ID] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vMultiPath]'
GO

-- ============================================================================
-- Staging.vLicense (10 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vLicense', 'U') IS NOT NULL DROP TABLE [Staging].[vLicense]
GO

CREATE TABLE [Staging].[vLicense] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Key] NVARCHAR(MAX) NULL,
    [Labels] NVARCHAR(MAX) NULL,
    [Cost_Unit] NVARCHAR(MAX) NULL,
    [Total] NVARCHAR(MAX) NULL,
    [Used] NVARCHAR(MAX) NULL,
    [Expiration_Date] NVARCHAR(MAX) NULL,
    [Features] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vLicense]'
GO

-- ============================================================================
-- Staging.vFileInfo (8 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vFileInfo', 'U') IS NOT NULL DROP TABLE [Staging].[vFileInfo]
GO

CREATE TABLE [Staging].[vFileInfo] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Friendly_Path_Name] NVARCHAR(MAX) NULL,
    [File_Name] NVARCHAR(MAX) NULL,
    [File_Type] NVARCHAR(MAX) NULL,
    [File_Size_in_bytes] NVARCHAR(MAX) NULL,
    [Path] NVARCHAR(MAX) NULL,
    [Internal_Sort_Column] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vFileInfo]'
GO

-- ============================================================================
-- Staging.vHealth (5 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vHealth', 'U') IS NOT NULL DROP TABLE [Staging].[vHealth]
GO

CREATE TABLE [Staging].[vHealth] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Message] NVARCHAR(MAX) NULL,
    [Message_type] NVARCHAR(MAX) NULL,
    [VI_SDK_Server] NVARCHAR(MAX) NULL,
    [VI_SDK_UUID] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vHealth]'
GO

-- ============================================================================
-- Staging.vMetaData (4 columns)
-- ============================================================================
IF OBJECT_ID('Staging.vMetaData', 'U') IS NOT NULL DROP TABLE [Staging].[vMetaData]
GO

CREATE TABLE [Staging].[vMetaData] (
    StagingId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportBatchId INT NOT NULL,
    ImportRowNum INT NOT NULL,
    [RVTools_major_version] NVARCHAR(MAX) NULL,
    [RVTools_version] NVARCHAR(MAX) NULL,
    [xlsx_creation_datetime] NVARCHAR(MAX) NULL,
    [Server] NVARCHAR(MAX) NULL
)
GO

PRINT 'Created [Staging].[vMetaData]'
GO

PRINT '=============================================='
PRINT 'All 27 staging tables created successfully!'
PRINT '=============================================='
GO
