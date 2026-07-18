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

        // Matches MVC ReportProcessor.PurchaseReportDropDown() which calls "AllMasterDataForPurchaseReport"
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

            // Matches MVC: if (filter.FromDate != null && filter.ToDate != null)
            if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
            {
                where += " AND tsp.TrnDate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";
            }

            // Matches MVC: MCurCode = GetCurCode("MasterCurrencyRate=1"); SCurCode = GetCurCode("StockCurrencyRate=1");
            long mCurCode = await GetCurCode("MasterCurrencyRate=1", companyId);
            long sCurCode = await GetCurCode("StockCurrencyRate=1", companyId);
            string curCode;

            // Matches MVC: if (filter.Currency == null)
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

            // Matches MVC: if (filter.DispCurrency == null)
            if (string.IsNullOrEmpty(filter.DispCurrency))
            {
                DispCurrency = "NULL";
            }
            else
            {
                DispCurrency = filter.DispCurrency;
            }

            // Matches MVC: CurMasterOperator = GetMCurOperator(CurCode); CurMasterCurrencyRate = GetMCurRate(CurCode);
            string curMasterOperator = await GetMCurOperator(curCode, companyId);
            double curMasterCurrencyRate = await GetMCurRate(curCode, companyId);

            if (!string.IsNullOrEmpty(filter.ReportType))
            {
                ReportType = filter.ReportType;
            }

            // Matches MVC: if (filter.FromDueChkBox == true)
            if (filter.FromDueChkBox == true)
            {
                where += " AND tsp.DueDate  Between ''" + filter.FromDueDate + "'' AND ''" + filter.ToDueDate + "''";
            }

            // Matches MVC: if (filter.Category != null)
            if (!string.IsNullOrEmpty(filter.Category))
            {
                where += " AND tsp.Category IN (''" + filter.Category + "'')";
            }

            // Matches MVC: if (filter.Party != null)
            if (!string.IsNullOrEmpty(filter.Party))
            {
                where += " and tparty.AccGrpCode = " + filter.Party;
            }

            // Matches MVC: if (filter.Broker != null)
            if (!string.IsNullOrEmpty(filter.Broker))
            {
                where += " AND tparty.AccGrpCode = " + filter.Broker;
            }

            // Matches MVC: if (filter.Commission != null)
            if (!string.IsNullOrEmpty(filter.Commission))
            {
                where += " and tparty.AccGrpCode = " + filter.Commission;
            }

            // Matches MVC: if (filter.SubType != null)
            if (!string.IsNullOrEmpty(filter.SubType))
            {
                where += " AND tsp.SubType IN (''" + filter.SubType + "'')";
            }

            // Matches MVC: if (filter.Type != null)
            if (!string.IsNullOrEmpty(filter.Type))
            {
                where += " AND tsp.invtype IN (''" + filter.Type + "'')";
            }

            // Build raw SQL exactly as MVC does in ReportProcessor.GetMonthAnalysisReport()
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

        // Matches MVC ReportProcessor.GetCurCode(string where)
        // MVC: EXEC [GetCurrencyDetail] @Type='ID', @Where='MasterCurrencyRate=1'
        private async Task<long> GetCurCode(string where, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='ID',@Where = '" + where + "' ";
            var result = await _sqlDataAccess.SelectRawSqlSingle<string>(sql, companyId);
            return long.TryParse(result, out long id) ? id : 0;
        }

        // Matches MVC ReportProcessor.GetMCurOperator(string curcode)
        // MVC: EXEC [GetCurrencyDetail] @Type='MCurOperator', @Where=' and ID=<curcode>'
        private async Task<string> GetMCurOperator(string curcode, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='MCurOperator',@Where = ' and ID=" + curcode + "' ";
            var result = await _sqlDataAccess.SelectRawSqlSingle<string>(sql, companyId);
            return result ?? string.Empty;
        }

        // Matches MVC ReportProcessor.GetMCurRate(string curcode)
        // MVC: EXEC [GetCurrencyDetail] @Type='McurBaseRate', @Where='<curcode>'  (raw curcode, not " and ID=X")
        private async Task<double> GetMCurRate(string curcode, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='McurBaseRate',@Where = '" + curcode + "' ";
            var result = await _sqlDataAccess.SelectRawSqlSingle<string>(sql, companyId);
            return double.TryParse(result, out double rate) ? rate : 0.0;
        }
    }
}
