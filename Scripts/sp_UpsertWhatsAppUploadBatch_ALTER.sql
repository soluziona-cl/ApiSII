-- =============================================
-- Stored Procedure: sp_UpsertWhatsAppUploadBatch (ALTER)
-- Descripción: Actualiza el stored procedure para buscar por Name (Campaign) en lugar de CampaignId.
--              Verifica si existe un batch con Name (Campaign) y TenantId, si existe lo actualiza, si no existe lo crea.
--              Utiliza valores fijos: ClientId=22, CampaignId=8, TenantId=5
-- Autor: Sistema CRM Omnicanal
-- Fecha: 2025-01-XX
-- =============================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_UpsertWhatsAppUploadBatch]
    @CampaignId INT,
    @TenantId INT,
    @CampaignName NVARCHAR(100),
    @ClientId INT,
    @BatchId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Buscar si existe un batch con este Name (Campaign) y TenantId
        SELECT TOP 1 @BatchId = Id
        FROM [dbo].[WhatsAppUploadBatches] 
        WHERE [Name] = @CampaignName AND TenantId = @TenantId 
        ORDER BY Id DESC;
        
        IF @BatchId IS NOT NULL
        BEGIN
            -- Existe, actualizar incrementando los contadores
            UPDATE [dbo].[WhatsAppUploadBatches]
            SET TotalItems = TotalItems + 1,
                ValidItems = ValidItems + 1,
                ProcessedItems = ProcessedItems + 1,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @BatchId;
        END
        ELSE
        BEGIN
            -- No existe, crear nuevo batch
            DECLARE @Name NVARCHAR(100) = ISNULL(@CampaignName, 'Batch_' + CAST(@CampaignId AS NVARCHAR(20)) + '_' + FORMAT(GETUTCDATE(), 'yyyyMMddHHmmss'));
            
            -- Usar el ClientId proporcionado como parámetro (valor fijo: 22)
            
            -- Crear nuevo batch usando el SP existente
            DECLARE @NewBatchId BIGINT;
            EXEC [dbo].[sp_CreateWhatsAppUploadBatch]
                @Name = @Name,
                @ClientId = @ClientId,
                @CampaignId = @CampaignId,
                @TenantId = @TenantId,
                @TotalItems = 1,
                @CreatedByUserId = 0,
                @NewBatchId = @NewBatchId OUTPUT;
            
            SET @BatchId = @NewBatchId;
            
            -- Actualizar el batch recién creado para incrementar ProcessedItems y ValidItems
            UPDATE [dbo].[WhatsAppUploadBatches]
            SET ValidItems = 1,
                ProcessedItems = 1,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @BatchId;
        END
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Re-lanzar el error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

