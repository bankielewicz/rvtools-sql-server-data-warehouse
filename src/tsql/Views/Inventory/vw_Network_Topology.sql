/*
    RVTools Data Warehouse - Network Topology View

    Purpose: Map port groups to VMs and identify orphaned port groups
    Source:  Current.vPort, Current.vSwitch, Current.dvSwitch, Current.vInfo

    Usage:
        SELECT * FROM [Reporting].[vw_Inventory_Network_Topology]
        WHERE Is_Orphaned = 1
        ORDER BY VLAN, Port_Group

    NOTE: Uses reserved word [Switch] - must bracket in queries
*/

USE [RVToolsDW]
GO

CREATE OR ALTER VIEW [Reporting].[vw_Inventory_Network_Topology]
AS
SELECT
    -- Port Group Details
    p.Port_Group,
    p.VLAN,
    p.[Switch] AS Switch_Name,
    p.Host AS HostName,
    p.Datacenter,
    p.Cluster,
    p.VI_SDK_Server,

    -- Switch Type (determine if standard or distributed)
    CASE
        WHEN EXISTS (
            SELECT 1 FROM [Current].[dvSwitch] dv
            WHERE dv.Name = p.[Switch]
              AND dv.VI_SDK_Server = p.VI_SDK_Server
        ) THEN 'Distributed'
        WHEN EXISTS (
            SELECT 1 FROM [Current].[vSwitch] vs
            WHERE vs.[Switch] = p.[Switch]
              AND vs.Host = p.Host
              AND vs.VI_SDK_Server = p.VI_SDK_Server
        ) THEN 'Standard'
        ELSE 'Unknown'
    END AS Switch_Type,

    -- VM Count using this port group
    (
        SELECT COUNT(DISTINCT i.VM_UUID)
        FROM [Current].[vInfo] i
        WHERE p.VI_SDK_Server = i.VI_SDK_Server
          AND (
              i.Network_1 = p.Port_Group OR
              i.Network_2 = p.Port_Group OR
              i.Network_3 = p.Port_Group OR
              i.Network_4 = p.Port_Group OR
              i.Network_5 = p.Port_Group OR
              i.Network_6 = p.Port_Group OR
              i.Network_7 = p.Port_Group OR
              i.Network_8 = p.Port_Group
          )
          AND i.Template = 0
    ) AS VM_Count,

    -- Orphaned Detection
    CASE
        WHEN NOT EXISTS (
            SELECT 1 FROM [Current].[vInfo] i
            WHERE p.VI_SDK_Server = i.VI_SDK_Server
              AND (
                  i.Network_1 = p.Port_Group OR
                  i.Network_2 = p.Port_Group OR
                  i.Network_3 = p.Port_Group OR
                  i.Network_4 = p.Port_Group OR
                  i.Network_5 = p.Port_Group OR
                  i.Network_6 = p.Port_Group OR
                  i.Network_7 = p.Port_Group OR
                  i.Network_8 = p.Port_Group
              )
              AND i.Template = 0
        ) THEN 1
        ELSE 0
    END AS Is_Orphaned,

    -- Security Settings
    p.Promiscuous_Mode,
    p.Mac_Changes,
    p.Forged_Transmits,

    -- Traffic Shaping
    p.Traffic_Shaping,

    -- Policy
    p.Policy AS Load_Balancing_Policy,

    -- Audit
    p.ImportBatchId,
    p.LastModifiedDate

FROM [Current].[vPort] p
WHERE ISNULL(p.IsDeleted, 0) = 0  -- Exclude soft-deleted records
GO

PRINT 'Created [Reporting].[vw_Inventory_Network_Topology]'
GO
