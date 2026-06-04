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

        public async Task<(LoginResponseModel? User, string ErrorMessage)> GetUserByCredentialsAsync(string username, string password, string companyId)
        {
            // 1. First connect to the Master Database to get the Client's Connection String
            using var masterConnection = (SqlConnection)_connectionFactory.CreateConnection("MasterConnection");
            
            const string masterQuery = @"
                SELECT ConnectionString 
                FROM TabCompanyMaster 
                WHERE CompanyID = @CompanyID";

            using var masterCommand = new SqlCommand(masterQuery, masterConnection);
            masterCommand.Parameters.AddWithValue("@CompanyID", companyId);

            if (masterConnection.State != ConnectionState.Open)
            {
                await masterConnection.OpenAsync();
            }

            var connStringResult = await masterCommand.ExecuteScalarAsync();
            if (connStringResult == null || string.IsNullOrWhiteSpace(connStringResult.ToString()))
            {
                // Company not found or no connection string mapped
                return (null, "Invalid Company ID");
            }

            string clientConnectionString = connStringResult.ToString()!;

            // Fix for 'The certificate chain was issued by an authority that is not trusted'
            // Older connection strings in the DB might not have TrustServerCertificate=True
            var builder = new SqlConnectionStringBuilder(clientConnectionString);
            builder.TrustServerCertificate = true;
            
            // 2. Now connect to the specific client database using their full connection string
            using var clientConnection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(builder.ConnectionString);
            
            const string query = @"
                SELECT *
                FROM TabUserMaster
                WHERE UserName = @UserName
                AND CompanyID = @CompanyID";

            using var command = new SqlCommand(query, clientConnection);
            command.Parameters.AddWithValue("@UserName", username);
            command.Parameters.AddWithValue("@CompanyID", companyId);

            if (clientConnection.State != ConnectionState.Open)
            {
                await clientConnection.OpenAsync();
            }

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string dbPassword = reader["UserPassword"]?.ToString() ?? string.Empty;
                if (dbPassword != password)
                {
                    return (null, "Invalid Password");
                }

                var user = new LoginResponseModel
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
                
                return (user, string.Empty);
            }

            return (null, "Invalid Username");
        }
    }
}
