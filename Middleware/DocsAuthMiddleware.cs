using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiSII.Middleware
{
    /// <summary>
    /// Middleware para proteger las rutas de Swagger y Docs con autenticación basada en cookies
    /// </summary>
    public class DocsAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public DocsAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var pathBase = context.Request.PathBase.Value?.TrimEnd('/') ?? "";

            // Rutas públicas que NO requieren autenticación
            bool isPublicRoute = path.Contains("/docs-login") ||
                                path.Contains("/docs-logout") ||
                                path.StartsWith("/api/") ||
                                path.Contains("/docs/css/") ||
                                path.Contains("/docs/js/") ||
                                path.Contains("/docs/images/") ||
                                path.EndsWith("/docs/index.md");

            // Si es una ruta pública, permitir el acceso
            if (isPublicRoute)
            {
                await _next(context);
                return;
            }

            // Rutas que requieren autenticación
            bool requiresAuth = path.StartsWith("/swagger") || 
                               (path.StartsWith("/docs") && !path.EndsWith("/docs/index.md"));

            if (requiresAuth)
            {
                // Verificar si el usuario está autenticado con el esquema de cookies
                var authResult = await context.AuthenticateAsync("DocsCookieAuth");
                
                if (!authResult.Succeeded || authResult.Principal == null)
                {
                    // Si no está autenticado, redirigir al login usando PathBase
                    var returnUrl = context.Request.Path.Value ?? "/swagger";
                    var loginUrl = string.IsNullOrEmpty(pathBase) 
                        ? $"/docs-login?returnUrl={Uri.EscapeDataString(returnUrl)}"
                        : $"{pathBase}/docs-login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                    
                    context.Response.Redirect(loginUrl);
                    return;
                }

                // Establecer el usuario autenticado en el contexto
                context.User = authResult.Principal;
            }

            await _next(context);
        }
    }
}

