using System.Threading.Tasks;
using Dia_CoreAPI.Models;

namespace Dia_CoreAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<(LoginResponseModel? User, string ErrorMessage)> GetUserByCredentialsAsync(string username, string password, string companyId);
    }
}
