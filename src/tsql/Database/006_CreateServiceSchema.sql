/*
    006_CreateServiceSchema.sql
    Creates the Service schema for the Windows Service import job management.

    Execute against: RVToolsDW database
    Part of: Phase 1 - Foundation
*/

USE [RVToolsDW];
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Service')
BEGIN
    EXEC('CREATE SCHEMA [Service] AUTHORIZATION dbo');
    PRINT 'Service schema created successfully';
END
ELSE
BEGIN
    PRINT 'Service schema already exists';
END
GO
