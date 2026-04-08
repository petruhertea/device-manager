-- Idempotent schema creation (mirrors the EF migration)
-- NOTE: In this project, schema is managed via EF migrations.
-- This script is provided as per spec requirements and can be run
-- independently to recreate the schema from scratch.

IF
NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DeviceManagerDb')
BEGIN
    CREATE
DATABASE DeviceManagerDb;
END
GO

USE DeviceManagerDb;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
CREATE TABLE Users
(
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Name     NVARCHAR(100) NOT NULL,
    Role     NVARCHAR(50)  NOT NULL,
    Location NVARCHAR(100) NOT NULL
);
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Devices' AND xtype='U')
BEGIN
CREATE TABLE Devices
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(100) NOT NULL,
    Manufacturer    NVARCHAR(100) NOT NULL,
    Type            NVARCHAR(20)  NOT NULL,
    OperatingSystem NVARCHAR(50)  NOT NULL,
    OsVersion       NVARCHAR(50)  NOT NULL,
    Processor       NVARCHAR(100) NOT NULL,
    RamAmount       INT NOT NULL,
    Description     NVARCHAR(500) NOT NULL DEFAULT '',
    AssignedUserId  INT NULL REFERENCES Users(Id) ON DELETE SET NULL
);
END
GO