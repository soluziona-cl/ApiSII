# Nuevos Stored Procedures para WhatsApp API

## Descripción

Se han creado dos nuevos stored procedures para manejar el guardado de mensajes enviados a través de la API de WhatsApp sin modificar los SPs o tablas existentes.

## Stored Procedures

### 1. sp_UpsertWhatsAppUploadBatchForCampaign

**Propósito**: Verifica si existe un batch para una campaña específica. Si existe, actualiza los contadores. Si no existe, crea uno nuevo.

**Parámetros**:
- `@CampaignId INT` - ID de la campaña
- `@TenantId INT` - ID del tenant
- `@ClientId INT = 0` - ID del cliente (opcional, por defecto 0)
- `@BatchId BIGINT OUTPUT` - ID del batch (creado o existente)
- `@WasCreated BIT OUTPUT` - Indica si el batch fue creado (1) o actualizado (0)

**Ejemplo de uso**:
```sql
DECLARE @BatchId BIGINT;
DECLARE @WasCreated BIT;

EXEC sp_UpsertWhatsAppUploadBatchForCampaign
    @CampaignId = 5,
    @TenantId = 1,
    @ClientId = 0,
    @BatchId = @BatchId OUTPUT,
    @WasCreated = @WasCreated OUTPUT;

SELECT @BatchId AS BatchId, @WasCreated AS WasCreated;
```

### 2. sp_UpdateContactQueueWamid

**Propósito**: Actualiza el campo `wamid` (WhatsApp Message ID) y `dateSend` de un contacto en la tabla `ContactQueus`.

**Parámetros**:
- `@ContactId BIGINT` - ID del contacto en ContactQueus
- `@Wamid NVARCHAR(500)` - WhatsApp Message ID recibido de la API
- `@RowsAffected INT OUTPUT` - Número de filas afectadas

**Ejemplo de uso**:
```sql
DECLARE @RowsAffected INT;

EXEC sp_UpdateContactQueueWamid
    @ContactId = 12345,
    @Wamid = 'wamid.xxxxxxxxxxxx',
    @RowsAffected = @RowsAffected OUTPUT;

SELECT @RowsAffected AS RowsAffected;
```

## Instalación

1. Ejecutar `sp_UpsertWhatsAppUploadBatchForCampaign.sql` en la base de datos `OmniFlows_WhatsApp`
2. Ejecutar `sp_UpdateContactQueueWamid.sql` en la base de datos `OmniFlows_WhatsApp`

## Notas

- Estos SPs no modifican ningún SP o tabla existente
- Se utilizan únicamente por el servicio de API de WhatsApp
- El SP `sp_InsertContactQueue` existente se sigue utilizando para insertar contactos

