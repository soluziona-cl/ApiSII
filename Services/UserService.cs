using ApiSII.Interfaces;
using ApiSII.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ApiSII.Services
{
    /// <summary>
    /// Servicio para gestión y validación de usuarios
    /// </summary>
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly ILogger<UserService> _logger;

        public UserService(IConfiguration configuration, ILogger<UserService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection_Core")
                ?? throw new InvalidOperationException("DefaultConnection_Core no está configurada.");
            _logger = logger;
        }

        public async Task<UserModel?> GetUserByUsernameAsync(string username)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Usar stored procedure para obtener usuario de la tabla UserAPI
                using var command = new SqlCommand("sp_UserAPI_GetByUsername", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Username", username);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new UserModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                        Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                        LastLoginAt = reader.IsDBNull(reader.GetOrdinal("LastLoginAt")) ? null : reader.GetDateTime(reader.GetOrdinal("LastLoginAt")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por nombre de usuario: {Username}", username);
                throw;
            }
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return false;
                }

                var user = await GetUserByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Intento de login con usuario inexistente: {Username}", username);
                    return false;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Intento de login con usuario inactivo: {Username}", username);
                    return false;
                }

                // Validar contraseña (hash SHA256)
                var hashedPassword = HashPassword(password);
                var isValid = hashedPassword.Equals(user.PasswordHash, StringComparison.OrdinalIgnoreCase);

                if (isValid)
                {
                    // Actualizar fecha de último login
                    await UpdateLastLoginAsync(username);
                }
                else
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Username}", username);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar credenciales de usuario: {Username}", username);
                return false;
            }
        }

        /// <summary>
        /// Actualiza la fecha del último login del usuario
        /// </summary>
        private async Task UpdateLastLoginAsync(string username)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UserAPI_UpdateLastLogin", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Username", username);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                // No lanzar excepción si falla la actualización del último login
                _logger.LogWarning(ex, "No se pudo actualizar la fecha de último login para usuario: {Username}", username);
            }
        }

        /// <summary>
        /// Genera un hash SHA256 de la contraseña
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}

