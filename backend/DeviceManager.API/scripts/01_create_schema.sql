-- =============================================================================
-- 01_create_schema.sql
-- Creates the DeviceManagerDb database and all tables.
-- Mirrors the EF Core InitialCreate migration exactly.
-- IDEMPOTENT: safe to run multiple times.
-- =============================================================================

IF
NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DeviceManagerDb')
BEGIN
    CREATE
DATABASE DeviceManagerDb;
END
GO

USE DeviceManagerDb;
GO

-- ── ASP.NET Identity tables ───────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetRoles' AND xtype = 'U')
BEGIN
CREATE TABLE AspNetRoles
(
    Id               INT NOT NULL IDENTITY(1,1),
    Name             NVARCHAR(256)  NULL,
    NormalizedName   NVARCHAR(256)  NULL,
    ConcurrencyStamp NVARCHAR(MAX)  NULL,

    CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'RoleNameIndex' AND object_id = OBJECT_ID('AspNetRoles'))
BEGIN
CREATE UNIQUE INDEX RoleNameIndex
    ON AspNetRoles (NormalizedName) WHERE NormalizedName IS NOT NULL;
END
GO

-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUsers' AND xtype = 'U')
BEGIN
CREATE TABLE AspNetUsers
(
    Id                   INT NOT NULL IDENTITY(1,1),
    -- Custom columns added by ApplicationUser
    FullName             NVARCHAR(100)  NOT NULL DEFAULT '',
    Role                 NVARCHAR(50)   NOT NULL DEFAULT 'Employee',
    Location             NVARCHAR(100)  NOT NULL DEFAULT '',
    -- Standard Identity columns
    UserName             NVARCHAR(256)  NULL,
    NormalizedUserName   NVARCHAR(256)  NULL,
    Email                NVARCHAR(256)  NULL,
    NormalizedEmail      NVARCHAR(256)  NULL,
    EmailConfirmed       BIT NOT NULL DEFAULT 0,
    PasswordHash         NVARCHAR(MAX)  NULL,
    SecurityStamp        NVARCHAR(MAX)  NULL,
    ConcurrencyStamp     NVARCHAR(MAX)  NULL,
    PhoneNumber          NVARCHAR(MAX)  NULL,
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled     BIT NOT NULL DEFAULT 0,
    LockoutEnd           DATETIMEOFFSET NULL,
    LockoutEnabled       BIT NOT NULL DEFAULT 0,
    AccessFailedCount    INT NOT NULL DEFAULT 0,

    CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'EmailIndex' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
CREATE INDEX EmailIndex ON AspNetUsers (NormalizedEmail);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'UserNameIndex' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
CREATE UNIQUE INDEX UserNameIndex
    ON AspNetUsers (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
END
GO

-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetRoleClaims' AND xtype = 'U')
BEGIN
CREATE TABLE AspNetRoleClaims
(
    Id         INT NOT NULL IDENTITY(1,1),
    RoleId     INT NOT NULL,
    ClaimType  NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,

    CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_AspNetRoleClaims_RoleId'
                 AND object_id = OBJECT_ID('AspNetRoleClaims'))
BEGIN
CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims (RoleId);
END
GO

-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUserClaims' AND xtype = 'U')
BEGIN
CREATE TABLE AspNetUserClaims
(
    Id         INT NOT NULL IDENTITY(1,1),
    UserId     INT NOT NULL,
    ClaimType  NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,

    CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_AspNetUserClaims_UserId'
                 AND object_id = OBJECT_ID('AspNetUserClaims'))
BEGIN
CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims (UserId);
END
GO

-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUserLogins' AND xtype = 'U')
BEGIN
CREATE TABLE AspNetUserLogins
(
    LoginProvider       NVARCHAR(450) NOT NULL,
    ProviderKey         NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId              INT NOT NULL,

    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_AspNetUserLogins_UserId'
                 AND object_id = OBJECT_ID('AspNetUserLogins'))
BEGIN
CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins (UserId);
END
GO

-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUserRoles' AND xtype = 'U')
BEGIN
CREATE TABLE AspNetUserRoles
(
    UserId INT NOT NULL,
    RoleId INT NOT NULL,

    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_AspNetUserRoles_RoleId'
                 AND object_id = OBJECT_ID('AspNetUserRoles'))
BEGIN
CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles (RoleId);
END
GO

-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUserTokens' AND xtype = 'U')
BEGIN
CREATE TABLE AspNetUserTokens
(
    UserId        INT NOT NULL,
    LoginProvider NVARCHAR(450) NOT NULL,
    Name          NVARCHAR(450) NOT NULL,
    Value         NVARCHAR(MAX) NULL,

    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
END
GO

-- ── Application tables ────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Devices' AND xtype = 'U')
BEGIN
CREATE TABLE Devices
(
    Id              INT NOT NULL IDENTITY(1,1),
    Name            NVARCHAR(100)  NOT NULL,
    Manufacturer    NVARCHAR(100)  NOT NULL,
    Type            NVARCHAR(20)   NOT NULL, -- 'phone' | 'tablet'
    OperatingSystem NVARCHAR(50)   NOT NULL,
    OsVersion       NVARCHAR(50)   NOT NULL,
    Processor       NVARCHAR(100)  NOT NULL,
    RamAmount       INT NOT NULL,
    Description     NVARCHAR(500)  NOT NULL DEFAULT '',
    AssignedUserId  INT NULL,

    CONSTRAINT PK_Devices PRIMARY KEY (Id),
    CONSTRAINT FK_Devices_AspNetUsers_AssignedUserId
        FOREIGN KEY (AssignedUserId)
            REFERENCES AspNetUsers (Id) ON DELETE SET NULL
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_Devices_AssignedUserId'
                 AND object_id = OBJECT_ID('Devices'))
BEGIN
CREATE INDEX IX_Devices_AssignedUserId ON Devices (AssignedUserId);
END
GO