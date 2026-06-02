using Microsoft.Extensions.Configuration;

namespace Dia_CoreAPI.Repository
{
    public class MasterRepository
    {
        private readonly IConfiguration _configuration;

        public MasterRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        public string GetConnectionString(string connectionName)
        {
            return _configuration.GetConnectionString(connectionName);
        }
    }
}