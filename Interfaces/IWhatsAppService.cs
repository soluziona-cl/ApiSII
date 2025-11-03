using ApiSII.Models;

namespace ApiSII.Interfaces
{
    public interface IWhatsAppService
    {
        Task<WhatsAppConfigModel?> GetWhatsAppConfigAsync(int unidadDeNegocio);
        Task<string> SendWhatsAppMessageAsync(WhatsAppRequestModel request);
    }
}

