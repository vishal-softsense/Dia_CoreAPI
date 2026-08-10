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
    public class SolitaireRateReviseReportRepository : ISolitaireRateReviseReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireRateReviseReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<IEnumerable<SolitaireRateReviseReportRowDto>> GetReportAsync(
            SolitaireRateReviseReportFilterDto filter,
            string companyId)
        {
            var where = BuildRateReviseWhere(filter);

            var dbParams = new Dictionary<string, object>
            {
                { "@Where", where.Replace("''", "'") }
            };

            return await _db.SelectDataFromSP<SolitaireRateReviseReportRowDto>(
                "GetRateRevise",
                dbParams,
                companyId);
        }

        private static string BuildRateReviseWhere(SolitaireRateReviseReportFilterDto filter)
        {
            string strWhere = "";

            try
            {
                if (filter == null) return strWhere;

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                {
                    strWhere +=
                        " AND CONVERT(date,  trr.[Date])  BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";
                }

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = string.Empty;
                    strTmpNewLine = filter.LotNo.Replace(" ", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                    strWhere += " AND [trr].LotNo IN (''" + strTmpNewLine + "'')";
                }

                AppendInClause(ref strWhere, filter.Shape, "tshp.ShapeId");
                AppendInClause(ref strWhere, filter.Color, "titem.ColorId");
                AppendInClause(ref strWhere, filter.Clarity, "titem.ClarityId");
                AppendInClause(ref strWhere, filter.Size, "titem.SizeId");
                AppendInClause(ref strWhere, filter.Proportion, "titem.PropId");
                AppendInClause(ref strWhere, filter.Polish, "titem.PolishId");
                AppendInClause(ref strWhere, filter.Symmetry, "titem.SymId");
                AppendInClause(ref strWhere, filter.Fluorescence, "titem.FLId");
                AppendInClause(ref strWhere, filter.LAB, "titem.CertId");
            }
            catch
            {
            }

            return strWhere;
        }

        private static void AppendInClause(
            ref string strWhere,
            List<string>? values,
            string column)
        {
            if (values == null || values.Count == 0) return;

            string joined = "";
            values.ForEach(el => joined += "''" + el + "'',");

            if (joined.Trim().Length > 1)
                joined = joined.Remove(joined.Length - 1);

            if (joined.Length > 0)
                strWhere += " And " + column + " In (" + joined + ")";
        }
    }
}
