using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Classes.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class _4CMasterService : I4CMasterService
    {
        private readonly I4CMasterRepository _repository;

        public _4CMasterService(I4CMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            return await _repository.CheckUserPermissionAsync(
                userTypeId,
                formName,
                actionType,
                companyId);
        }

        public async Task<List<ShapeMaster>> GetShapeMasterListAsync(
            string where,
            string companyId)
        {
            return await _repository.GetShapeMasterListAsync(
                where,
                companyId);
        }
        public async Task<List<ColorMaster>> GetColorMasterListAsync(
            string where,
            string companyId)
        {
            return await _repository.GetColorMasterListAsync(
                where,
                companyId);
        }

        public async Task<bool> SaveShapeMasterAsync(
            ShapeMaster model,
            string userId,
            string companyId,
            string actionType)
        {
            return await _repository.SaveShapeMasterAsync(
                model,
                userId,
                companyId,
                actionType);
        }

        public async Task<bool> SaveColorMasterAsync(
            ColorMaster model,
            string userId,
            string companyId,
            string actionType)
        {
            return await _repository.SaveColorMasterAsync(
                model,
                userId,
                companyId,
                actionType);
        }

        public async Task<bool> DeleteShapeAsync(
    int id,
    string userTypeId,
    string userId,
    string companyId)
        {
            return await _repository.DeleteShapeAsync(
                id,
                userTypeId,
                userId,
                companyId);
        }
    }
}