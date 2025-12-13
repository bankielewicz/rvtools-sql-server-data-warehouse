/*
    View: Config.vw_ActiveVCenterList
    Description: Returns list of active vCenter servers for filtering reports.
    Usage: JOIN or WHERE ... IN (SELECT VI_SDK_Server FROM Config.vw_ActiveVCenterList)

    Note: Run EXEC dbo.usp_SyncActiveVCenters after importing from new vCenters
          to ensure they appear in this list.
*/
CREATE OR ALTER VIEW [Config].[vw_ActiveVCenterList]
AS
SELECT VIServer AS VI_SDK_Server
FROM [Config].[ActiveVCenters]
WHERE IsActive = 1;
GO
