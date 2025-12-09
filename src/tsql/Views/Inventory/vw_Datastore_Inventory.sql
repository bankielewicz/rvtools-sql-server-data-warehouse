/*
    RVTools Data Warehouse - Datastore Inventory View

    Purpose: Storage infrastructure overview with capacity metrics
    Source:  Current.vDatastore

    Usage:
        SELECT * FROM [Reporting].[vw_Datastore_Inventory]
        WHERE Free_Percent < 20
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Datastore_Inventory]
AS
SELECT
    -- Identity
    Name AS DatastoreName,
    VI_SDK_Server,

    -- Status
    Config_status,
    Accessible,

    -- Type
    Type,
    Major_Version,
    Version,
    VMFS_Upgradeable,

    -- Capacity (MiB)
    Capacity_MiB,
    Provisioned_MiB,
    In_Use_MiB,
    Free_MiB,
    Free_Percent,

    -- Usage
    Num_VMs,
    Num_Hosts,

    -- SIOC
    SIOC_enabled,
    SIOC_Threshold,

    -- Cluster
    Cluster_name,
    Cluster_capacity_MiB,
    Cluster_free_space_MiB,

    -- Technical
    Block_size,
    URL,

    -- Audit
    ImportBatchId,
    LastModifiedDate

FROM [Current].[vDatastore]
GO

PRINT 'Created [Reporting].[vw_Datastore_Inventory]'
GO
