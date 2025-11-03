namespace ApiSII.Models
{
    /// <summary>
    /// Modelo de respuesta exitosa para el envío de mensaje de WhatsApp
    /// </summary>
    public class WhatsAppSuccessResponse
    {
        /// <summary>
        /// Mensaje descriptivo del resultado
        /// </summary>
        /// <example>Mensaje enviado exitosamente</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Respuesta completa de la API de WhatsApp Business
        /// </summary>
        public object? Response { get; set; }
    }

    /// <summary>
    /// Modelo de respuesta de error
    /// </summary>
    public class WhatsAppErrorResponse
    {
        /// <summary>
        /// Mensaje de error descriptivo
        /// </summary>
        /// <example>Error de validación</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detalles adicionales del error (opcional)
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Diccionario de errores de validación por campo (si aplica)
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}

