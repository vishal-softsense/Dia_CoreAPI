using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Payable_Receivables;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Payable_Receivables
{
    public class BrokerageOutstandingReportRepository : IBrokerageOutstandingReportRepository
    {
        private readonly ISqlDataAccess _db;

        public BrokerageOutstandingReportRepository(ISqlDataAccess db)
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
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<List<BrokerageOutstandingReportResultDto>> GetBrokerageOutstandingDataAsync(
            string where,
            long curCode,
            long sCurCode,
            long mCurCode,
            string curMasterOperator,
            double mCurRate,
            string companyId)
        {
            string sql =
                @"Exec [GetOutstandingReportData] @Type = 'BrokerageRpt',@Where = '" + where
                + "', @CurCode = '" + curCode
                + "',@SCurCode = '" + sCurCode
                + "',@MCurCode = '" + mCurCode
                + "',@MCurRate = '" + mCurRate.ToString(CultureInfo.InvariantCulture)
                + "',@CurMasterOperator = '" + curMasterOperator + "'";

            var result = await _db.SelectRawSql<BrokerageOutstandingReportResultDto>(sql, companyId);
            return result.ToList();
        }

        public async Task<long> GetCurCodeAsync(string where, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='ID',@Where = '" + where + "' ";
            var result = await _db.SelectRawSqlSingle<string>(sql, companyId);
            return long.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out long id) ? id : 0;
        }

        public async Task<string> GetMCurOperatorAsync(string curCode, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='MCurOperator',@Where = ' and ID=" + curCode + "' ";
            var result = await _db.SelectRawSqlSingle<string>(sql, companyId);
            return result ?? string.Empty;
        }

        public async Task<double> GetMCurRateAsync(string curCode, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='McurBaseRate',@Where = '" + curCode + "' ";
            var result = await _db.SelectRawSqlSingle<string>(sql, companyId);
            return double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out double rate) ? rate : 0.0;
        }
    }
}
