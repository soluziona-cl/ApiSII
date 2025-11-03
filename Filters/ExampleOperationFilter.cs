using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace ApiSII.Filters
{
    public class ExampleOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Agregar ejemplos de respuesta
            if (operation.Responses.ContainsKey("200") && operation.Responses["200"].Content.ContainsKey("application/json"))
            {
                var successExampleJson = @"{
                    ""message"": ""Mensaje enviado exitosamente"",
                    ""response"": {
                        ""messaging_product"": ""whatsapp"",
                        ""contacts"": [
                            {
                                ""input"": ""56912345678"",
                                ""wa_id"": ""56912345678""
                            }
                        ],
                        ""messages"": [
                            {
                                ""id"": ""wamid.XXX""
                            }
                        ]
                    }
                }";

                operation.Responses["200"].Content["application/json"].Example = 
                    OpenApiAnyFactory.CreateFromJson(successExampleJson);
            }

            if (operation.Responses.ContainsKey("400") && operation.Responses["400"].Content.ContainsKey("application/json"))
            {
                var errorExampleJson = @"{
                    ""message"": ""Error de validación"",
                    ""errors"": {
                        ""Fono"": [""El campo Fono es requerido y no puede ser vacío.""],
                        ""Template"": [""El campo Template no puede estar vacío y no puede exceder 100 caracteres.""]
                    }
                }";

                operation.Responses["400"].Content["application/json"].Example = 
                    OpenApiAnyFactory.CreateFromJson(errorExampleJson);
            }

            if (operation.Responses.ContainsKey("401") && operation.Responses["401"].Content.ContainsKey("application/json"))
            {
                var unauthorizedExampleJson = @"{
                    ""message"": ""No autorizado"",
                    ""details"": ""Token JWT inválido o ausente. Por favor, autentíquese primero.""
                }";

                operation.Responses["401"].Content["application/json"].Example = 
                    OpenApiAnyFactory.CreateFromJson(unauthorizedExampleJson);
            }

            if (operation.Responses.ContainsKey("502") && operation.Responses["502"].Content.ContainsKey("application/json"))
            {
                var badGatewayExampleJson = @"{
                    ""message"": ""Error al comunicarse con la API de WhatsApp"",
                    ""details"": ""No se pudo establecer conexión con el servidor de WhatsApp Business API.""
                }";

                operation.Responses["502"].Content["application/json"].Example = 
                    OpenApiAnyFactory.CreateFromJson(badGatewayExampleJson);
            }
        }
    }

    // Helper para crear OpenApiAny desde JSON
    public static class OpenApiAnyFactory
    {
        public static IOpenApiAny CreateFromJson(string json)
        {
            using var doc = JsonDocument.Parse(json);
            return Convert(doc.RootElement);
        }

        private static IOpenApiAny Convert(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => ConvertObject(element),
                JsonValueKind.Array => ConvertArray(element),
                JsonValueKind.String => new OpenApiString(element.GetString() ?? string.Empty),
                JsonValueKind.Number => element.TryGetInt32(out var intValue) 
                    ? new OpenApiInteger(intValue) 
                    : new OpenApiDouble(element.GetDouble()),
                JsonValueKind.True => new OpenApiBoolean(true),
                JsonValueKind.False => new OpenApiBoolean(false),
                JsonValueKind.Null => new OpenApiNull(),
                _ => new OpenApiString(string.Empty)
            };
        }

        private static OpenApiObject ConvertObject(JsonElement element)
        {
            var obj = new OpenApiObject();
            foreach (var prop in element.EnumerateObject())
            {
                obj[prop.Name] = Convert(prop.Value);
            }
            return obj;
        }

        private static OpenApiArray ConvertArray(JsonElement element)
        {
            var arr = new OpenApiArray();
            foreach (var item in element.EnumerateArray())
            {
                arr.Add(Convert(item));
            }
            return arr;
        }
    }
}

