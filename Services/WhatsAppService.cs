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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<WhatsAppService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection_Core")
                ?? throw new InvalidOperationException("DefaultConnection_Core no está configurada.");
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
    }
}

