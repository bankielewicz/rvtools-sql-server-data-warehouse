/*
    RVTools Data Warehouse - Database Creation Script

    Purpose: Creates the RVToolsDW database with appropriate settings

    Usage: Execute against master database
           sqlcmd -S localhost -d master -i 001_CreateDatabase.sql

    Notes:
    - Adjust file paths as needed for your environment
    - Adjust initial size and growth settings based on expected data volume
*/

USE [master]
GO

-- Check if database exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RVToolsDW')
BEGIN
    PRINT 'Database RVToolsDW already exists. Skipping creation.'
END
ELSE
BEGIN
    PRINT 'Creating database RVToolsDW...'

    CREATE DATABASE [RVToolsDW]
    CONTAINMENT = NONE
    ON PRIMARY
    (
        NAME = N'RVToolsDW',
        FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\RVToolsDW.mdf',
        SIZE = 512MB,
        MAXSIZE = UNLIMITED,
        FILEGROWTH = 256MB
    )
    LOG ON
    (
        NAME = N'RVToolsDW_log',
        FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\RVToolsDW_log.ldf',
        SIZE = 128MB,
        MAXSIZE = 2048GB,
        FILEGROWTH = 128MB
    )

    PRINT 'Database RVToolsDW created successfully.'
END
GO

-- Set database options
USE [RVToolsDW]
GO

-- Set recovery model to SIMPLE for data warehouse (adjust if needed)
ALTER DATABASE [RVToolsDW] SET RECOVERY SIMPLE
GO

-- Enable snapshot isolation for better concurrency during imports
ALTER DATABASE [RVToolsDW] SET ALLOW_SNAPSHOT_ISOLATION ON
GO

ALTER DATABASE [RVToolsDW] SET READ_COMMITTED_SNAPSHOT ON
GO

-- Set compatibility level (SQL Server 2016 = 130, 2017 = 140, 2019 = 150, 2022 = 160)
ALTER DATABASE [RVToolsDW] SET COMPATIBILITY_LEVEL = 150
GO

-- Set auto-update statistics
ALTER DATABASE [RVToolsDW] SET AUTO_UPDATE_STATISTICS ON
GO

ALTER DATABASE [RVToolsDW] SET AUTO_CREATE_STATISTICS ON
GO

PRINT 'Database configuration complete.'
GO
