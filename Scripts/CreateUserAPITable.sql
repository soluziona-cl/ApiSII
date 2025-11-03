-- Script SQL para crear la tabla UserAPI separada de Users del CRM
-- Ejecutar en la base de datos DefaultConnection_Core

-- Crear tabla UserAPI
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserAPI]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserAPI] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(100) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(256) NOT NULL,
        [Email] NVARCHAR(255) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [LastLoginAt] DATETIME2 NULL,
        [Description] NVARCHAR(500) NULL
    );

    CREATE INDEX IX_UserAPI_Username ON [dbo].[UserAPI] ([Username]);
    CREATE INDEX IX_UserAPI_IsActive ON [dbo].[UserAPI] ([IsActive]);
    CREATE INDEX IX_UserAPI_Email ON [dbo].[UserAPI] ([Email]);
END
GO

-- Stored Procedure: sp_UserAPI_GetByUsername
-- Obtiene un usuario de API por nombre de usuario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UserAPI_GetByUsername]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UserAPI_GetByUsername]
GO

CREATE PROCEDURE [dbo].[sp_UserAPI_GetByUsername]
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        Username,
        PasswordHash,
        Email,
        IsActive,
        CreatedAt,
        UpdatedAt,
        LastLoginAt,
        Description
    FROM [dbo].[UserAPI]
    WHERE Username = @Username AND IsActive = 1;
END
GO

-- Stored Procedure: sp_UserAPI_Create
-- Crea un nuevo usuario de API
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UserAPI_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UserAPI_Create]
GO

CREATE PROCEDURE [dbo].[sp_UserAPI_Create]
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(256),
    @Email NVARCHAR(255) = NULL,
    @IsActive BIT = 1,
    @Description NVARCHAR(500) = NULL,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar si el usuario ya existe
    IF EXISTS (SELECT 1 FROM [dbo].[UserAPI] WHERE Username = @Username)
    BEGIN
        SET @Id = -1; -- Usuario ya existe
        RETURN;
    END

    INSERT INTO [dbo].[UserAPI] 
        (Username, PasswordHash, Email, IsActive, Description, CreatedAt)
    VALUES 
        (@Username, @PasswordHash, @Email, @IsActive, @Description, GETUTCDATE());

    SET @Id = SCOPE_IDENTITY();
END
GO

-- Stored Procedure: sp_UserAPI_Update
-- Actualiza un usuario de API existente
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UserAPI_Update]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UserAPI_Update]
GO

CREATE PROCEDURE [dbo].[sp_UserAPI_Update]
    @Id INT,
    @Username NVARCHAR(100) = NULL,
    @PasswordHash NVARCHAR(256) = NULL,
    @Email NVARCHAR(255) = NULL,
    @IsActive BIT = NULL,
    @Description NVARCHAR(500) = NULL,
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[UserAPI]
    SET 
        Username = ISNULL(@Username, Username),
        PasswordHash = ISNULL(@PasswordHash, PasswordHash),
        Email = ISNULL(@Email, Email),
        IsActive = ISNULL(@IsActive, IsActive),
        Description = ISNULL(@Description, Description),
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;

    SET @RowsAffected = @@ROWCOUNT;
END
GO

-- Stored Procedure: sp_UserAPI_Delete
-- Elimina (desactiva) un usuario de API
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UserAPI_Delete]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UserAPI_Delete]
GO

CREATE PROCEDURE [dbo].[sp_UserAPI_Delete]
    @Id INT,
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Soft delete: solo desactiva el usuario
    UPDATE [dbo].[UserAPI]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;

    SET @RowsAffected = @@ROWCOUNT;
END
GO

-- Stored Procedure: sp_UserAPI_GetAll
-- Obtiene todos los usuarios de API (activos e inactivos)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UserAPI_GetAll]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UserAPI_GetAll]
GO

CREATE PROCEDURE [dbo].[sp_UserAPI_GetAll]
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        Username,
        PasswordHash,
        Email,
        IsActive,
        CreatedAt,
        UpdatedAt,
        LastLoginAt,
        Description
    FROM [dbo].[UserAPI]
    WHERE (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY Username;
END
GO

-- Stored Procedure: sp_UserAPI_UpdateLastLogin
-- Actualiza la fecha del último login
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UserAPI_UpdateLastLogin]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UserAPI_UpdateLastLogin]
GO

CREATE PROCEDURE [dbo].[sp_UserAPI_UpdateLastLogin]
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[UserAPI]
    SET LastLoginAt = GETUTCDATE()
    WHERE Username = @Username;
END
GO

-- Insertar usuario de prueba (admin/admin)
-- Hash SHA256 de "admin" (sin comillas): 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
IF NOT EXISTS (SELECT * FROM [dbo].[UserAPI] WHERE [Username] = 'admin')
BEGIN
    DECLARE @NewId INT;
    EXEC [dbo].[sp_UserAPI_Create]
        @Username = 'admin',
        @PasswordHash = '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', -- Hash de "admin"
        @Email = 'admin@apisii.com',
        @IsActive = 1,
        @Description = 'Usuario administrador de prueba',
        @Id = @NewId OUTPUT;
END
GO

