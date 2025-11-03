using ApiSII.Models;

namespace ApiSII.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de usuarios
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtiene un usuario por nombre de usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<UserModel?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Valida las credenciales de un usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>True si las credenciales son válidas, False en caso contrario</returns>
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
    }
}

