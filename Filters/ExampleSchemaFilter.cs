using ApiSII.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace ApiSII.Filters
{
    public class ExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(WhatsAppRequestModel))
            {
                var exampleJson = @"{
                    ""unidadDeNegocio"": 5,
                    ""template"": ""welcome_message"",
                    ""fono"": ""56912345678"",
                    ""campaign"": ""CAMP2024-001"",
                    ""text1"": ""Juan"",
                    ""text2"": ""Gracias por contactarnos"",
                    ""text3"": ""Nos pondremos en contacto pronto"",
                    ""languageCode"": ""es""
                }";

                schema.Example = OpenApiAnyFactory.CreateFromJson(exampleJson);
            }
        }
    }
}

