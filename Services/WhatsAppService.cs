using ApiSII.Interfaces;
using ApiSII.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Text.Json;

namespace ApiSII.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly string _connectionString;
        private readonly string _connectionStringWSP;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<WhatsAppService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection_Core")
                ?? throw new InvalidOperationException("DefaultConnection_Core no está configurada.");
            _connectionStringWSP = configuration.GetConnectionString("DefaultConnection_WSP")
                ?? throw new InvalidOperationException("DefaultConnection_WSP no está configurada.");
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<WhatsAppConfigModel?> GetWhatsAppConfigAsync(int unidadDeNegocio)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetWhatsAppBusinessConfig", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@TenantId", unidadDeNegocio);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new WhatsAppConfigModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        TenantId = reader.GetInt32(reader.GetOrdinal("TenantId")),
                        BusinessId = reader.IsDBNull(reader.GetOrdinal("BusinessId")) ? null : reader.GetString(reader.GetOrdinal("BusinessId")),
                        PhoneNumberId = reader.IsDBNull(reader.GetOrdinal("PhoneNumberId")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumberId")),
                        AccessToken = reader.IsDBNull(reader.GetOrdinal("AccessToken")) ? null : reader.GetString(reader.GetOrdinal("AccessToken")),
                        ApiVersion = reader.IsDBNull(reader.GetOrdinal("ApiVersion")) ? null : reader.GetString(reader.GetOrdinal("ApiVersion")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                        ApiVersionFlow = reader.IsDBNull(reader.GetOrdinal("ApiVersionFlow")) ? null : reader.GetString(reader.GetOrdinal("ApiVersionFlow")),
                        UrlTestWSP = reader.IsDBNull(reader.GetOrdinal("urlTestWSP")) ? null : reader.GetString(reader.GetOrdinal("urlTestWSP")),
                        UrlProdWSP = reader.IsDBNull(reader.GetOrdinal("urlProdWSP")) ? null : reader.GetString(reader.GetOrdinal("urlProdWSP"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de WhatsApp para UnidadDeNegocio: {UnidadDeNegocio}", unidadDeNegocio);
                throw;
            }
        }

        public async Task<string> SendWhatsAppMessageAsync(WhatsAppRequestModel request)
        {
            try
            {
                // Obtener configuración desde BD
                var config = await GetWhatsAppConfigAsync(request.UnidadDeNegocio);
                if (config == null)
                {
                    throw new InvalidOperationException($"No se encontró configuración de WhatsApp para la UnidadDeNegocio: {request.UnidadDeNegocio}");
                }

                if (!config.IsActive)
                {
                    throw new InvalidOperationException($"La configuración de WhatsApp para la UnidadDeNegocio {request.UnidadDeNegocio} no está activa.");
                }

                if (string.IsNullOrEmpty(config.PhoneNumberId) || string.IsNullOrEmpty(config.AccessToken) || string.IsNullOrEmpty(config.ApiVersion))
                {
                    throw new InvalidOperationException($"La configuración de WhatsApp para la UnidadDeNegocio {request.UnidadDeNegocio} está incompleta.");
                }

                // Limpiar el número de teléfono (eliminar espacios, guiones, paréntesis)
                var cleanedPhone = CleanPhoneNumber(request.Fono);

                // Construir el request para la API de WhatsApp
                var apiRequest = new WhatsAppApiRequest
                {
                    to = cleanedPhone,
                    template = new Template
                    {
                        name = request.Template,
                        language = new Language
                        {
                            code = request.LanguageCode ?? "en"
                        },
                        components = new List<Component>
                        {
                            new Component
                            {
                                type = "body",
                                parameters = GetTextParameters(request)
                            }
                        }
                    }
                };

                // Construir la URL de la API
                var apiUrl = $"https://graph.facebook.com/{config.ApiVersion}/{config.PhoneNumberId}/messages";

                // Crear HttpClient y enviar request
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.AccessToken);

                // Serializar manteniendo los nombres snake_case del modelo tal cual están
                // Usar SnakeCaseNamingPolicy personalizada para asegurar formato correcto
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = false
                };

                var jsonContent = JsonSerializer.Serialize(apiRequest, jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al enviar mensaje de WhatsApp. Status: {Status}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    throw new HttpRequestException($"Error al enviar mensaje de WhatsApp: {response.StatusCode} - {responseContent}");
                }

                _logger.LogInformation("Mensaje de WhatsApp enviado exitosamente a {Fono}", request.Fono);

                // Guardar en base de datos después del envío exitoso
                try
                {
                    await SaveWhatsAppMessageToDatabaseAsync(request, responseContent, jsonContent);
                }
                catch (Exception dbEx)
                {
                    // Log del error pero no fallar la operación principal
                    _logger.LogError(dbEx, "Error al guardar mensaje en base de datos, pero el mensaje fue enviado exitosamente");
                }

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar mensaje de WhatsApp");
                throw;
            }
        }

        private List<Parameter> GetTextParameters(WhatsAppRequestModel request)
        {
            var parameters = new List<Parameter>();

            var textFields = new[] { request.Text1, request.Text2, request.Text3, request.Text4, request.Text5,
                                   request.Text6, request.Text7, request.Text8, request.Text9, request.Text10 };

            foreach (var text in textFields)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    parameters.Add(new Parameter
                    {
                        type = "text",
                        text = text
                    });
                }
            }

            return parameters;
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            // Eliminar espacios, guiones, paréntesis y otros caracteres no numéricos excepto +
            var cleaned = phoneNumber.Replace(" ", "")
                                    .Replace("-", "")
                                    .Replace("(", "")
                                    .Replace(")", "")
                                    .Replace(".", "");

            return cleaned;
        }

        private class SnakeCaseNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return name;

                var result = new StringBuilder();
                result.Append(char.ToLowerInvariant(name[0]));

                for (int i = 1; i < name.Length; i++)
                {
                    if (char.IsUpper(name[i]))
                    {
                        result.Append('_');
                        result.Append(char.ToLowerInvariant(name[i]));
                    }
                    else
                    {
                        result.Append(name[i]);
                    }
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Guarda el mensaje enviado en la base de datos (WhatsAppUploadBatches y ContactQueus)
        /// </summary>
        private async Task SaveWhatsAppMessageToDatabaseAsync(WhatsAppRequestModel request, string apiResponse, string jsonContent)
        {
            try
            {
                // Valores fijos para WhatsAppUploadBatches
                const int clientId = 22;
                const int campaignId = 8;
                const int tenantId = 5;
                _logger.LogInformation("Request recibido en SaveWhatsAppMessageToDatabaseAsync: {@Request}", request);

                // 1. Verificar/crear/actualizar batch en WhatsAppUploadBatches
                var batchId = await EnsureBatchExistsAsync(campaignId, tenantId, request.Campaign, clientId);

                // 2. Insertar contacto en ContactQueus usando el SP
                await InsertContactQueueAsync(request, batchId, tenantId, apiResponse, jsonContent);

                _logger.LogInformation("Mensaje guardado exitosamente en base de datos. BatchId: {BatchId}", batchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar mensaje en base de datos");
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un batch con el Name (Campaign), si no existe lo crea, si existe lo actualiza
        /// </summary>
        private async Task<long> EnsureBatchExistsAsync(int campaignId, int tenantId, string campaignName, int clientId)
        {
            using var connection = new SqlConnection(_connectionStringWSP);
            await connection.OpenAsync();

            // Usar el SP para verificar/crear/actualizar el batch
            using var command = new SqlCommand("sp_UpsertWhatsAppUploadBatch", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@CampaignId", campaignId);
            command.Parameters.AddWithValue("@TenantId", tenantId);
            command.Parameters.AddWithValue("@CampaignName", campaignName ?? string.Empty);
            command.Parameters.AddWithValue("@ClientId", clientId);
            
            var batchIdParam = new SqlParameter("@BatchId", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(batchIdParam);

            await command.ExecuteNonQueryAsync();

            var batchId = Convert.ToInt64(batchIdParam.Value);
            _logger.LogInformation("Batch procesado. BatchId: {BatchId}", batchId);

            return batchId;
        }

        /// <summary>
        /// Inserta un contacto en ContactQueus usando el SP sp_InsertContactQueue
        /// </summary>
        private async Task InsertContactQueueAsync(WhatsAppRequestModel request, long batchId, int tenantId, string apiResponse, string jsonContent)
        {
            using var connection = new SqlConnection(_connectionStringWSP);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_InsertContactQueue", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parámetros del SP
            command.Parameters.AddWithValue("@Phone", request.Fono);
            command.Parameters.AddWithValue("@Queu", string.Empty);
            command.Parameters.AddWithValue("@TemplateName", request.Template);
            command.Parameters.AddWithValue("@Status", "SendOK"); // Mensaje enviado exitosamente
            command.Parameters.AddWithValue("@Param01", request.Text1 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param02", request.Text2 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param03", request.Text3 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param04", request.Text4 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param05", request.Text5 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param06", request.Text6 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param07", request.Text7 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param08", request.Text8 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param09", request.Text9 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param10", request.Text10 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param11", (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param12", (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param13", (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param14", (object)DBNull.Value);
            command.Parameters.AddWithValue("@Param15", (object)DBNull.Value);
            command.Parameters.AddWithValue("@TenantId", tenantId);
            command.Parameters.AddWithValue("@ClientId", (object)DBNull.Value);
            command.Parameters.AddWithValue("@BatchId", batchId);
            command.Parameters.AddWithValue("@CreatedByUserId", (object)DBNull.Value);

            var newContactIdParam = new SqlParameter("@NewContactId", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(newContactIdParam);

            await command.ExecuteNonQueryAsync();

            var newContactId = newContactIdParam.Value != DBNull.Value ? Convert.ToInt64(newContactIdParam.Value) : 0;
            
            // Actualizar campos adicionales con el nuevo SP
            if (newContactId > 0)
            {
                try
                {
                    // Formato de fecha: 2025-10-28 14:35:20.2056780
                    var dateTimeNow = DateTime.UtcNow;
                    var dateSend = dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                    var dateLoad = dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");

                    // Actualizar campos adicionales usando el nuevo SP
                    using var updateCommand = new SqlCommand("sp_UpdateContactQueueWhatsAppAPI", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    updateCommand.Parameters.AddWithValue("@ContactId", newContactId);
                    updateCommand.Parameters.AddWithValue("@Json", jsonContent ?? (object)DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@DateSend", dateSend);
                    updateCommand.Parameters.AddWithValue("@DateLoad", dateLoad);
                    updateCommand.Parameters.AddWithValue("@ClientId", 8); // Valor fijo
                    updateCommand.Parameters.AddWithValue("@CreatedByUserId", 99999); // Valor fijo
                    
                    var rowsAffectedParam = new SqlParameter("@RowsAffected", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    updateCommand.Parameters.Add(rowsAffectedParam);

                    await updateCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Campos adicionales actualizados para ContactId {ContactId}", newContactId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudieron actualizar los campos adicionales para ContactId {ContactId}", newContactId);
                }
            }
            
            // Actualizar el campo wamid con la respuesta de la API si está disponible
            if (newContactId > 0 && !string.IsNullOrEmpty(apiResponse))
            {
                try
                {
                    // Intentar extraer el message_id de la respuesta JSON
                    // La respuesta de WhatsApp API típicamente tiene esta estructura:
                    // { "messaging_product": "whatsapp", "contacts": [...], "messages": [{ "id": "wamid.HBg..." }] }
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var responseJson = JsonSerializer.Deserialize<JsonElement>(apiResponse, options);
                    
                    string? wamid = null;

                    // Intentar obtener el id desde messages[0].id
                    if (responseJson.TryGetProperty("messages", out var messagesElement) && 
                        messagesElement.ValueKind == JsonValueKind.Array && 
                        messagesElement.GetArrayLength() > 0)
                    {
                        var firstMessage = messagesElement[0];
                        if (firstMessage.TryGetProperty("id", out var idElement))
                        {
                            wamid = idElement.GetString();
                        }
                    }

                    // Si no se encontró, intentar buscar directamente en la raíz
                    if (string.IsNullOrEmpty(wamid) && responseJson.TryGetProperty("id", out var rootIdElement))
                    {
                        wamid = rootIdElement.GetString();
                    }

                    // Actualizar el campo wamid si se encontró usando el SP
                    if (!string.IsNullOrEmpty(wamid))
                    {
                        using var updateCommand = new SqlCommand("sp_UpdateContactQueueWamid", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        updateCommand.Parameters.AddWithValue("@ContactId", newContactId);
                        updateCommand.Parameters.AddWithValue("@Wamid", wamid);
                        await updateCommand.ExecuteNonQueryAsync();
                        _logger.LogInformation("Campo wamid actualizado para ContactId {ContactId}: {Wamid}", newContactId, wamid);
                    }
                    else
                    {
                        _logger.LogWarning("No se pudo extraer el message_id de la respuesta de la API: {Response}", apiResponse);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo actualizar el campo wamid con la respuesta de la API: {Response}", apiResponse);
                }
            }

            _logger.LogInformation("Contacto insertado en ContactQueus. ContactId: {ContactId}", newContactId);
        }
    }
}

