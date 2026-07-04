using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Microsoft.AspNetCore.Connections;
using System.Data;

namespace Dia_CoreAPI.Repositories.Classes.StockAccount
{
    public class JewStockMasterRepository : IJewStockMasterRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

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

            var result =
                await _sqlDataAccess.SelectDataFromSP<PermissionCheck>("GetUserPermission", parameters, companyId);
            return result.Count();
        }

        public JewStockMasterRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }
        public async Task<IEnumerable<JewStockMasterDto>> GetMountingStockAsync(string companyId, string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Where", whereClause);
            param.Add("@Type", "JewStock");

            return await _sqlDataAccess.SelectDataFromSP<JewStockMasterDto>(
                "GetALLMasterList",
                param,
                companyId);
        }
        public async Task<IEnumerable<JewMetalMasterDto>> GetJewMetalMasterAsync(string companyId, string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Where", "");
            param.Add("@Type", "JewMetal");

            var result = await _sqlDataAccess.SelectDataFromSP<JewMetalMasterDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<IEnumerable<JewColorMasterDto>> GetJewColorMasterAsync(string companyId, string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Where", "");
            param.Add("@Type", "JewColor");

            var result = await _sqlDataAccess.SelectDataFromSP<JewColorMasterDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<IEnumerable<GetJewPurityDto>> GetJewPurityAsync(string companyId, string metalType)
        {
            var param = new Dictionary<string, object>();

            //param.Add("@Where", $"and Metal=''{metalType}''");
            param.Add("@Where", $"and Metal='{metalType}'");
            param.Add("@Type", "GetJewPurity");
            var result = await _sqlDataAccess.SelectDataFromSP<GetJewPurityDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<double> GetPureRatioAsync(string companyId, string metalType, string metalPurity)
        {
            var param = new Dictionary<string, object>();

            param.Add("@Type", "GetMetalPurity");
            param.Add("@Where",
                $"and Metal='{metalType}' and MetalPurityID='{metalPurity}'");

            return await _sqlDataAccess.GetScalarDataFromSP<double>(
                "GetALLMasterList",
                param,
                companyId);
        }
        public async Task<IEnumerable<BasicDataDto>> GetItemMasterDropdownAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            return await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("GetAllMasterDataForItemMaster", param, companyId);
        }
        public async Task<List<JewCategoryDto>> GetJewCategoryMasterAsync(string companyId,string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Type", "JewCategory");
            param.Add("@Where", whereClause);

            var result = await _sqlDataAccess.SelectDataFromSP<JewCategoryDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<List<JewSubCategoryDto>> GetJewSubCategoryMasterAsync(string companyId,string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Type", "JewSubCategory");
            param.Add("@Where", whereClause);

            var result = await _sqlDataAccess.SelectDataFromSP<JewSubCategoryDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<List<SettingMasterDto>> GetJewSettingValueAsync(string companyId)
        {
            var param = new Dictionary<string, object>();

            param.Add("@Type", "Setting");
            param.Add("@Where", "");

            var result = await _sqlDataAccess.SelectDataFromSP<SettingMasterDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<AddMountingStockDto> GetAddMountingStockDataAsync(string companyId)
        {
            var itemMaster = (await GetItemMasterDropdownAsync(companyId)).ToList();

            var category = (await GetJewCategoryMasterAsync(companyId)).ToList();

            var subCategory = (await GetJewSubCategoryMasterAsync(companyId)).ToList();

            var metal = (await GetJewMetalMasterAsync(companyId)).ToList();

            var color = (await GetJewColorMasterAsync(companyId)).ToList();

            var setting = (await GetJewSettingValueAsync(companyId)).ToList();

            return new AddMountingStockDto
            {
                Shape = itemMaster.Where(x => x.Type == "Shape").ToList(),
                Color = itemMaster.Where(x => x.Type == "Color").ToList(),
                Size = itemMaster.Where(x => x.Type == "Size").ToList(),
                Clarity = itemMaster.Where(x => x.Type == "Clarity").ToList(),
                Sieve = itemMaster.Where(x => x.Type == "Sieve").ToList(),
                Category = itemMaster.Where(x => x.Type == "Category").ToList(),

                JewCategories = category,

                JewSubCategories = subCategory,

                JewMetalType = metal,

                JewColor = color,

                JewSettingValue = setting
                    .Where(x => x.Name == "Jew Setting Type")
                    .Select(x => x.Value)
                    .Distinct()
                    .ToList(),

                DecimalinInvoiceAmount = setting
                    .Where(x => x.Name == "JewelleryMetalAmountCalculateOnGoldWeight")
                    .Select(x => x.Value)
                    .FirstOrDefault()
            };
        }
    }
}