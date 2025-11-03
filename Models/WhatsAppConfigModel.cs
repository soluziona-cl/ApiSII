namespace ApiSII.Models
{
    public class WhatsAppConfigModel
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string? BusinessId { get; set; }
        public string? PhoneNumberId { get; set; }
        public string? AccessToken { get; set; }
        public string? ApiVersion { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ApiVersionFlow { get; set; }
        public string? UrlTestWSP { get; set; }
        public string? UrlProdWSP { get; set; }
    }
}

