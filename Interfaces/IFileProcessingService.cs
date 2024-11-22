using System.Threading.Tasks;

namespace CajValp.Interfaces
{
    public interface IFileProcessingService
    {
        Task ProcessFileAsync(string localFilePath);
    }
}
