using Dia_CoreAPI.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Dia_CoreAPI.Repositories.Implementations
{
    public class MasterRepository : IMasterRepository
    {
        private readonly IConfiguration _configuration;

        public MasterRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public string GetConnectionString(string connectionName)
        {
            return _configuration.GetConnectionString(connectionName) ?? string.Empty;
        }
    }
}
