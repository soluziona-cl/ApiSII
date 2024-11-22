// Interfaces/IFtpService.cs
using System.Threading.Tasks;

namespace CajValp.Interfaces
{
    public interface IFtpService
    {
        Task<string> DownloadLatestFileAsync();
        Task<string> DownloadFileByNameAsync(string fileName);
        Task<string> DownloadFile2ByNameAsync(string fileName);
        Task<string> UploadFileAsync(string localFilePath);
        Task<string> UploadFile2Async(string localFilePath);
    }
}
