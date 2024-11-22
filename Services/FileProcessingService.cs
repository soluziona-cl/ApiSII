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
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IFileRepository _fileRepository;
        private readonly string _connectionString;
        private readonly ILogger<FileProcessingService> _logger;

        public FileProcessingService(IFileRepository fileRepository, string connectionString, ILogger<FileProcessingService> logger)
        {
            _fileRepository = fileRepository;
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task ProcessFileAsync(string localFilePath)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID_ENCUESTA", typeof(string));
            dataTable.Columns.Add("ID_OI", typeof(string));
            dataTable.Columns.Add("TELEFONO", typeof(string));
            dataTable.Columns.Add("REGION", typeof(string));
            dataTable.Columns.Add("FECHA_INGRESO_OI", typeof(string));
            dataTable.Columns.Add("centro_CAJVAL", typeof(string));
            dataTable.Columns.Add("COMUNAS", typeof(string));
            dataTable.Columns.Add("SEXO", typeof(string));
            dataTable.Columns.Add("SEXO_OTRO", typeof(string));
            dataTable.Columns.Add("fech_nac_pat", typeof(string));
            dataTable.Columns.Add("edad", typeof(string));
            dataTable.Columns.Add("mes", typeof(string));
            dataTable.Columns.Add("tipo_centro", typeof(string));
            dataTable.Columns.Add("Tipo_Atencion", typeof(string));
            dataTable.Columns.Add("Materia", typeof(string));
            dataTable.Columns.Add("Submateria", typeof(string));
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
                        row["ID_ENCUESTA"] = csv.GetField<string>(0);
                        row["ID_OI"] = csv.GetField<string>(1);
                        row["TELEFONO"] = csv.GetField<string>(2);
                        row["REGION"] = csv.GetField<string>(3);
                        row["FECHA_INGRESO_OI"] = csv.GetField<string>(4);
                        row["centro_CAJVAL"] = csv.GetField<string>(5);
                        row["COMUNAS"] = csv.GetField<string>(6);
                        row["SEXO"] = csv.GetField<string>(7);
                        row["SEXO_OTRO"] = csv.GetField<string>(8);
                        row["fech_nac_pat"] = csv.GetField<string>(9);
                        row["edad"] = csv.GetField<string>(10);
                        row["mes"] = csv.GetField<string>(11);
                        row["tipo_centro"] = csv.GetField<string>(12);
                        row["Tipo_Atencion"] = csv.GetField<string>(13);
                        row["Materia"] = csv.GetField<string>(14);
                        row["Submateria"] = csv.GetField<string>(15);
                        row["Archivo"] = Path.GetFileName(localFilePath);

                        dataTable.Rows.Add(row);
                    }
                }

                using (SqlConnection connection = new(_connectionString))
                {
                    await connection.OpenAsync();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string insertQuery = @"INSERT INTO [172.16.119.28].[CustomerSoluziona].dbo.tbl_IVR_CAJVAL_ENCUESTA_CARGA 
                                               (ID_ENCUESTA, ID_OI, TELEFONO, REGION, FECHA_INGRESO_OI, centro_CAJVAL, COMUNAS, SEXO, SEXO_OTRO, fech_nac_pat, edad, mes, tipo_centro, Tipo_Atencion, Materia, Submateria, Archivo)
                                               VALUES (@ID_ENCUESTA, @ID_OI, @TELEFONO, @REGION, @FECHA_INGRESO_OI, @centro_CAJVAL, @COMUNAS, @SEXO, @SEXO_OTRO, @fech_nac_pat, @edad, @mes, @tipo_centro, @Tipo_Atencion, @Materia, @Submateria, @Archivo)";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.Add("@ID_ENCUESTA", SqlDbType.NVarChar).Value = row["ID_ENCUESTA"];
                            command.Parameters.Add("@ID_OI", SqlDbType.NVarChar).Value = row["ID_OI"];
                            command.Parameters.Add("@TELEFONO", SqlDbType.NVarChar).Value = row["TELEFONO"];
                            command.Parameters.Add("@REGION", SqlDbType.NVarChar).Value = row["REGION"];
                            command.Parameters.Add("@FECHA_INGRESO_OI", SqlDbType.NVarChar).Value = row["FECHA_INGRESO_OI"];
                            command.Parameters.Add("@centro_CAJVAL", SqlDbType.NVarChar).Value = row["centro_CAJVAL"];
                            command.Parameters.Add("@COMUNAS", SqlDbType.NVarChar).Value = row["COMUNAS"];
                            command.Parameters.Add("@SEXO", SqlDbType.NVarChar).Value = row["SEXO"];
                            command.Parameters.Add("@SEXO_OTRO", SqlDbType.NVarChar).Value = row["SEXO_OTRO"];
                            command.Parameters.Add("@fech_nac_pat", SqlDbType.NVarChar).Value = row["fech_nac_pat"];
                            command.Parameters.Add("@edad", SqlDbType.NVarChar).Value = row["edad"];
                            command.Parameters.Add("@mes", SqlDbType.NVarChar).Value = row["mes"];
                            command.Parameters.Add("@tipo_centro", SqlDbType.NVarChar).Value = row["tipo_centro"];
                            command.Parameters.Add("@Tipo_Atencion", SqlDbType.NVarChar).Value = row["Tipo_Atencion"];
                            command.Parameters.Add("@Materia", SqlDbType.NVarChar).Value = row["Materia"];
                            command.Parameters.Add("@Submateria", SqlDbType.NVarChar).Value = row["Submateria"];
                            command.Parameters.Add("@Archivo", SqlDbType.NVarChar).Value = row["Archivo"];

                            await command.ExecuteNonQueryAsync();
                        }
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
