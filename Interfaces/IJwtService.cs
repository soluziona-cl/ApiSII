using ApiSII.Models;

namespace ApiSII.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de generación y validación de tokens JWT
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Genera un token JWT para un usuario autenticado
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Token JWT generado</returns>
        string GenerateToken(string username);

        /// <summary>
        /// Valida las credenciales de un usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="password">Contraseña</param>
        /// <returns>True si las credenciales son válidas, False en caso contrario</returns>
        Task<bool> ValidateCredentialsAsync(string username, string password);
    }
}

