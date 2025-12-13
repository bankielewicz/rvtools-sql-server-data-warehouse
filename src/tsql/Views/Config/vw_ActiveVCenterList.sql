/*
    View: Config.vw_ActiveVCenterList
    Description: Returns list of active vCenter servers for filtering reports.
                 vCenters not yet registered in Config.ActiveVCenters default to active.
    Usage: JOIN or WHERE ... IN (SELECT VI_SDK_Server FROM Config.vw_ActiveVCenterList)
*/
CREATE OR ALTER VIEW [Config].[vw_ActiveVCenterList]
AS
SELECT VIServer AS VI_SDK_Server
FROM [Config].[ActiveVCenters]
WHERE IsActive = 1
UNION
-- Include vCenters not yet in Config table (default to active)
SELECT DISTINCT VI_SDK_Server
FROM [Current].[vInfo]
WHERE VI_SDK_Server NOT IN (SELECT VIServer FROM [Config].[ActiveVCenters]);
GO
