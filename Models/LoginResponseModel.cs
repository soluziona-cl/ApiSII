namespace ApiSII.Models
{
    /// <summary>
    /// Modelo de respuesta de autenticación exitosa
    /// </summary>
    public class LoginResponseModel
    {
        /// <summary>
        /// Token JWT para autenticación
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de token
        /// </summary>
        /// <example>Bearer</example>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Fecha de expiración del token en formato ISO 8601
        /// </summary>
        /// <example>2024-12-31T23:59:59Z</example>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Nombre de usuario autenticado
        /// </summary>
        /// <example>usuario@example.com</example>
        public string Username { get; set; } = string.Empty;
    }
}

