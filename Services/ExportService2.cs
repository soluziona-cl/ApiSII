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
    public class ExportService2
    {
        private readonly string _connectionString;

        public ExportService2(string connectionString)
        {
            _connectionString = connectionString;
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
            using var package = new ExcelPackage();
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
