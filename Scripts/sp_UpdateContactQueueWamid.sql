-- =============================================
-- Stored Procedure: sp_UpdateContactQueueWamid
-- Descripción: Actualiza el campo wamid y dateSend de un contacto en ContactQueus
-- Autor: Sistema CRM Omnicanal
-- Fecha: 2025-01-XX
-- =============================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UpdateContactQueueWamid]
    @ContactId BIGINT,
    @Wamid NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Actualizar el campo wamid y dateSend
        UPDATE [dbo].[ContactQueus] 
        SET wamid = @Wamid, 
            dateSend = CONVERT(VARCHAR(33), GETUTCDATE(), 126)
        WHERE id = @ContactId;
        
        -- Retornar el número de filas afectadas
        SELECT @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        -- Re-lanzar el error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

