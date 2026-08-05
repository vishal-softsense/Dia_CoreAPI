using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireConsumptionReportRepository : ISolitaireConsumptionReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireConsumptionReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            // Same as MVC AdminStockSearchDropDown()
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForAdminStockSearch",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<IEnumerable<SolitaireConsumptionReportRowDto>> GetReportAsync(
            SolitaireConsumptionReportFilterDto filter,
            string companyId)
        {
            var (where, date, saleWhere) = BuildFilterParams(filter);

            // Ported from MVC ReportProcessor.GetStockConsumptionReport — ADO.NET (no Dapper)
            string sql =
                " EXEC [SP_StockConsumptionReport]  @Where = '" + where +
                "' ,@Date ='" + date +
                "',@saleWhere='" + saleWhere + "'";

            return await _db.SelectRawSql<SolitaireConsumptionReportRowDto>(sql, companyId);
        }

        /// <summary>
        /// Ported from MVC ReportController.StockConsumptionReport(StockConsumptionFilter)
        /// </summary>
        private static (string where, string date, string saleWhere) BuildFilterParams(
            SolitaireConsumptionReportFilterDto filter)
        {
            var where = "";
            var date = "";
            var saleWhere = "";

            if (filter == null)
                return (where, date, saleWhere);

            try
            {
                AppendInClause(ref where, filter.Shape, "Shp");
                AppendInClause(ref where, filter.Color, "Col");
                AppendInClause(ref where, filter.Clarity, "Cla");
                AppendInClause(ref where, filter.Size, "SizeId");
                AppendInClause(ref where, filter.Polish, "Pol");
                AppendInClause(ref where, filter.Proportion, "Prop");
                AppendInClause(ref where, filter.Symmetry, "Sym");

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Replace(" ", "\n");
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                    where += " AND Lotno IN (''" + strTmpNewLine + "'')";
                }

                if (!string.IsNullOrEmpty(filter.ToDate))
                {
                    date = filter.ToDate;
                }

                if (filter.FromDueChkBox)
                {
                    saleWhere +=
                        "and sa.MinSPDate Between ''" + filter.FromDateDue +
                        "'' AND ''" + filter.ToDateDue + "''";
                }
            }
            catch
            {
                // MVC swallows exceptions and returns partial where
            }

            return (where, date, saleWhere);
        }

        private static void AppendInClause(ref string where, List<string>? values, string column)
        {
            if (values == null || values.Count == 0)
                return;

            try
            {
                string joined = "";
                values.ForEach(el => joined += "''" + el + "'',");
                if (joined.Trim().Length > 1)
                    joined = joined.Remove(joined.Length - 1);

                if (!string.IsNullOrEmpty(joined))
                    where += " And " + column + " In ( " + joined + ")";
            }
            catch
            {
                // MVC swallows
            }
        }
    }
}
