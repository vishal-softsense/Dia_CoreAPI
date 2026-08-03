using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Reports
{
    public class ParcelStockSearchReportRepository : IParcelStockSearchReportRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ParcelStockSearchReportRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        #region Permission

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

        #endregion

        #region Dropdown

        public async Task<List<BasicDataDto>> GetParcelStockSearchDropdownAsync(string companyId)
        {
            var param = new Dictionary<string, object>();

            var result = await _sqlDataAccess.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForAdminStockSearch",
                param,
                companyId);

            return result.ToList();
        }

        #endregion

        #region Report

        public async Task<List<ParcelStockrpt>> GetParcelStockSearchDataAsync(
            string date,
            string where,
            string askingQry,
            string companyId)
        {
            var param = new Dictionary<string, object>();

            param.Add("@Date", $"'{date}'");
            param.Add("@Where", where);
            param.Add("@AskingQry", askingQry);

            var result = await _sqlDataAccess.SelectDataFromSP<ParcelStockrpt>(
                "ParcelGetStockReportData",
                param,
                companyId);

            return result.ToList();
        }

        #endregion

        public async Task<IEnumerable<BasicDataDto>> GetParcelLotNoListAsync(string searchby, string companyId)
        {
            string strWhere = " AND ISNULL(SinglePcs,0) = 0 ";
            if (!string.IsNullOrEmpty(searchby))
            {
                var cleaned = searchby.Replace("\r\n", ",").Replace("\n", ",").Trim();
                var items = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(i => i.Trim())
                                   .Where(i => !string.IsNullOrEmpty(i))
                                   .ToList();

                if (items.Any())
                {
                    if (items.Count == 1)
                    {
                        string safeItem = items[0].Replace("'", "''");
                        strWhere += $" AND (Name LIKE '%{safeItem}%' OR TextName LIKE '%{safeItem}%') ";
                    }
                    else
                    {
                        string joined = string.Join("','", items.Select(i => i.Replace("'", "''")));
                        strWhere += " AND Name IN ('" + joined + "') ";
                    }
                }
            }

            var dbParams = new Dictionary<string, object>
            {
                { "@strWhere", strWhere }
            };

            return await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("GetDiamondLotDropdown", dbParams, companyId);
        }

        public async Task<bool> UpdateLocationAsync(ParcelStockLocationUpdateDTO dto, string companyId)
        {
            if (dto.LotNumbers == null || dto.LotNumbers.Count == 0)
                return false;

            foreach (var lot in dto.LotNumbers)
            {
                var param = new Dictionary<string, object>
                {
                    { "@City", dto.City ?? "" },
                    { "@Country", dto.Country ?? "" },
                    { "@State", dto.State ?? "" },
                    { "@Lot", lot },
                    { "@Type", "location" }
                };

                await _sqlDataAccess.ExecuteSP("SP_InsertUpdateLocation", param, companyId);
            }
            return true;
        }

        public async Task<bool> UpdateBoxAndGroupAsync(ParcelStockBoxGroupUpdateDTO dto, string companyId)
        {
            if (dto.LotNumbers == null || dto.LotNumbers.Count == 0)
                return false;

            foreach (var lot in dto.LotNumbers)
            {
                var param = new Dictionary<string, object>
                {
                    { "@boxname", dto.BoxName ?? "" },
                    { "@groupname", dto.Group ?? "" },
                    { "@Lot", lot },
                    { "@Type", "box-group-update" }
                };

                await _sqlDataAccess.ExecuteSP("SP_InsertUpdateLocation", param, companyId);
            }
            return true;
        }
    }
}