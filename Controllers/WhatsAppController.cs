using ApiSII.Interfaces;
using ApiSII.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiSII.Controllers
{
    /// <summary>
    /// Controlador para el envío de mensajes de WhatsApp Business mediante templates
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación JWT
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<WhatsAppController> _logger;

        public WhatsAppController(IWhatsAppService whatsAppService, ILogger<WhatsAppController> logger)
        {
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        /// <summary>
        /// Envía un mensaje de WhatsApp Business usando un template predefinido
        /// </summary>
        /// <param name="request">Datos del mensaje a enviar incluyendo unidad de negocio, template, teléfono y parámetros</param>
        /// <returns>Resultado del envío del mensaje</returns>
        /// <response code="200">Mensaje enviado exitosamente</response>
        /// <response code="400">Error de validación en los datos enviados</response>
        /// <response code="401">No autorizado - Token JWT inválido o ausente</response>
        /// <response code="502">Error al comunicarse con la API de WhatsApp</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("send")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendMessage([FromBody] WhatsAppRequestModel request)
        {
            try
            {
                // Las validaciones se ejecutan automáticamente por los atributos DataAnnotations
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

                var result = await _whatsAppService.SendWhatsAppMessageAsync(request);

                return Ok(new { message = "Mensaje enviado exitosamente", response = result });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al enviar mensaje de WhatsApp");
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error HTTP al enviar mensaje de WhatsApp");
                return StatusCode(502, new { message = "Error al comunicarse con la API de WhatsApp", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al enviar mensaje de WhatsApp");
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }
    }
}

