using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    public class StockPriceComparisonRepository : IStockPriceComparisonRepository
    {
        private readonly ISqlDataAccess _db;

        public StockPriceComparisonRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            string where = actionType switch
            {
                "insert" => " InsertData = 1 ",
                "edit" => " EditData = 1 ",
                "delete" => " DeleteData = 1 ",
                "navigate" => " NavigateData = 1 ",
                "select" => " SelectData = 1 ",
                _ => string.Empty
            };

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

        /// <summary>Matches MVC ReportProcessor.StockPriceComparisonDropDown → SP_StockPriceComparison Type=Filter.</summary>
        public async Task<List<BasicDataDto>> GetFilterDropdownAsync(string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Type", "Filter" },
                { "@where", "" }
            };

            var result = await _db.SelectDataFromSP<BasicDataDto>(
                "SP_StockPriceComparison",
                parameters,
                companyId);

            return result.ToList();
        }

        /// <summary>Matches MVC ReportProcessor.GetStockPriceComparisonData (raw EXEC with embedded where).</summary>
        public async Task<List<StockPriceComparisonReportDto>> GetReportDataAsync(
            string where,
            string type,
            string companyId)
        {
            var sql = @" EXEC [SP_StockPriceComparison] @where = '" + where + "' ,@Type='" + type + "'";
            var result = await _db.SelectRawSql<StockPriceComparisonReportDto>(sql, companyId);
            return result.ToList();
        }
    }
}
