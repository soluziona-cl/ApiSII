// Repositories/FtpControlRepository.cs
using Dapper;
using CajValp.Models;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace CajValp.Repositories
{
    public class FtpControlRepository
    {
        private readonly string _connectionString;

        public FtpControlRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddRecordAsync(FtpControlRecord record)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "INSERT INTO [172.16.119.28].[CustomerSoluziona].[dbo].[FtpControl] (FileName, ImportDate, IsProcessed) VALUES (@FileName, @ImportDate, @IsProcessed)";
            return await connection.ExecuteAsync(sql, record);
        }
    }
}
