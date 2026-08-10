using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitarieGradingComparisonReportRepository : ISolitarieGradingComparisonReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitarieGradingComparisonReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return await _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<IEnumerable<GradingComparisonReportRowDto>> GetReportAsync(
            GradingComparisonReportFilterDto filter,
            string companyId)
        {
            var where = BuildGradingComparisonWhere(filter);

            var dbParams = new Dictionary<string, object>
            {
                { "@Where", where.Replace("''", "'") }
            };

            return await _db.SelectDataFromSP<GradingComparisonReportRowDto>(
                "GetGradingComparision",
                dbParams,
                companyId);
        }

        private static string BuildGradingComparisonWhere(GradingComparisonReportFilterDto filter)
        {
            string strWhere = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Replace(" ", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                    strWhere += " AND a.LotNo  IN (''" + strTmpNewLine + "'')";
                }

                AppendInClause(ref strWhere, filter.Shape, "a.Shape");
                AppendInClause(ref strWhere, filter.Color, "a.Color");
                AppendInClause(ref strWhere, filter.Clarity, "a.Clarity");
                AppendInClause(ref strWhere, filter.Size, "a.Size");
                AppendInClause(ref strWhere, filter.Proportion, "a.Proportion");
                AppendInClause(ref strWhere, filter.Polish, "a.Polish");
                AppendInClause(ref strWhere, filter.Symmetry, "a.Symm");
                AppendInClause(ref strWhere, filter.Fluorescence, "a.FLCode");

                if (filter.LAB != null && filter.LAB.Any())
                {
                    string lab = "";
                    filter.LAB.ForEach(el => lab += "''" + el + "[]'',");
                    if (lab.Trim().Length > 1)
                        lab = lab.Remove(lab.Length - 1);

                    if (lab.Length > 0)
                        strWhere += " And a.grader In (" + lab + ")";
                }

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND a.Gdate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";
            }
            catch
            {
            }

            return strWhere;
        }

        private static void AppendInClause(ref string strWhere, List<string>? values, string column)
        {
            if (values == null || !values.Any()) return;

            string joined = "";
            values.ForEach(el => joined += "''" + el + "'',");
            if (joined.Trim().Length > 1)
                joined = joined.Remove(joined.Length - 1);

            if (joined.Length > 0)
                strWhere += " And " + column + " In (" + joined + ")";
        }
    }
}
