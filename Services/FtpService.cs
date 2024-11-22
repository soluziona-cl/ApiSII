// Services/FtpService.cs
using FluentFTP;
using CajValp.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CajValp.Services
{
    public class FtpService : IFtpService
    {
        private readonly string _ftpHost = "200.14.246.179";
        private readonly string _ftpUser = "encuesta@sendacaj.cl";
        private readonly string _ftpPass = "T8Rn2:k)V90E$iOum&@@";
        private readonly string _ftpFolder = "/enviados/";
        private readonly string _ftpFolder2 = "/enviados_encuesta_2/";

        public async Task<string> DownloadLatestFileAsync()
        {
            using var ftpClient = new FtpClient(_ftpHost, _ftpUser, _ftpPass);
            ftpClient.Connect();

            var files = await ftpClient.GetListingAsync(_ftpFolder);
            var latestFile = files
                .Where(x => x.Name.StartsWith("Meta1CAJVAL"))
                .OrderByDescending(x => x.Modified)
                .FirstOrDefault();

            if (latestFile != null)
            {
                string sanitizedFileName = latestFile.Name.Replace(":", "_");
                string localFilePath = Path.Combine("Download", sanitizedFileName);

                // Ensure the local directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);

                // Copy the file directly
                using (var ftpStream = await ftpClient.OpenReadAsync(latestFile.FullName))
                using (var localStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await ftpStream.CopyToAsync(localStream);
                }

                return localFilePath;
            }

            return null!;
        }

        public async Task<string> DownloadFileByNameAsync(string fileName)
        {
            using var ftpClient = new FtpClient(_ftpHost, _ftpUser, _ftpPass);
            ftpClient.Connect();
            string remoteFilePath = Path.Combine(_ftpFolder, fileName);

            if (await ftpClient.FileExistsAsync(remoteFilePath))
            {
                string sanitizedFileName = fileName.Replace(":", "_");
                string localFilePath = Path.Combine("Download", sanitizedFileName);

                // Ensure the local directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);

                // Copy the file directly
                using (var ftpStream = await ftpClient.OpenReadAsync(remoteFilePath))
                using (var localStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await ftpStream.CopyToAsync(localStream);
                }

                return localFilePath;
            }

            return null!;
        }

        public async Task<string> DownloadFile2ByNameAsync(string fileName)
        {
            using var ftpClient = new FtpClient(_ftpHost, _ftpUser, _ftpPass);
            ftpClient.Connect();
            string remoteFilePath = Path.Combine(_ftpFolder2, fileName);

            if (await ftpClient.FileExistsAsync(remoteFilePath))
            {
                string sanitizedFileName = fileName.Replace(":", "_");
                string localFilePath = Path.Combine("Download", sanitizedFileName);

                // Ensure the local directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);

                // Copy the file directly
                using (var ftpStream = await ftpClient.OpenReadAsync(remoteFilePath))
                using (var localStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await ftpStream.CopyToAsync(localStream);
                }

                return localFilePath;
            }

            return null!;
        }
        public async Task<string> UploadFileAsync(string localFilePath)
        {
            using var ftpClient = new FtpClient(_ftpHost, _ftpUser, _ftpPass);
            ftpClient.Connect();

            string remoteFilePath = Path.Combine("/recibidos/", Path.GetFileName(localFilePath));

            using (var localStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await ftpClient.UploadAsync(localStream, remoteFilePath);
            }

            return remoteFilePath; // Return the remote file path as a string
        }
        public async Task<string> UploadFile2Async(string localFilePath)
        {
            using var ftpClient = new FtpClient(_ftpHost, _ftpUser, _ftpPass);
            ftpClient.Connect();

            string remoteFilePath = Path.Combine("/recibidos_2/", Path.GetFileName(localFilePath));

            using (var localStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await ftpClient.UploadAsync(localStream, remoteFilePath);
            }

            return remoteFilePath; // Return the remote file path as a string
        }
    }
}
