/*
    RVTools Data Warehouse - Web Schema Creation

    Purpose: Schema for web application logging and audit data

    Usage: Execute against RVToolsDW database
           sqlcmd -S localhost -d RVToolsDW -i 003_CreateWebSchema.sql

    Note: Run after 001_CreateDatabase.sql and 002_CreateSchemas.sql
*/

USE [RVToolsDW]
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Web')
BEGIN
    EXEC('CREATE SCHEMA [Web]')
    PRINT 'Schema [Web] created.'
END
ELSE
    PRINT 'Schema [Web] already exists.'
GO
