-- Script SQL para insertar usuarios adicionales en la tabla UserAPI
-- Ejecutar en la base de datos OmniFlows_CoreAuth (DefaultConnection_Core)
-- 
-- Usuarios a insertar:
-- 1. sii_api - Contraseña: Fl%vYY4zdf!l (12 caracteres aleatorios)
-- 2. sii_api_test - Contraseña: #4GStT7TEWt* (12 caracteres aleatorios)
--
-- NOTA: Las contraseñas están hasheadas usando SHA256 (en minúsculas, sin guiones)

USE [OmniFlows_CoreAuth]
GO

-- Insertar usuario sii_api
-- Contraseña: Fl%vYY4zdf!l
-- Hash SHA256: a87c35cd159df1735bd2ee79c3fe8e3733744705f7ff129137e9f1d89bcb0ab4
IF NOT EXISTS (SELECT 1 FROM [dbo].[UserAPI] WHERE Username = 'sii_api')
BEGIN
    INSERT INTO [dbo].[UserAPI] 
    (
        [Username],
        [PasswordHash],
        [Email],
        [IsActive],
        [CreatedAt],
        [Description]
    )
    VALUES 
    (
        'sii_api',
        'a87c35cd159df1735bd2ee79c3fe8e3733744705f7ff129137e9f1d89bcb0ab4',
        'sii_api@apisii.com',
        1, -- Activo
        GETUTCDATE(),
        'Usuario API para integraciones SII'
    );
    
    PRINT 'Usuario sii_api insertado exitosamente.';
END
ELSE
BEGIN
    PRINT 'Usuario sii_api ya existe. Omitiendo inserción.';
END
GO

-- Insertar usuario sii_api_test
-- Contraseña: #4GStT7TEWt*
-- Hash SHA256: 1fee416fd5cbdfe55151d1b59b190301cfacd640986dc26322170086fb8a200a
IF NOT EXISTS (SELECT 1 FROM [dbo].[UserAPI] WHERE Username = 'sii_api_test')
BEGIN
    INSERT INTO [dbo].[UserAPI] 
    (
        [Username],
        [PasswordHash],
        [Email],
        [IsActive],
        [CreatedAt],
        [Description]
    )
    VALUES 
    (
        'sii_api_test',
        '1fee416fd5cbdfe55151d1b59b190301cfacd640986dc26322170086fb8a200a',
        'sii_api_test@apisii.com',
        1, -- Activo
        GETUTCDATE(),
        'Usuario API de pruebas para integraciones SII'
    );
    
    PRINT 'Usuario sii_api_test insertado exitosamente.';
END
ELSE
BEGIN
    PRINT 'Usuario sii_api_test ya existe. Omitiendo inserción.';
END
GO

-- Verificar usuarios insertados
SELECT 
    [Id],
    [Username],
    [PasswordHash],
    [Email],
    [IsActive],
    [CreatedAt],
    [UpdatedAt],
    [LastLoginAt],
    [Description]
FROM [dbo].[UserAPI]
WHERE Username IN ('sii_api', 'sii_api_test')
ORDER BY [Id];
GO

PRINT 'Script completado. Se han insertado los usuarios sii_api y sii_api_test.';
PRINT '';
PRINT 'Credenciales:';
PRINT 'Usuario 1: sii_api / Contraseña: Fl%vYY4zdf!l';
PRINT 'Usuario 2: sii_api_test / Contraseña: #4GStT7TEWt*';

