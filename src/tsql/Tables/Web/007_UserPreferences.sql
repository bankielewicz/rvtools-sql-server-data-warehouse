/*
    User Preferences Table

    Purpose: Store per-user settings including global time filter

    TimeFilterValue options:
        'latest'  - Most recent import only
        '7d'      - Last 7 days
        '30d'     - Last 30 days (default)
        '90d'     - Last 90 days
        '1y'      - Last year
        'all'     - All time (no filter)
*/

USE [RVToolsDW]
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPreferences' AND schema_id = SCHEMA_ID('Web'))
BEGIN
    CREATE TABLE [Web].[UserPreferences]
    (
        UserPreferenceId    INT IDENTITY(1,1) PRIMARY KEY,
        UserId              INT NOT NULL,
        PreferenceKey       NVARCHAR(100) NOT NULL,
        PreferenceValue     NVARCHAR(500) NOT NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ModifiedDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_UserPreferences_Users FOREIGN KEY (UserId)
            REFERENCES [Web].[Users](UserId) ON DELETE CASCADE,
        CONSTRAINT UQ_UserPreferences_UserKey UNIQUE (UserId, PreferenceKey)
    );

    CREATE INDEX IX_UserPreferences_UserId ON [Web].[UserPreferences](UserId);

    PRINT 'Created [Web].[UserPreferences]'
END
ELSE
BEGIN
    PRINT '[Web].[UserPreferences] already exists'
END
GO
