-- =============================================
-- Stored Procedure: sp_UpdateContactQueueWhatsAppAPI
-- Descripción: Actualiza campos adicionales en ContactQueus para la API de WhatsApp:
--              - json: JSON enviado a Meta
--              - dateSend: Fecha de envío con formato específico
--              - dateLoad: Fecha de carga con formato específico
--              - ClientId: 8 (valor fijo)
--              - CreatedByUserId: 99999 (valor fijo)
-- Autor: Sistema CRM Omnicanal
-- Fecha: 2025-01-XX
-- =============================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UpdateContactQueueWhatsAppAPI]
    @ContactId BIGINT,
    @Json NVARCHAR(MAX) = NULL,
    @DateSend NVARCHAR(200) = NULL,
    @DateLoad NVARCHAR(200) = NULL,
    @ClientId INT = 8,
    @CreatedByUserId INT = 99999,
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Actualizar los campos
        UPDATE [dbo].[ContactQueus]
        SET 
            [json] = ISNULL(@Json, [json]),
            [dateSend] = ISNULL(@DateSend, [dateSend]),
            [dateLoad] = ISNULL(@DateLoad, [dateLoad]),
            [ClientId] = @ClientId,
            [CreatedByUserId] = @CreatedByUserId
        WHERE [id] = @ContactId;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Re-lanzar el error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        SET @RowsAffected = 0;
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

