using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;

namespace Dia_CoreAPI.Services.Classes.StockAccount
{
    public class AccGrpMasterService : IAccGrpMasterService
    {
        private readonly IAccGrpMasterRepository _repository;

        public AccGrpMasterService(IAccGrpMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AccGrpMasterListDto>> GetAccGrpMasterListAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetAccGrpMasterListAsync(companyId, whereClause);
        }

        public async Task<IEnumerable<BasicDataDto>> GetAccGrpMasterDropdownAsync(string companyId)
        {
            return await _repository.GetAccGrpMasterDropdownAsync(companyId);
        }

        public async Task<AccGrpMasterDto?> GetAccGrpMasterByIdAsync(int id, string companyId)
        {
            return await _repository.GetAccGrpMasterByIdAsync(id, companyId);
        }

        public async Task<bool> CheckAccGrpNameExistsAsync(string name, string companyId, int? excludeId = null)
        {
            return await _repository.CheckAccGrpNameExistsAsync(name, companyId, excludeId);
        }

        public async Task<float> GetMcurBaseRateAsync(string code, string companyId)
        {
            return await _repository.GetMcurBaseRateAsync(code, companyId);
        }

        public async Task<float> GetScurBaseRateAsync(string code, string companyId)
        {
            return await _repository.GetScurBaseRateAsync(code, companyId);
        }

        public async Task<string> GetDefaultCurrencyCodeAsync(string companyId)
        {
            return await _repository.GetDefaultCurrencyCodeAsync(companyId);
        }

        public async Task<string> GetMCurOperatorAsync(int currencyId, string companyId)
        {
            return await _repository.GetMCurOperatorAsync(currencyId, companyId);
        }

        public async Task<string> GetSCurOperatorAsync(int currencyId, string companyId)
        {
            return await _repository.GetSCurOperatorAsync(currencyId, companyId);
        }

        public async Task<(bool Success, string Message)> AddAccGrpMasterAsync(AccGrpMasterDto model, string companyId)
        {
            if (string.IsNullOrWhiteSpace(model.AccGrpName))
            {
                return (false, "Account Group Name is required.");
            }

            bool exists = await _repository.CheckAccGrpNameExistsAsync(model.AccGrpName, companyId);
            if (exists)
            {
                return (false, "Account/Group Already Created.");
            }

            bool success = await _repository.SaveAccGrpMasterAsync(model, "insert", companyId);
            if (success)
            {
                return (true, "Account Group saved successfully.");
            }
            return (false, "Failed to save Account Group.");
        }

        public async Task<(bool Success, string Message)> UpdateAccGrpMasterAsync(AccGrpMasterDto model, string companyId)
        {
            if (string.IsNullOrWhiteSpace(model.AccGrpName))
            {
                return (false, "Account Group Name is required.");
            }

            bool exists = await _repository.CheckAccGrpNameExistsAsync(model.AccGrpName, companyId, model.AccGrpCode);
            if (exists)
            {
                return (false, "Account/Group Already Created.");
            }

            bool success = await _repository.SaveAccGrpMasterAsync(model, "update", companyId);
            if (success)
            {
                return (true, "Account Group updated successfully.");
            }
            return (false, "Failed to update Account Group.");
        }

        public async Task<(bool Success, string Message)> DeleteAccGrpMasterAsync(int id, string companyId)
        {
            string countStr = await _repository.GetAccCountAsync(id, companyId);
            if (countStr != "0")
            {
                return (false, "Record is in use, You can not delete record.");
            }

            bool success = await _repository.DeleteAccGrpMasterAsync(id, companyId);
            if (success)
            {
                return (true, "Account Group deleted successfully.");
            }
            return (false, "Failed to delete Account Group.");
        }

        public async Task<string> GetAccEffCodeAsync(int accType, string companyId)
        {
            return await _repository.GetAccEffCodeAsync(accType, companyId);
        }

        public async Task<string> GetAccEffSideAsync(int accType, string companyId)
        {
            return await _repository.GetAccEffSideAsync(accType, companyId);
        }
    }
}
