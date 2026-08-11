using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Payable_Receivables;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Payable_Receivables
{
    public class SalePurchaseSummaryReportRepository : ISalePurchaseSummaryReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SalePurchaseSummaryReportRepository(ISqlDataAccess db)
        {
            _db = db;
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

            var result = await _db.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownDataAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForSalePurchaseSummaryReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<List<SalePurchaseSummaryReportResultDto>> GetSalePurchaseSummaryDataAsync(
            string where,
            string companyId)
        {
            string sql = @" EXEC [GetSalePurSummaryReportData] @Where = '" + where + "' ";
            var result = await _db.SelectRawSql<SalePurchaseSummaryReportResultDto>(sql, companyId);
            return result.ToList();
        }
    }
}
