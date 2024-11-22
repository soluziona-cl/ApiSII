// Controllers/FtpController.cs
using CajValp.Interfaces;
using CajValp.Models;
using CajValp.Repositories;
using CajValp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CajValp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FtpController : ControllerBase
    {
        private readonly IFtpService _ftpService;
        private readonly FtpControlRepository _repository;
        private readonly FileProcessingService _repositoryfile;
        private readonly FileProcessingService2 _repositoryfile2;
        public FtpController(IFtpService ftpService, FtpControlRepository repository, FileProcessingService file, FileProcessingService2 file2)
        {
            _ftpService = ftpService;
            _repository = repository;
            _repositoryfile = file;
            _repositoryfile2 = file2;
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadCsv()
        {
            string filePath = await _ftpService.DownloadLatestFileAsync();

            if (filePath != null)
            {
                var record = new FtpControlRecord
                {
                    FileName = filePath,
                    ImportDate = DateTime.Now,
                    IsProcessed = false
                };

                await _repository.AddRecordAsync(record);

                return Ok(new { message = "File downloaded and record added to control table.", filePath });
            }

            return NotFound("No file found to download.");
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadCsvByName(string fileName)
        {
            string filePath = await _ftpService.DownloadFileByNameAsync(fileName);

            if (filePath != null)
            {
                var record = new FtpControlRecord
                {
                    FileName = filePath,
                    ImportDate = DateTime.Now,
                    IsProcessed = false
                };

                await _repository.AddRecordAsync(record);
                await _repositoryfile.ProcessFileAsync(filePath);

                return Ok(new { message = "File downloaded and record added to control table.", filePath });
            }

            return NotFound($"File with name '{fileName}' not found on the server.");
        }


        [HttpGet("download2/{fileName}")]
        public async Task<IActionResult> Download2CsvByName(string fileName)
        {
            string filePath = await _ftpService.DownloadFile2ByNameAsync(fileName);

            if (filePath != null)
            {
                var record = new FtpControlRecord
                {
                    FileName = filePath,
                    ImportDate = DateTime.Now,
                    IsProcessed = false
                };

                await _repository.AddRecordAsync(record);
                await _repositoryfile2.ProcessFileAsync2(filePath);

                return Ok(new { message = "File downloaded and record added to control table.", filePath });
            }

            return NotFound($"File with name '{fileName}' not found on the server.");
        }
    }
}
