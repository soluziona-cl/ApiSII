using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CajValp.Services;
using CajValp.Interfaces; // Add this line
using System.IO;
using System;

namespace CajValp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly ExportService _exportService;
        private readonly IFtpService _ftpService;

        public ExportController(ExportService exportService, IFtpService ftpService)
        {
            _exportService = exportService;
            _ftpService = ftpService;
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportData(string parameterValue)
        {
            string fileName = $"export_{parameterValue}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            await _exportService.ExportDataToCsvAsync(parameterValue, fileName);

            // Upload the file to FTP
            string uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload");
            string filePath = Path.Combine(uploadFolderPath, fileName);
            await _ftpService.UploadFileAsync(filePath);

            return Ok(new { message = "File exported and uploaded to FTP successfully", fileName });
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportDataExcel(string parameterValue)
        {
            // Cambia el nombre del archivo a la estructura deseada
            string fileName = $"Meta1CAJVAL_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.xlsx";

            // Llama a un método que exporte a XLSX (debes implementar este método en ExportService)
            await _exportService.ExportDataToXlsxAsync(parameterValue, fileName);

            // Upload the file to FTP
            string uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload");
            string filePath = Path.Combine(uploadFolderPath, fileName);
            await _ftpService.UploadFileAsync(filePath);

            return Ok(new { message = "File exported and uploaded to FTP successfully", fileName });
        }
        [HttpGet("export-excel-2")]
        public async Task<IActionResult> ExportDataExcel2(string parameterValue)
        {
            // Cambia el nombre del archivo a la estructura deseada
            string fileName = $"Terminos_2024_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.xlsx";

            // Llama a un método que exporte a XLSX (debes implementar este método en ExportService)
            await _exportService.ExportDataToXlsxAsync2(parameterValue, fileName);

            // Upload the file to FTP
            string uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload");
            string filePath = Path.Combine(uploadFolderPath, fileName);
            await _ftpService.UploadFile2Async(filePath);

            return Ok(new { message = "File exported and uploaded to FTP successfully", fileName });
        }
    }
}
