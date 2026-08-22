using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    public class MissingStockRepository : IMissingStockRepository
    {
        private readonly ISqlDataAccess _db;

        public MissingStockRepository(ISqlDataAccess db)
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

        /// <summary>Matches MVC ReportProcessor.GetMissingStockSearchData → Sp_MissingStock.</summary>
        public async Task<List<MissingStockReportDto>> GetMissingStockDataAsync(
            string stockType,
            string where,
            string companyId)
        {
            var sql = @"EXEC [Sp_MissingStock] @StockType = '" + stockType + "', @Where = '" + where + "'";
            var result = await _db.SelectRawSql<MissingStockReportDto>(sql, companyId);
            return result.ToList();
        }
    }
}
