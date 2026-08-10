using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Dashboard;

namespace Dia_CoreAPI.Repositories.Classes.Dashboard
{
    public class MonthAnalysisReportRepository : IMonthAnalysisReportRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public MonthAnalysisReportRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<List<BasicData>> GetDropdownDataAsync(string companyId)
        {
            var parameters = new Dictionary<string, object>();
            var result = await _sqlDataAccess.SelectDataFromSP<BasicData>(
                "AllMasterDataForPurchaseReport",
                parameters,
                companyId);
            return result.ToList();
        }

        public async Task<List<MonthAnalysisReportDto>> GetMonthAnalysisReportAsync(MonthAnalysisRequestDto filter, string companyId)
        {
            string DispCurrency = "";
            string where = "";
            string ReportType = "";
            string GroupBy = filter.GroupBy ?? "Party";

            if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
            {
                where += " AND tsp.TrnDate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";
            }

            long mCurCode = await GetCurCode("MasterCurrencyRate=1", companyId);
            long sCurCode = await GetCurCode("StockCurrencyRate=1", companyId);
            string curCode;

            if (string.IsNullOrEmpty(filter.Currency))
            {
                curCode = sCurCode.ToString();
                where += " AND tsp.CurCode IN (" + sCurCode + "," + mCurCode + ")";
            }
            else
            {
                curCode = filter.Currency;
                where += " and tsp.curcode = " + curCode;
            }

            if (string.IsNullOrEmpty(filter.DispCurrency))
            {
                DispCurrency = "NULL";
            }
            else
            {
                DispCurrency = filter.DispCurrency;
            }

            string curMasterOperator = await GetMCurOperator(curCode, companyId);
            double curMasterCurrencyRate = await GetMCurRate(curCode, companyId);

            if (!string.IsNullOrEmpty(filter.ReportType))
            {
                ReportType = filter.ReportType;
            }

            if (filter.FromDueChkBox == true)
            {
                where += " AND tsp.DueDate  Between ''" + filter.FromDueDate + "'' AND ''" + filter.ToDueDate + "''";
            }

            if (!string.IsNullOrEmpty(filter.Category))
            {
                where += " AND tsp.Category IN (''" + filter.Category + "'')";
            }

            if (!string.IsNullOrEmpty(filter.Party))
            {
                where += " and tparty.AccGrpCode = " + filter.Party;
            }

            if (!string.IsNullOrEmpty(filter.Broker))
            {
                where += " AND tparty.AccGrpCode = " + filter.Broker;
            }

            if (!string.IsNullOrEmpty(filter.Commission))
            {
                where += " and tparty.AccGrpCode = " + filter.Commission;
            }

            if (!string.IsNullOrEmpty(filter.SubType))
            {
                where += " AND tsp.SubType IN (''" + filter.SubType + "'')";
            }

            if (!string.IsNullOrEmpty(filter.Type))
            {
                where += " AND tsp.invtype IN (''" + filter.Type + "'')";
            }

            string sql = " EXEC [SP_MonthlyAnalysisReport] @ReportType='" + ReportType
                       + "' ,@where='" + where
                       + "',@GroupBy='" + GroupBy
                       + "',@CurCode = '" + curCode
                       + "',@SCurCode = '" + sCurCode
                       + "',@MCurCode = '" + mCurCode
                       + "',@MCurRate = '" + curMasterCurrencyRate
                       + "',@CurMasterOperator = '" + curMasterOperator
                       + "',@DispCurrency = '" + DispCurrency + "'";

            var data = await _sqlDataAccess.SelectRawSql<MonthAnalysisReportDto>(sql, companyId);
            return data.ToList();
        }

        private async Task<long> GetCurCode(string where, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='ID',@Where = '" + where + "' ";
            var result = await _sqlDataAccess.SelectRawSqlSingle<string>(sql, companyId);
            return long.TryParse(result, out long id) ? id : 0;
        }

        private async Task<string> GetMCurOperator(string curcode, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='MCurOperator',@Where = ' and ID=" + curcode + "' ";
            var result = await _sqlDataAccess.SelectRawSqlSingle<string>(sql, companyId);
            return result ?? string.Empty;
        }

        private async Task<double> GetMCurRate(string curcode, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='McurBaseRate',@Where = '" + curcode + "' ";
            var result = await _sqlDataAccess.SelectRawSqlSingle<string>(sql, companyId);
            return double.TryParse(result, out double rate) ? rate : 0.0;
        }
    }
}
