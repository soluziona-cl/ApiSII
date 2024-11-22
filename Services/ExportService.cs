using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CajValp.Models;
using OfficeOpenXml; // Asegúrate de instalar el paquete EPPlus

namespace CajValp.Services
{
    public class ExportService
    {
        private readonly string _connectionString;

        public ExportService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ExportDataToCsvAsync(string parameterValue, string fileName)
        {
            var records = new List<ExportDataModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "EXEC [172.16.119.28].[CustomerSoluziona].[dbo].[sp_RS_ReporteIVR_EncuestaCajval] @ParameterValue";
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ParameterValue", parameterValue);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var record = new ExportDataModel
                            {
                                ID_ENCUESTA = reader.GetString(reader.GetOrdinal("ID_ENCUESTA")),
                                ID_OI = reader.GetString(reader.GetOrdinal("ID_OI")),
                                // NOMBRE_CARGA = reader.GetString(reader.GetOrdinal("NOMBRE_CARGA")),
                                // FECHA_CARGA = reader.GetDateTime(reader.GetOrdinal("FECHA_CARGA")),
                                REGION = reader.GetString(reader.GetOrdinal("REGION")),
                                FECHA_INGRESO_OI = reader.GetString(reader.GetOrdinal("FECHA_INGRESO_OI")),
                                TELEFONO = reader.GetString(reader.GetOrdinal("TELEFONO")),
                                CENTRO_CAJVAL = reader.GetString(reader.GetOrdinal("CENTRO_CAJVAL")),
                                COMUNAS = reader.GetString(reader.GetOrdinal("COMUNAS")),
                                SEXO = reader.GetString(reader.GetOrdinal("SEXO")),
                                SEXO_OTRO = reader.GetString(reader.GetOrdinal("SEXO_OTRO")),
                                FECH_NAC_PAT = reader.GetString(reader.GetOrdinal("FECH_NAC_PAT")),
                                EDAD = reader.GetString(reader.GetOrdinal("EDAD")),
                                MES = reader.GetString(reader.GetOrdinal("MES")),
                                TIPO_CENTRO = reader.GetString(reader.GetOrdinal("TIPO_CENTRO")),
                                TIPO_ATENCION = reader.GetString(reader.GetOrdinal("TIPO_ATENCION")),
                                MATERIA = reader.GetString(reader.GetOrdinal("MATERIA")),
                                SUBMATERIA = reader.GetString(reader.GetOrdinal("SUBMATERIA")),
                                DURACION = reader.GetInt32(reader.GetOrdinal("DURACION")),
                                FECHA_DISCADO = reader.GetDateTime(reader.GetOrdinal("FECHA_DISCADO")),
                                // HORA_LLAMADA = reader.GetString(reader.GetOrdinal("HORA_LLAMADA")),
                                P1 = reader.GetInt32(reader.GetOrdinal("P1")),
                                P2 = reader.GetInt32(reader.GetOrdinal("P2")),
                                P3 = reader.GetInt32(reader.GetOrdinal("P3")),
                                P4 = reader.GetInt32(reader.GetOrdinal("P4")),
                                P5 = reader.GetInt32(reader.GetOrdinal("P5")),
                                // EVALUACION_GLOBAL = reader.GetInt32(reader.GetOrdinal("EVALUACION_GLOBAL")),
                                OCUPADO = reader.GetInt32(reader.GetOrdinal("OCUPADO")),
                                NO_CONTESTA = reader.GetInt32(reader.GetOrdinal("NO_CONTESTA")),
                                NO_HAY_LINEA = reader.GetInt32(reader.GetOrdinal("NO_HAY_LINEA")),
                                DESCONECTADO_ANTES = reader.GetInt32(reader.GetOrdinal("DESCONECTADO_ANTES")),
                                SI_CONTESTA = reader.GetInt32(reader.GetOrdinal("SI_CONTESTA")),
                                // ESTADO_ENCUESTA = reader.GetString(reader.GetOrdinal("ESTADO_ENCUESTA")),
                                CANTIDAD_LLAMADAS = reader.GetInt32(reader.GetOrdinal("CANTIDAD_LLAMADAS"))
                            };

                            records.Add(record);
                        }
                    }
                }
            }

            string uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload");
            if (!Directory.Exists(uploadFolderPath))
            {
                Directory.CreateDirectory(uploadFolderPath);
            }

            string filePath = Path.Combine(uploadFolderPath, fileName);

            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                Delimiter = ";", // Adjust delimiter if necessary
                Encoding = Encoding.UTF8
            };

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, config))
            {
                await csv.WriteRecordsAsync(records);
            }
        }

        public async Task ExportDataToXlsxAsync(string parameterValue, string fileName)
        {
            var records = new List<ExportDataModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "EXEC [172.16.119.28].[CustomerSoluziona].[dbo].[sp_RS_ReporteIVR_EncuestaCajval] @ParameterValue";
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ParameterValue", parameterValue);
                    command.CommandTimeout = 120;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var record = new ExportDataModel
                            {
                                ID_ENCUESTA = reader.GetString(reader.GetOrdinal("ID_ENCUESTA")),
                                ID_OI = reader.GetString(reader.GetOrdinal("ID_OI")),
                                // NOMBRE_CARGA = reader.GetString(reader.GetOrdinal("NOMBRE_CARGA")),
                                // FECHA_CARGA = reader.GetDateTime(reader.GetOrdinal("FECHA_CARGA")),
                                REGION = reader.GetString(reader.GetOrdinal("REGION")),
                                FECHA_INGRESO_OI = reader.GetString(reader.GetOrdinal("FECHA_INGRESO_OI")),
                                TELEFONO = reader.GetString(reader.GetOrdinal("TELEFONO")),
                                CENTRO_CAJVAL = reader.GetString(reader.GetOrdinal("CENTRO_CAJVAL")),
                                COMUNAS = reader.GetString(reader.GetOrdinal("COMUNAS")),
                                SEXO = reader.GetString(reader.GetOrdinal("SEXO")),
                                SEXO_OTRO = reader.GetString(reader.GetOrdinal("SEXO_OTRO")),
                                FECH_NAC_PAT = reader.GetString(reader.GetOrdinal("FECH_NAC_PAT")),
                                EDAD = reader.GetString(reader.GetOrdinal("EDAD")),
                                MES = reader.GetString(reader.GetOrdinal("MES")),
                                TIPO_CENTRO = reader.GetString(reader.GetOrdinal("TIPO_CENTRO")),
                                TIPO_ATENCION = reader.GetString(reader.GetOrdinal("TIPO_ATENCION")),
                                MATERIA = reader.GetString(reader.GetOrdinal("MATERIA")),
                                SUBMATERIA = reader.GetString(reader.GetOrdinal("SUBMATERIA")),
                                DURACION = reader.GetInt32(reader.GetOrdinal("DURACION")),
                                FECHA_DISCADO = reader.GetDateTime(reader.GetOrdinal("FECHA_DISCADO")),
                                // HORA_LLAMADA = reader.GetString(reader.GetOrdinal("HORA_LLAMADA")),
                                P1 = reader.GetInt32(reader.GetOrdinal("P1")),
                                P2 = reader.GetInt32(reader.GetOrdinal("P2")),
                                P3 = reader.GetInt32(reader.GetOrdinal("P3")),
                                P4 = reader.GetInt32(reader.GetOrdinal("P4")),
                                P5 = reader.GetInt32(reader.GetOrdinal("P5")),
                                // EVALUACION_GLOBAL = reader.GetInt32(reader.GetOrdinal("EVALUACION_GLOBAL")),
                                OCUPADO = reader.GetInt32(reader.GetOrdinal("OCUPADO")),
                                NO_CONTESTA = reader.GetInt32(reader.GetOrdinal("NO_CONTESTA")),
                                NO_HAY_LINEA = reader.GetInt32(reader.GetOrdinal("NO_HAY_LINEA")),
                                DESCONECTADO_ANTES = reader.GetInt32(reader.GetOrdinal("DESCONECTADO_ANTES")),
                                SI_CONTESTA = reader.GetInt32(reader.GetOrdinal("SI_CONTESTA")),
                                // ESTADO_ENCUESTA = reader.GetString(reader.GetOrdinal("ESTADO_ENCUESTA")),
                                CANTIDAD_LLAMADAS = reader.GetInt32(reader.GetOrdinal("CANTIDAD_LLAMADAS"))
                            };

                            records.Add(record);
                        }
                    }
                }
            }

            // Crear el archivo XLSX
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Data");

                // Agregar encabezados

                worksheet.Cells[1, 1].Value = "ID_ENCUESTA";
                worksheet.Cells[1, 2].Value = "ID_OI";
                // worksheet.Cells[1, 3].Value = "NOMBRE_CARGA";
                // worksheet.Cells[1, 4].Value = "FECHA_CARGA";
                worksheet.Cells[1, 3].Value = "REGION";
                worksheet.Cells[1, 4].Value = "FECHA_INGRESO_OI";
                worksheet.Cells[1, 5].Value = "TELEFONO";
                worksheet.Cells[1, 6].Value = "CENTRO_CAJVAL";
                worksheet.Cells[1, 7].Value = "COMUNAS";
                worksheet.Cells[1, 8].Value = "SEXO";
                worksheet.Cells[1, 9].Value = "SEXO_OTRO";
                worksheet.Cells[1, 10].Value = "FECH_NAC_PAT";
                worksheet.Cells[1, 11].Value = "EDAD";
                worksheet.Cells[1, 12].Value = "MES";
                worksheet.Cells[1, 13].Value = "TIPO_CENTRO";
                worksheet.Cells[1, 14].Value = "TIPO_ATENCION";
                worksheet.Cells[1, 15].Value = "MATERIA";
                worksheet.Cells[1, 16].Value = "SUBMATERIA";
                worksheet.Cells[1, 17].Value = "DURACION";
                worksheet.Cells[1, 18].Value = "FECHA_DISCADO";
                // worksheet.Cells[1, 19].Value = "HORA_LLAMADA";
                worksheet.Cells[1, 19].Value = "P1";
                worksheet.Cells[1, 20].Value = "P2";
                worksheet.Cells[1, 21].Value = "P3";
                worksheet.Cells[1, 22].Value = "P4";
                worksheet.Cells[1, 23].Value = "P5";
                // worksheet.Cells[1, 24].Value = "EVALUACION_GLOBAL";
                worksheet.Cells[1, 24].Value = "OCUPADO";
                worksheet.Cells[1, 25].Value = "NO_CONTESTA";
                worksheet.Cells[1, 26].Value = "NO_HAY_LINEA";
                worksheet.Cells[1, 27].Value = "DESCONECTADO_ANTES";
                worksheet.Cells[1, 28].Value = "SI_CONTESTA";
                // worksheet.Cells[1, 33].Value = "ESTADO_ENCUESTA";
                worksheet.Cells[1, 29].Value = "CANTIDAD_LLAMADAS";

                // Agregar datos
                for (int i = 0; i < records.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = records[i].ID_ENCUESTA;
                    worksheet.Cells[i + 2, 2].Value = records[i].ID_OI;
                    worksheet.Cells[i + 2, 3].Value = records[i].REGION;
                    worksheet.Cells[i + 2, 4].Value = records[i].FECHA_INGRESO_OI;
                    worksheet.Cells[i + 2, 5].Value = records[i].TELEFONO;
                    worksheet.Cells[i + 2, 6].Value = records[i].CENTRO_CAJVAL;
                    worksheet.Cells[i + 2, 7].Value = records[i].COMUNAS;
                    worksheet.Cells[i + 2, 8].Value = records[i].SEXO;
                    worksheet.Cells[i + 2, 9].Value = records[i].SEXO_OTRO;
                    worksheet.Cells[i + 2, 10].Value = records[i].FECH_NAC_PAT;
                    worksheet.Cells[i + 2, 11].Value = records[i].EDAD;
                    worksheet.Cells[i + 2, 12].Value = records[i].MES;
                    worksheet.Cells[i + 2, 13].Value = records[i].TIPO_CENTRO;
                    worksheet.Cells[i + 2, 14].Value = records[i].TIPO_ATENCION;
                    worksheet.Cells[i + 2, 15].Value = records[i].MATERIA;
                    worksheet.Cells[i + 2, 16].Value = records[i].SUBMATERIA;
                    worksheet.Cells[i + 2, 17].Value = records[i].DURACION;
                    worksheet.Cells[i + 2, 18].Value = records[i].FECHA_DISCADO?.ToString("yyyy-MM-dd HH:mm:ss");
                    // worksheet.Cells[i + 2, 19].Value = records[i].HORA_LLAMADA;
                    worksheet.Cells[i + 2, 19].Value = records[i].P1;
                    worksheet.Cells[i + 2, 20].Value = records[i].P2;
                    worksheet.Cells[i + 2, 21].Value = records[i].P3;
                    worksheet.Cells[i + 2, 22].Value = records[i].P4;
                    worksheet.Cells[i + 2, 23].Value = records[i].P5;
                    // worksheet.Cells[i + 2, 24].Value = records[i].EVALUACION_GLOBAL;
                    worksheet.Cells[i + 2, 24].Value = records[i].OCUPADO;
                    worksheet.Cells[i + 2, 25].Value = records[i].NO_CONTESTA;
                    worksheet.Cells[i + 2, 26].Value = records[i].NO_HAY_LINEA;
                    worksheet.Cells[i + 2, 27].Value = records[i].DESCONECTADO_ANTES;
                    worksheet.Cells[i + 2, 28].Value = records[i].SI_CONTESTA;
                    worksheet.Cells[i + 2, 29].Value = records[i].CANTIDAD_LLAMADAS;
                    // worksheet.Cells[i + 2, 32].Value = records[i].SI_CONTESTA;
                    // worksheet.Cells[i + 2, 33].Value = records[i].ESTADO_ENCUESTA;
                    // worksheet.Cells[i + 2, 33].Value = records[i].CANTIDAD_LLAMADAS;
                }

                // Guardar el archivo
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", fileName);
                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }

        public async Task ExportDataToXlsxAsync2(string parameterValue, string fileName)
        {
            var records = new List<ExportDataModel2>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using var command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = "EXEC [172.16.119.28].[CustomerSoluziona].[dbo].[sp_RS_ReporteIVR_Encuesta_2_Cajval] @ParameterValue";
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@ParameterValue", parameterValue);
                command.CommandTimeout = 120;

                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var record = new ExportDataModel2
                    {
                        Id_Interno = reader.GetString(reader.GetOrdinal("Id_Interno")),
                        Id_Caso = reader.GetString(reader.GetOrdinal("Id_Caso")),
                        REGION = reader.GetString(reader.GetOrdinal("REGION")),
                        DESCRIPCION = reader.GetString(reader.GetOrdinal("DESCRIPCION")),
                        TELEFONO = reader.GetString(reader.GetOrdinal("TELEFONO")),
                        FECHA_INGRESO_CASO = reader.GetString(reader.GetOrdinal("FECHA_INGRESO_CASO")),
                        MES_ESTADISTICO = reader.GetString(reader.GetOrdinal("MES_ESTADISTICO")),
                        SEMESTRE = reader.GetString(reader.GetOrdinal("SEMESTRE")),
                        TIPO_CASO = reader.GetString(reader.GetOrdinal("TIPO_CASO")),
                        FECHA_DISCADO = reader.GetDateTime(reader.GetOrdinal("FECHA_DISCADO")),
                        P1 = reader.GetInt32(reader.GetOrdinal("P1")),
                        P2 = reader.GetInt32(reader.GetOrdinal("P2")),
                        P3 = reader.GetInt32(reader.GetOrdinal("P3")),
                        P4 = reader.GetInt32(reader.GetOrdinal("P4")),
                        OCUPADO = reader.GetInt32(reader.GetOrdinal("OCUPADO")),
                        NO_CONTESTA = reader.GetInt32(reader.GetOrdinal("NO_CONTESTA")),
                        NO_HAY_LINEA = reader.GetInt32(reader.GetOrdinal("NO_HAY_LINEA")),
                        DESCONECTADO_ANTES = reader.GetInt32(reader.GetOrdinal("DESCONECTADO_ANTES")),
                        SI_CONTESTA = reader.GetInt32(reader.GetOrdinal("SI_CONTESTA")),
                        CANTIDAD_LLAMADAS = reader.GetInt32(reader.GetOrdinal("CANTIDAD_LLAMADAS"))
                    };

                    records.Add(record);
                }
            }

            // Crear el archivo XLSX
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Data");

                // Agregar encabezados

                worksheet.Cells[1, 1].Value = "Id_Interno";
                worksheet.Cells[1, 2].Value = "Id_Caso";
                worksheet.Cells[1, 3].Value = "REGION";
                worksheet.Cells[1, 4].Value = "DESCRIPCION";
                worksheet.Cells[1, 5].Value = "TELEFONO";
                worksheet.Cells[1, 6].Value = "FECHA_INGRESO_CASO";
                worksheet.Cells[1, 7].Value = "MES_ESTADISTICO";
                worksheet.Cells[1, 8].Value = "SEMESTRE";
                worksheet.Cells[1, 9].Value = "TIPO_CASO";
                worksheet.Cells[1, 10].Value = "FECHA_DISCADO";

                worksheet.Cells[1, 11].Value = "P1";
                worksheet.Cells[1, 12].Value = "P2";
                worksheet.Cells[1, 13].Value = "P3";
                worksheet.Cells[1, 14].Value = "P4";

                worksheet.Cells[1, 15].Value = "OCUPADO";
                worksheet.Cells[1, 16].Value = "NO_CONTESTA";
                worksheet.Cells[1, 17].Value = "NO_HAY_LINEA";
                worksheet.Cells[1, 18].Value = "DESCONECTADO_ANTES";
                worksheet.Cells[1, 19].Value = "SI_CONTESTA";

                worksheet.Cells[1, 20].Value = "CANTIDAD_LLAMADAS";

                // Agregar datos
                for (int i = 0; i < records.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = records[i].Id_Interno;
                    worksheet.Cells[i + 2, 2].Value = records[i].Id_Caso;
                    worksheet.Cells[i + 2, 3].Value = records[i].REGION;
                    worksheet.Cells[i + 2, 4].Value = records[i].DESCRIPCION;
                    worksheet.Cells[i + 2, 5].Value = records[i].TELEFONO;
                    worksheet.Cells[i + 2, 6].Value = records[i].FECHA_INGRESO_CASO;
                    worksheet.Cells[i + 2, 7].Value = records[i].MES_ESTADISTICO;
                    worksheet.Cells[i + 2, 8].Value = records[i].SEMESTRE;
                    worksheet.Cells[i + 2, 9].Value = records[i].TIPO_CASO;
                    worksheet.Cells[i + 2, 10].Value = records[i].FECHA_DISCADO?.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cells[i + 2, 11].Value = records[i].P1;
                    worksheet.Cells[i + 2, 12].Value = records[i].P2;
                    worksheet.Cells[i + 2, 13].Value = records[i].P3;
                    worksheet.Cells[i + 2, 14].Value = records[i].P4;
                    worksheet.Cells[i + 2, 15].Value = records[i].OCUPADO;
                    worksheet.Cells[i + 2, 16].Value = records[i].NO_CONTESTA;
                    worksheet.Cells[i + 2, 17].Value = records[i].NO_HAY_LINEA;
                    worksheet.Cells[i + 2, 18].Value = records[i].DESCONECTADO_ANTES;
                    worksheet.Cells[i + 2, 19].Value = records[i].SI_CONTESTA;
                    worksheet.Cells[i + 2, 20].Value = records[i].CANTIDAD_LLAMADAS;
                }

                // Guardar el archivo
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", fileName);
                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }
    }
}
