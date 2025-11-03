namespace ApiSII.Models
{
    /// <summary>
    /// Modelo de usuario API para autenticación (tabla UserAPI separada de Users del CRM)
    /// </summary>
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? Description { get; set; }
    }
}

