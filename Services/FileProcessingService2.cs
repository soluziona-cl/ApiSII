using CsvHelper;
using CsvHelper.Configuration;
using CajValp.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CajValp.Models;

namespace CajValp.Services
{
    public class FileProcessingService2 : IFileProcessingService2
    {
        private readonly IFileRepository _fileRepository;
        private readonly string _connectionString;
        private readonly ILogger<FileProcessingService2> _logger;

        public FileProcessingService2(IFileRepository fileRepository, string connectionString, ILogger<FileProcessingService2> logger)
        {
            _fileRepository = fileRepository;
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task ProcessFileAsync2(string localFilePath)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Id_Interno", typeof(string));
            dataTable.Columns.Add("Id_Caso", typeof(string));
            dataTable.Columns.Add("Region", typeof(string));
            dataTable.Columns.Add("Descripcion", typeof(string));
            dataTable.Columns.Add("Fecha_ingreso_caso", typeof(string));
            dataTable.Columns.Add("Fecha_de_termino", typeof(string));
            dataTable.Columns.Add("Fono", typeof(string));
            dataTable.Columns.Add("Mes_estadistico", typeof(string));
            dataTable.Columns.Add("Semestre", typeof(string));
            dataTable.Columns.Add("Tipo_caso", typeof(string));
            dataTable.Columns.Add("Archivo", typeof(string));
          

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Encoding = Encoding.GetEncoding("ISO-8859-1"), // Change encoding to ISO-8859-1
                Delimiter = ";",
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null,
                Quote = '"', // Ensure quotes are handled correctly
            };

            try
            {
                using (var reader = new StreamReader(localFilePath, Encoding.GetEncoding("ISO-8859-1"))) // Change encoding to ISO-8859-1
                using (var csv = new CsvReader(reader, config))
                {
                    while (await csv.ReadAsync())
                    {
                        var row = dataTable.NewRow();
                        row["Id_Interno"] = csv.GetField<string>(0);
                        row["Id_Caso"] = csv.GetField<string>(1);
                        row["Region"] = csv.GetField<string>(2);
                        row["Descripcion"] = csv.GetField<string>(3);
                        row["Fecha_ingreso_caso"] = csv.GetField<string>(4);
                        row["Fecha_de_termino"] = csv.GetField<string>(5);
                        row["Fono"] = csv.GetField<string>(6);
                        row["Mes_estadistico"] = csv.GetField<string>(7);
                        row["Semestre"] = csv.GetField<string>(8);
                        row["Tipo_caso"] = csv.GetField<string>(9);
                        row["Archivo"] = Path.GetFileName(localFilePath);

                        dataTable.Rows.Add(row);
                    }
                }

                using (SqlConnection connection = new(_connectionString))
                {
                    await connection.OpenAsync();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string insertQuery = @"INSERT INTO [172.16.119.28].[CustomerSoluziona].dbo.tbl_IVR_CAJVAL_ENCUESTA_2_CARGA
                        
                ( Id_Interno      ,Id_Caso      ,Region      ,Descripcion      ,Fecha_ingreso_caso      ,Fecha_de_termino      ,Fono      ,Mes_estadistico      ,Semestre      ,Tipo_caso      ,Archivo)
                values (@Id_Interno, @Id_Caso, @Region, @Descripcion, @Fecha_ingreso_caso, @Fecha_de_termino, @Fono, @Mes_estadistico, @Semestre, @Tipo_caso, @Archivo)";
                        using SqlCommand command = new(insertQuery, connection);
                        command.Parameters.Add("@Id_Interno", SqlDbType.NVarChar).Value = row["Id_Interno"];
                        command.Parameters.Add("@Id_Caso", SqlDbType.NVarChar).Value = row["Id_Caso"];
                        command.Parameters.Add("@Region", SqlDbType.NVarChar).Value = row["Region"];
                        command.Parameters.Add("@Descripcion", SqlDbType.NVarChar).Value = row["Descripcion"];
                        command.Parameters.Add("@Fecha_ingreso_caso", SqlDbType.NVarChar).Value = row["Fecha_ingreso_caso"];
                        command.Parameters.Add("@Fecha_de_termino", SqlDbType.NVarChar).Value = row["Fecha_de_termino"];
                        command.Parameters.Add("@Fono", SqlDbType.NVarChar).Value = row["Fono"];
                        command.Parameters.Add("@Mes_estadistico", SqlDbType.NVarChar).Value = row["Mes_estadistico"];
                        command.Parameters.Add("@Semestre", SqlDbType.NVarChar).Value = row["Semestre"];
                        command.Parameters.Add("@Tipo_caso", SqlDbType.NVarChar).Value = row["Tipo_caso"];
                        command.Parameters.Add("@Archivo", SqlDbType.NVarChar).Value = row["Archivo"];

                        await command.ExecuteNonQueryAsync();
                    }
                }

                var fileModel = new FileModel
                {
                    FileName = Path.GetFileName(localFilePath),
                    ImportedAt = DateTime.Now
                };

                await _fileRepository.SaveFileRecordAsync(fileModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FilePath}", localFilePath);
                throw;
            }
        }
    }
}
