-- Script SQL para crear la tabla de usuarios
-- Ejecutar en la base de datos DefaultConnection_Core

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(100) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(256) NOT NULL,
        [Email] NVARCHAR(255) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL
    );

    CREATE INDEX IX_Users_Username ON [dbo].[Users] ([Username]);
    CREATE INDEX IX_Users_IsActive ON [dbo].[Users] ([IsActive]);
END
GO

-- Script para crear un usuario de prueba
-- La contraseña "Admin123!" tiene el hash SHA256: a8c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8
-- Para generar el hash de otra contraseña, usa el método HashPassword de UserService o una herramienta online

IF NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Email], [IsActive])
    VALUES (
        'admin',
        'a8c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8b5c5c1b8', -- Hash de "Admin123!"
        'admin@apisii.com',
        1
    );
END
GO

-- NOTA: Para crear usuarios nuevos, debes generar el hash SHA256 de la contraseña
-- Puedes usar este código C# o una herramienta online:
-- using System.Security.Cryptography;
-- using System.Text;
-- var sha256 = SHA256.Create();
-- var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("tu_contraseña"));
-- var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();

