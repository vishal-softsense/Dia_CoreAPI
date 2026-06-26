using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public InvoiceRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<int> CheckUserPermissionAsync(
             string userTypeId,
             string formName,
             string actionType,
             string companyId)
        {
            string where = string.Empty;

            if (actionType == "insert")
                where = " InsertData = 1 ";
            else if (actionType == "edit")
                where = " EditData = 1 ";
            else if (actionType == "delete")
                where = " DeleteData = 1 ";
            else if (actionType == "navigate")
                where = " NavigateData = 1 ";
            else if (actionType == "select")
                where = " SelectData = 1 ";

            var parameters = new Dictionary<string, object>
            {
                { "@UserTypeId", "'" + userTypeId + "'" },
                { "@FormName", "'" + formName + "'" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }

        #region Term Master
        public async Task<List<TermMaster>> GetTermMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Term" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<TermMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveTermMasterAsync(TermMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@TermId", model.TermID },
                { "@TermName", model.TermName ?? (object)DBNull.Value },
                { "@TermPriority", model.TermPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_TermMaster", param, companyId);
            return true;
        }
        #endregion

        #region Type Master
        public async Task<List<TypeMaster>> GetTypeMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Type" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<TypeMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveTypeMasterAsync(TypeMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@TypeID", model.TypeID },
                { "@TypeName", model.TypeName ?? (object)DBNull.Value },
                { "@TypePriority", model.TypePriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_TypeMaster", param, companyId);
            return true;
        }
        #endregion

        #region SubType Master
        public async Task<List<SubTypeMaster>> GetSubTypeMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "SubType" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<SubTypeMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveSubTypeMasterAsync(SubTypeMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@SubTypeID", model.SubTypeID },
                { "@SubTypeName", model.SubTypeName ?? (object)DBNull.Value },
                { "@SubTypePriority", model.SubTypePriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionSubType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_SubTypeMaster", param, companyId);
            return true;
        }
        #endregion

        #region Currency Master
        public async Task<List<CurrencyMaster>> GetCurrencyMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Currency" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<CurrencyMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        #endregion
    }
}
