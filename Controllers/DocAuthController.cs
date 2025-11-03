using ApiSII.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiSII.Controllers
{
    /// <summary>
    /// Controlador para autenticación de acceso a la documentación (Swagger y Docs)
    /// </summary>
    [ApiController]
    [Route("")] // Ruta base vacía para que las rutas de los métodos funcionen directamente
    [ApiExplorerSettings(IgnoreApi = true)] // Excluir de Swagger y documentación
    public class DocAuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<DocAuthController> _logger;

        public DocAuthController(IUserService userService, ILogger<DocAuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Muestra la página de login para acceder a la documentación
        /// </summary>
        [HttpGet("docs-login")]
        public IActionResult Login([FromQuery] string? returnUrl, [FromQuery] bool denied = false, [FromQuery] bool loggedout = false)
        {
            _logger.LogInformation("DocAuthController.Login GET - Path: {Path}, PathBase: {PathBase}", 
                HttpContext.Request.Path, HttpContext.Request.PathBase);
            
            var viewModel = new
            {
                ReturnUrl = returnUrl ?? "/swagger",
                ErrorMessage = denied ? "Acceso denegado. Por favor, inicia sesión." : null,
                SuccessMessage = loggedout ? "Sesión cerrada exitosamente." : null,
                HasError = denied,
                HasSuccess = loggedout
            };

            // Retornar HTML directamente
            var html = GetLoginPage(viewModel);
            return Content(html, "text/html");
        }

        /// <summary>
        /// Procesa el login de la documentación
        /// </summary>
        [HttpPost("docs-login")]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, [FromForm] string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                var errorHtml = GetLoginPage(new { ReturnUrl = returnUrl ?? "/swagger", ErrorMessage = "Usuario y contraseña son requeridos.", HasError = true });
                return Content(errorHtml, "text/html");
            }

            // Validar credenciales usando el mismo servicio que la API
            var isValid = await _userService.ValidateUserCredentialsAsync(username, password);

            if (!isValid)
            {
                _logger.LogWarning("Intento de login fallido para documentación: {Username}", username);
                var errorHtml = GetLoginPage(new { ReturnUrl = returnUrl ?? "/swagger", ErrorMessage = "Credenciales inválidas. Por favor, intente nuevamente.", HasError = true });
                return Content(errorHtml, "text/html");
            }

            // Crear claims para la autenticación
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim("DocsAccess", "true")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "DocsCookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            };

            // Iniciar sesión con el esquema de cookies
            await HttpContext.SignInAsync("DocsCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

            _logger.LogInformation("Usuario autenticado exitosamente para documentación: {Username}", username);

            // Redirigir a la URL solicitada o a Swagger por defecto
            // Considerar el PathBase si existe (para prefijos como /API/SII)
            var pathBase = HttpContext.Request.PathBase.Value ?? "";
            var redirectUrl = returnUrl ?? $"{pathBase.TrimEnd('/')}/swagger";
            
            // Asegurar que la URL de retorno incluya el PathBase si no lo tiene
            if (!string.IsNullOrEmpty(pathBase) && !redirectUrl.StartsWith(pathBase))
            {
                redirectUrl = $"{pathBase.TrimEnd('/')}{redirectUrl}";
            }
            
            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Cierra la sesión de la documentación
        /// </summary>
        [HttpGet("docs-logout")]
        [HttpPost("docs-logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("DocsCookieAuth");
            var pathBase = HttpContext.Request.PathBase.Value ?? "";
            var logoutUrl = $"{pathBase.TrimEnd('/')}/docs-login?loggedout=true";
            return Redirect(logoutUrl);
        }

        private string GetLoginPage(dynamic viewModel)
        {
            var errorHtml = viewModel.HasError ? $@"<div class=""error-message"">{viewModel.ErrorMessage}</div>" : "";
            var successHtml = viewModel.HasSuccess ? $@"<div class=""success-message"">{viewModel.SuccessMessage}</div>" : "";
            
            // Obtener el PathBase para construir rutas correctas
            var pathBase = HttpContext.Request.PathBase.Value ?? "";
            var formAction = $"{pathBase.TrimEnd('/')}/docs-login";
            
            var html = $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Login - ApiSII Documentación</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        
        .login-container {{
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
            padding: 40px;
            width: 100%;
            max-width: 400px;
        }}
        
        .login-header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        
        .login-header h1 {{
            color: #333;
            font-size: 28px;
            margin-bottom: 10px;
        }}
        
        .login-header p {{
            color: #666;
            font-size: 14px;
        }}
        
        .form-group {{
            margin-bottom: 20px;
        }}
        
        .form-group label {{
            display: block;
            margin-bottom: 8px;
            color: #333;
            font-weight: 500;
            font-size: 14px;
        }}
        
        .form-group input {{
            width: 100%;
            padding: 12px;
            border: 2px solid #e0e0e0;
            border-radius: 6px;
            font-size: 14px;
            transition: border-color 0.3s;
        }}
        
        .form-group input:focus {{
            outline: none;
            border-color: #667eea;
        }}
        
        .error-message {{
            background-color: #fee;
            color: #c33;
            padding: 12px;
            border-radius: 6px;
            margin-bottom: 20px;
            font-size: 14px;
            border-left: 4px solid #c33;
        }}
        
        .success-message {{
            background-color: #efe;
            color: #3c3;
            padding: 12px;
            border-radius: 6px;
            margin-bottom: 20px;
            font-size: 14px;
            border-left: 4px solid #3c3;
        }}
        
        .btn-login {{
            width: 100%;
            padding: 14px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 6px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
        }}
        
        .btn-login:hover {{
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
        }}
        
        .btn-login:active {{
            transform: translateY(0);
        }}
        
        .login-footer {{
            text-align: center;
            margin-top: 20px;
            color: #666;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class=""login-container"">
        <div class=""login-header"">
            <h1>ApiSII</h1>
            <p>Acceso a la Documentación</p>
        </div>
        
            {errorHtml}
            {successHtml}
        
        <form method=""post"" action=""{formAction}"">
            <input type=""hidden"" name=""returnUrl"" value=""{viewModel.ReturnUrl}"" />
            
            <div class=""form-group"">
                <label for=""username"">Usuario</label>
                <input type=""text"" id=""username"" name=""username"" required autofocus />
            </div>
            
            <div class=""form-group"">
                <label for=""password"">Contraseña</label>
                <input type=""password"" id=""password"" name=""password"" required />
            </div>
            
            <button type=""submit"" class=""btn-login"">Iniciar Sesión</button>
        </form>
        
        <div class=""login-footer"">
            <p>Acceso restringido. Solo usuarios autorizados.</p>
        </div>
    </div>
</body>
</html>";
            
            return html.Replace("{formAction}", formAction);
        }
    }
}

