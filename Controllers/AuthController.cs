using ApiSII.Interfaces;
using ApiSII.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiSII.Controllers
{
    /// <summary>
    /// Controlador para autenticación y obtención de tokens JWT
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IJwtService jwtService, ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Autentica un usuario y genera un token JWT
        /// </summary>
        /// <param name="request">Credenciales de usuario (username y password)</param>
        /// <returns>Token JWT y información de autenticación</returns>
        /// <response code="200">Autenticación exitosa, token JWT generado</response>
        /// <response code="400">Error de validación en los datos enviados</response>
        /// <response code="401">Credenciales inválidas</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return BadRequest(new
                    {
                        message = "Error de validación",
                        errors = errors
                    });
                }

                // Validar credenciales
                var isValid = await _jwtService.ValidateCredentialsAsync(request.Username, request.Password);
                if (!isValid)
                {
                    _logger.LogWarning("Intento de autenticación fallido para usuario: {Username}", request.Username);
                    return Unauthorized(new
                    {
                        message = "Credenciales inválidas",
                        details = "El nombre de usuario o contraseña son incorrectos."
                    });
                }

                // Generar token JWT
                var token = _jwtService.GenerateToken(request.Username);
                var jwtSettings = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("JwtSettings");
                var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60");

                var response = new LoginResponseModel
                {
                    Token = token,
                    TokenType = "Bearer",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    Username = request.Username
                };

                _logger.LogInformation("Usuario autenticado exitosamente: {Username}", request.Username);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante la autenticación");
                return StatusCode(500, new
                {
                    message = "Error interno del servidor",
                    details = ex.Message
                });
            }
        }
    }
}

