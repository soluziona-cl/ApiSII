using System.ComponentModel.DataAnnotations;

namespace ApiSII.Models
{
    /// <summary>
    /// Modelo de solicitud para autenticación JWT
    /// </summary>
    public class LoginRequestModel
    {
        /// <summary>
        /// Nombre de usuario o email
        /// </summary>
        /// <example>usuario@example.com</example>
        [Required(ErrorMessage = "El campo Username es requerido.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Error de credenciales.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        /// <example>Password123!</example>
        [Required(ErrorMessage = "El campo Password es requerido.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Error de credenciales.")]
        public string Password { get; set; } = string.Empty;
    }
}

