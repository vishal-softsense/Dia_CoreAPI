using System.Threading.Tasks;
using Dia_CoreAPI.Models;

namespace Dia_CoreAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<LoginResponseModel?> GetUserByUsernameAndCompanyAsync(string username, string companyId);
    }
}
