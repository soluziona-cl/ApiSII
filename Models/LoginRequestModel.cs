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
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El campo Username debe tener entre 3 y 100 caracteres.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        /// <example>Password123!</example>
        [Required(ErrorMessage = "El campo Password es requerido.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "El campo Password debe tener entre 6 y 100 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }
}

