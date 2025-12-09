/*
    RVTools Data Warehouse - Merge vInfo Table

    Purpose: Merges staging vInfo data to Current and History tables
             Implements SCD Type 2 change tracking

    Process:
        1. Close out changed/deleted records in History (set ValidTo)
        2. Insert new versions to History
        3. Merge staging to Current (upsert)

    Natural Key: VM_UUID + VI_SDK_Server
*/

USE [RVToolsDW]
GO

IF OBJECT_ID('dbo.usp_MergeTable_vInfo', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_MergeTable_vInfo]
GO

CREATE PROCEDURE [dbo].[usp_MergeTable_vInfo]
    @ImportBatchId INT,
    @SourceFile NVARCHAR(500) = NULL,
    @MergedCount INT OUTPUT,
    @ArchivedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2 = GETUTCDATE()
    DECLARE @RowsAffected INT = 0

    -- ================================================================
    -- Step 1: Close out records in History that have changed or been deleted
    -- ================================================================
    UPDATE h
    SET h.ValidTo = @Now
    FROM [History].[vInfo] h
    INNER JOIN [Current].[vInfo] c
        ON h.VM_UUID = c.VM_UUID
        AND h.VI_SDK_Server = c.VI_SDK_Server
        AND h.ValidTo IS NULL
    WHERE NOT EXISTS (
        -- Record still exists in staging with same key values
        SELECT 1 FROM [Staging].[vInfo] s
        WHERE s.ImportBatchId = @ImportBatchId
        AND s.VM_UUID = c.VM_UUID
        AND s.VI_SDK_Server = c.VI_SDK_Server
    )
    OR EXISTS (
        -- Record exists but has changed
        SELECT 1 FROM [Staging].[vInfo] s
        WHERE s.ImportBatchId = @ImportBatchId
        AND s.VM_UUID = c.VM_UUID
        AND s.VI_SDK_Server = c.VI_SDK_Server
        AND (
            ISNULL(s.VM, '') <> ISNULL(c.VM, '') OR
            ISNULL(s.Powerstate, '') <> ISNULL(c.Powerstate, '') OR
            ISNULL(TRY_CAST(s.CPUs AS INT), 0) <> ISNULL(c.CPUs, 0) OR
            ISNULL(TRY_CAST(s.Memory AS BIGINT), 0) <> ISNULL(c.Memory, 0) OR
            ISNULL(s.Host, '') <> ISNULL(c.Host, '') OR
            ISNULL(s.Cluster, '') <> ISNULL(c.Cluster, '') OR
            ISNULL(s.Datacenter, '') <> ISNULL(c.Datacenter, '') OR
            ISNULL(TRY_CAST(s.Provisioned_MiB AS BIGINT), 0) <> ISNULL(c.Provisioned_MiB, 0) OR
            ISNULL(TRY_CAST(s.In_Use_MiB AS BIGINT), 0) <> ISNULL(c.In_Use_MiB, 0)
        )
    )

    SET @ArchivedCount = @@ROWCOUNT

    -- ================================================================
    -- Step 2: Insert new history records for changed/new items
    -- ================================================================
    INSERT INTO [History].[vInfo] (
        ImportBatchId, ValidFrom, ValidTo, SourceFile,
        VM, VM_UUID, VM_ID, SMBIOS_UUID, VI_SDK_Server, VI_SDK_UUID,
        VI_SDK_Server_type, VI_SDK_API_Version,
        Powerstate, Template, SRM_Placeholder, Config_status,
        Connection_state, Guest_state, Heartbeat, Consolidation_Needed,
        PowerOn, Suspended_To_Memory, Suspend_time, Suspend_Interval,
        Creation_date, Change_Version, CPUs, Overall_Cpu_Readiness,
        Memory, Active_Memory, NICs, Disks, Total_disk_capacity_MiB,
        Fixed_Passthru_HotPlug, min_Required_EVC_Mode_Key, Latency_Sensitivity,
        Op_Notification_Timeout, EnableUUID, CBT, Primary_IP_Address,
        Network_1, Network_2, Network_3, Network_4,
        Network_5, Network_6, Network_7, Network_8,
        Num_Monitors, Video_Ram_KiB, Resource_pool, Folder_ID, Folder,
        vApp, Datacenter, Cluster, Host,
        DAS_protection, FT_State, FT_Role, FT_Latency, FT_Bandwidth,
        FT_Sec_Latency, Vm_Failover_In_Progress,
        Provisioned_MiB, In_Use_MiB, Unshared_MiB,
        HA_Restart_Priority, HA_Isolation_Response, HA_VM_Monitoring,
        Cluster_rules, Cluster_rule_names,
        Boot_Required, Boot_delay, Boot_retry_delay, Boot_retry_enabled,
        Boot_BIOS_setup, Reboot_PowerOff, EFI_Secure_boot, Firmware,
        HW_version, HW_upgrade_status, HW_upgrade_policy, HW_target,
        Path, Log_directory, Snapshot_directory, Suspend_directory,
        Annotation, OS_according_to_the_configuration_file,
        OS_according_to_the_VMware_Tools, Customization_Info,
        Guest_Detailed_Data, DNS_Name
    )
    SELECT
        @ImportBatchId, @Now, NULL, @SourceFile,
        s.VM,
        s.VM_UUID,
        s.VM_ID,
        s.SMBIOS_UUID,
        s.VI_SDK_Server,
        s.VI_SDK_UUID,
        s.VI_SDK_Server_type,
        s.VI_SDK_API_Version,
        s.Powerstate,
        CASE WHEN s.Template IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        CASE WHEN s.SRM_Placeholder IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        s.Config_status,
        s.Connection_state,
        s.Guest_state,
        s.Heartbeat,
        CASE WHEN s.Consolidation_Needed IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        TRY_CAST(s.PowerOn AS DATETIME2),
        CASE WHEN s.Suspended_To_Memory IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        TRY_CAST(s.Suspend_time AS DATETIME2),
        s.Suspend_Interval,
        TRY_CAST(s.Creation_date AS DATETIME2),
        s.Change_Version,
        TRY_CAST(s.CPUs AS INT),
        TRY_CAST(s.[Overall_Cpu_Readiness] AS DECIMAL(10,2)),
        TRY_CAST(s.Memory AS BIGINT),
        TRY_CAST(s.Active_Memory AS BIGINT),
        TRY_CAST(s.NICs AS INT),
        TRY_CAST(s.Disks AS INT),
        TRY_CAST(s.Total_disk_capacity_MiB AS BIGINT),
        CASE WHEN s.Fixed_Passthru_HotPlug IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        s.min_Required_EVC_Mode_Key,
        s.Latency_Sensitivity,
        TRY_CAST(s.Op_Notification_Timeout AS INT),
        CASE WHEN s.EnableUUID IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        CASE WHEN s.CBT IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        s.Primary_IP_Address,
        s.Network_1, s.Network_2, s.Network_3, s.Network_4,
        s.Network_5, s.Network_6, s.Network_7, s.Network_8,
        TRY_CAST(s.Num_Monitors AS INT),
        TRY_CAST(s.Video_Ram_KiB AS INT),
        s.Resource_pool,
        s.Folder_ID,
        s.Folder,
        s.vApp,
        s.Datacenter,
        s.Cluster,
        s.Host,
        s.DAS_protection,
        s.FT_State,
        s.FT_Role,
        TRY_CAST(s.FT_Latency AS INT),
        TRY_CAST(s.FT_Bandwidth AS INT),
        TRY_CAST(s.FT_Sec_Latency AS INT),
        CASE WHEN s.Vm_Failover_In_Progress IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        TRY_CAST(s.Provisioned_MiB AS BIGINT),
        TRY_CAST(s.In_Use_MiB AS BIGINT),
        TRY_CAST(s.Unshared_MiB AS BIGINT),
        s.HA_Restart_Priority,
        s.HA_Isolation_Response,
        s.HA_VM_Monitoring,
        s.Cluster_rules,
        s.Cluster_rule_names,
        CASE WHEN s.Boot_Required IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        TRY_CAST(s.Boot_delay AS INT),
        TRY_CAST(s.Boot_retry_delay AS INT),
        CASE WHEN s.Boot_retry_enabled IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        CASE WHEN s.Boot_BIOS_setup IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        CASE WHEN s.Reboot_PowerOff IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        CASE WHEN s.EFI_Secure_boot IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
        s.Firmware,
        s.HW_version,
        s.HW_upgrade_status,
        s.HW_upgrade_policy,
        s.HW_target,
        s.Path,
        s.Log_directory,
        s.Snapshot_directory,
        s.Suspend_directory,
        s.Annotation,
        s.OS_according_to_the_configuration_file,
        s.OS_according_to_the_VMware_Tools,
        s.Customization_Info,
        s.Guest_Detailed_Data,
        s.DNS_Name
    FROM [Staging].[vInfo] s
    WHERE s.ImportBatchId = @ImportBatchId
    AND s.VM_UUID IS NOT NULL
    AND s.VI_SDK_Server IS NOT NULL

    -- ================================================================
    -- Step 3: Merge to Current table
    -- ================================================================
    MERGE [Current].[vInfo] AS target
    USING (
        SELECT * FROM [Staging].[vInfo]
        WHERE ImportBatchId = @ImportBatchId
        AND VM_UUID IS NOT NULL
        AND VI_SDK_Server IS NOT NULL
    ) AS source
    ON target.VM_UUID = source.VM_UUID
    AND target.VI_SDK_Server = source.VI_SDK_Server

    WHEN MATCHED THEN
        UPDATE SET
            ImportBatchId = @ImportBatchId,
            LastModifiedDate = @Now,
            VM = source.VM,
            VM_ID = source.VM_ID,
            SMBIOS_UUID = source.SMBIOS_UUID,
            VI_SDK_UUID = source.VI_SDK_UUID,
            VI_SDK_Server_type = source.VI_SDK_Server_type,
            VI_SDK_API_Version = source.VI_SDK_API_Version,
            Powerstate = source.Powerstate,
            Template = CASE WHEN source.Template IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
            SRM_Placeholder = CASE WHEN source.SRM_Placeholder IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
            Config_status = source.Config_status,
            Connection_state = source.Connection_state,
            Guest_state = source.Guest_state,
            Heartbeat = source.Heartbeat,
            Consolidation_Needed = CASE WHEN source.Consolidation_Needed IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
            PowerOn = TRY_CAST(source.PowerOn AS DATETIME2),
            Creation_date = TRY_CAST(source.Creation_date AS DATETIME2),
            CPUs = TRY_CAST(source.CPUs AS INT),
            Memory = TRY_CAST(source.Memory AS BIGINT),
            NICs = TRY_CAST(source.NICs AS INT),
            Disks = TRY_CAST(source.Disks AS INT),
            Total_disk_capacity_MiB = TRY_CAST(source.Total_disk_capacity_MiB AS BIGINT),
            Primary_IP_Address = source.Primary_IP_Address,
            Network_1 = source.Network_1,
            Network_2 = source.Network_2,
            Network_3 = source.Network_3,
            Network_4 = source.Network_4,
            Resource_pool = source.Resource_pool,
            Folder = source.Folder,
            vApp = source.vApp,
            Datacenter = source.Datacenter,
            Cluster = source.Cluster,
            Host = source.Host,
            Provisioned_MiB = TRY_CAST(source.Provisioned_MiB AS BIGINT),
            In_Use_MiB = TRY_CAST(source.In_Use_MiB AS BIGINT),
            Unshared_MiB = TRY_CAST(source.Unshared_MiB AS BIGINT),
            HW_version = source.HW_version,
            Firmware = source.Firmware,
            Path = source.Path,
            Annotation = source.Annotation,
            OS_according_to_the_configuration_file = source.OS_according_to_the_configuration_file,
            OS_according_to_the_VMware_Tools = source.OS_according_to_the_VMware_Tools,
            DNS_Name = source.DNS_Name

    WHEN NOT MATCHED BY TARGET THEN
        INSERT (
            ImportBatchId, LastModifiedDate,
            VM, VM_UUID, VM_ID, SMBIOS_UUID, VI_SDK_Server, VI_SDK_UUID,
            VI_SDK_Server_type, VI_SDK_API_Version,
            Powerstate, Template, SRM_Placeholder, Config_status,
            Connection_state, Guest_state, Heartbeat, Consolidation_Needed,
            PowerOn, Creation_date, CPUs, Memory, NICs, Disks,
            Total_disk_capacity_MiB, Primary_IP_Address,
            Network_1, Network_2, Network_3, Network_4,
            Resource_pool, Folder, vApp, Datacenter, Cluster, Host,
            Provisioned_MiB, In_Use_MiB, Unshared_MiB,
            HW_version, Firmware, Path, Annotation,
            OS_according_to_the_configuration_file, OS_according_to_the_VMware_Tools, DNS_Name
        )
        VALUES (
            @ImportBatchId, @Now,
            source.VM, source.VM_UUID, source.VM_ID, source.SMBIOS_UUID,
            source.VI_SDK_Server, source.VI_SDK_UUID,
            source.VI_SDK_Server_type, source.VI_SDK_API_Version,
            source.Powerstate,
            CASE WHEN source.Template IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
            CASE WHEN source.SRM_Placeholder IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
            source.Config_status, source.Connection_state, source.Guest_state,
            source.Heartbeat,
            CASE WHEN source.Consolidation_Needed IN ('True', '1', 'Yes') THEN 1 ELSE 0 END,
            TRY_CAST(source.PowerOn AS DATETIME2),
            TRY_CAST(source.Creation_date AS DATETIME2),
            TRY_CAST(source.CPUs AS INT),
            TRY_CAST(source.Memory AS BIGINT),
            TRY_CAST(source.NICs AS INT),
            TRY_CAST(source.Disks AS INT),
            TRY_CAST(source.Total_disk_capacity_MiB AS BIGINT),
            source.Primary_IP_Address,
            source.Network_1, source.Network_2, source.Network_3, source.Network_4,
            source.Resource_pool, source.Folder, source.vApp,
            source.Datacenter, source.Cluster, source.Host,
            TRY_CAST(source.Provisioned_MiB AS BIGINT),
            TRY_CAST(source.In_Use_MiB AS BIGINT),
            TRY_CAST(source.Unshared_MiB AS BIGINT),
            source.HW_version, source.Firmware, source.Path, source.Annotation,
            source.OS_according_to_the_configuration_file,
            source.OS_according_to_the_VMware_Tools, source.DNS_Name
        )

    WHEN NOT MATCHED BY SOURCE THEN
        DELETE;

    SET @MergedCount = @@ROWCOUNT

END
GO

PRINT 'Created [dbo].[usp_MergeTable_vInfo]'
GO
