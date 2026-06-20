
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class AllMasterRepository : IAllMasterRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public AllMasterRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<List<AllMasterDto>> GetAllMasterListAsync(string UserTypeId, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@UserTypeId", UserTypeId);

            var result =
                await _sqlDataAccess.SelectDataFromSP<AllMasterDto>(
                    "SP_AllMasterList",
                    param,
                    companyId);

            return result.ToList();
        }
    }
}
