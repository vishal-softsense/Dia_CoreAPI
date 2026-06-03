using System;
using System.Data;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Dia_CoreAPI.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<LoginResponseModel?> GetUserByUsernameAndCompanyAsync(string username, string companyId)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();

            // Raw SQL query to fetch user credentials matching username and companyID.
            // Under a clean architecture, database schema query logic is restricted to the Repository layer.
            const string query = @"
                SELECT *
                FROM TabUserMaster
                WHERE UserName = @UserName
                AND CompanyID = @CompanyID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", username);
            command.Parameters.AddWithValue("@CompanyID", companyId);

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new LoginResponseModel
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"]?.ToString() ?? string.Empty,
                    FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                    LastName = reader["LastName"]?.ToString() ?? string.Empty,
                    Title = reader["Title"]?.ToString() ?? string.Empty,
                    UserTypeId = Convert.ToInt32(reader["UserTypeId"]),
                    CompanyID = Convert.ToInt32(reader["CompanyID"]),
                    ProfileImage = reader["ProfileImage"]?.ToString() ?? string.Empty
                };
            }

            return null;
        }
    }
}
