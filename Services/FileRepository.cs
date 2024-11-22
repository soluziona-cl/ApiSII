using CajValp.Interfaces;
using CajValp.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CajValp.Services
{
    public class FileRepository : IFileRepository
    {
        private readonly string _connectionString;

        public FileRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SaveFileRecordAsync(FileModel fileModel)
        {
            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            string insertQuery = @"update [172.16.119.28].[CustomerSoluziona].[dbo].[FtpControl] set IsProcessed = 1 where FileName = @FileName";

            using SqlCommand command = new(insertQuery, connection);
            command.Parameters.AddWithValue("@FileName", fileModel.FileName);

            await command.ExecuteNonQueryAsync();
        }
    }
}
