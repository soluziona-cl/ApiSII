using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApiSII.Middleware
{
    /// <summary>
    /// Middleware para ajustar las rutas de Swagger UI cuando hay un PathBase configurado
    /// </summary>
    public class SwaggerPathBaseMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerPathBaseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var pathBase = context.Request.PathBase.Value?.TrimEnd('/') ?? "";

            // Solo procesar rutas de Swagger UI
            if (path.StartsWith("/swagger") && !string.IsNullOrEmpty(pathBase))
            {
                // Guardar el stream original de respuesta
                var originalBodyStream = context.Response.Body;

                try
                {
                    // Crear un nuevo stream de memoria para capturar la respuesta
                    using var responseBody = new MemoryStream();
                    context.Response.Body = responseBody;

                    // Continuar con la siguiente middleware
                    await _next(context);

                    // Solo procesar si la respuesta es HTML y fue exitosa
                    if (context.Response.StatusCode == 200 && 
                        context.Response.ContentType?.Contains("text/html") == true)
                    {
                        // Leer la respuesta
                        responseBody.Seek(0, SeekOrigin.Begin);
                        var responseText = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();

                        // Reemplazar las rutas de los scripts y estilos de Swagger UI
                        responseText = responseText.Replace("src=\"/swagger/", $"src=\"{pathBase}/swagger/");
                        responseText = responseText.Replace("href=\"/swagger/", $"href=\"{pathBase}/swagger/");
                        responseText = responseText.Replace("'/swagger/", $"'{pathBase}/swagger/");
                        responseText = responseText.Replace("\"/swagger/", $"\"{pathBase}/swagger/");
                        
                        // Reemplazar también las rutas relativas que pueden estar mal formadas
                        responseText = responseText.Replace("src=\"swagger/", $"src=\"{pathBase}/swagger/");
                        responseText = responseText.Replace("href=\"swagger/", $"href=\"{pathBase}/swagger/");

                        // Escribir la respuesta modificada
                        var bytes = Encoding.UTF8.GetBytes(responseText);
                        context.Response.Body = originalBodyStream;
                        context.Response.ContentLength = bytes.Length;
                        await context.Response.Body.WriteAsync(bytes);
                    }
                    else
                    {
                        // Si no es HTML, copiar la respuesta original
                        responseBody.Seek(0, SeekOrigin.Begin);
                        context.Response.Body = originalBodyStream;
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
                finally
                {
                    context.Response.Body = originalBodyStream;
                }
            }
            else
            {
                // Si no es Swagger, continuar normalmente
                await _next(context);
            }
        }
    }
}

