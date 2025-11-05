-- =============================================
-- Stored Procedure: sp_UpsertWhatsAppUploadBatchForCampaign
-- Descripción: Verifica si existe un batch para una campaña, si existe lo actualiza, si no existe lo crea
-- Autor: Sistema CRM Omnicanal
-- Fecha: 2025-01-11
-- =============================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UpsertWhatsAppUploadBatchForCampaign]
    @CampaignId INT,
    @TenantId INT,
    @ClientId INT = 0,
    @BatchId BIGINT OUTPUT,
    @WasCreated BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar si existe un registro para esta campaña y tenant
        SELECT TOP 1 
            @BatchId = Id
        FROM [dbo].[WhatsAppUploadBatches]
        WHERE CampaignId = @CampaignId 
          AND TenantId = @TenantId
        ORDER BY CreatedAt DESC;
        
        IF @BatchId IS NOT NULL AND @BatchId > 0
        BEGIN
            -- Existe: actualizar contadores
            SET @WasCreated = 0;
            
            UPDATE [dbo].[WhatsAppUploadBatches]
            SET 
                TotalItems = TotalItems + 1,
                ValidItems = ValidItems + 1,
                ProcessedItems = ProcessedItems + 1,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @BatchId;
            
            -- Verificar que se actualizó correctamente
            IF @@ROWCOUNT = 0
            BEGIN
                RAISERROR('Error al actualizar el batch existente', 16, 1);
                RETURN;
            END
        END
        ELSE
        BEGIN
            -- No existe: crear nuevo batch
            SET @WasCreated = 1;
            
            DECLARE @BatchName NVARCHAR(100) = 'Campaign_' + CAST(@CampaignId AS NVARCHAR(10)) + '_' + 
                FORMAT(GETUTCDATE(), 'yyyyMMddHHmmss');
            
            INSERT INTO [dbo].[WhatsAppUploadBatches]
            (
                [Name],
                [ClientId],
                [CampaignId],
                [TenantId],
                [TotalItems],
                [ValidItems],
                [InvalidItems],
                [ProcessedItems],
                [Status],
                [CreatedAt],
                [CreatedByUserId]
            )
            VALUES
            (
                @BatchName,
                @ClientId,
                @CampaignId,
                @TenantId,
                1,      -- TotalItems
                1,      -- ValidItems
                0,      -- InvalidItems
                1,      -- ProcessedItems (se actualiza después de insertar el contacto)
                'Created',
                GETUTCDATE(),
                0       -- CreatedByUserId
            );
            
            SET @BatchId = SCOPE_IDENTITY();
            
            -- Verificar que se insertó correctamente
            IF @BatchId IS NULL OR @BatchId <= 0
            BEGIN
                RAISERROR('Error al crear el nuevo batch', 16, 1);
                RETURN;
            END
        END
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        SET @BatchId = 0;
        SET @WasCreated = 0;
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

