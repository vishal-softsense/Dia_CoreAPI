using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories
{
    public class ParcelStockRegisterReportRepository : IParcelStockRegisterReportRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ParcelStockRegisterReportRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        #region Permission Check
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

        public async Task<IEnumerable<BasicDataDto>> GetPurchaseReportDropDownAsync(string companyId)
        {
            return await _sqlDataAccess.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetParcelLotNoListAsync(string searchby, string companyId)
        {
            // Match MVC Getparcealdiamonlotnods + GetLotNoDiamondForDropdown
            string strWhere = " AND ISNULL(SinglePcs,0) = 0 ";
            if (!string.IsNullOrEmpty(searchby))
            {
                var cleaned = searchby
                    .Replace("\r\n", ",")
                    .Replace("\n", ",")
                    .Replace("\r", ",")
                    .Trim();

                var items = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrEmpty(i))
                    .ToList();

                if (items.Count == 1)
                {
                    string safeItem = items[0].Replace("'", "''");
                    strWhere += " AND (Name LIKE ''%" + safeItem + "%'' OR TextName LIKE ''%" + safeItem + "%'') ";
                }
                else if (items.Count > 1)
                {
                    string lotSep = "''" + "," + "''";
                    string joined = string.Join(lotSep, items.Select(i => i.Replace("'", "''")));
                    strWhere += " AND Name IN (''" + joined + "'') ";
                }
            }

            var sql = " EXEC [GetDiamondLotDropdown] @strWhere = '" + strWhere + "'";
            return await _sqlDataAccess.SelectDataDapper<BasicDataDto>(sql, companyId);
        }

        /// <summary>
        /// Matches MVC ReportProcessor.ParcelGetStockRegisterReport.
        /// Uses dynamic Dapper mapping (same as working Solitaire stock-history popup).
        /// </summary>
        public async Task<IEnumerable<ParcelStockRegisterReportDatatable>> GetParcelStockRegisterReportAsync(
            string Type,
            string stkIn,
            string stkOut,
            string WhereFilter,
            string FromDate,
            string ToDate,
            string companyId)
        {
            var sql =
                " EXEC [SP_StockRegisterReport] @Type = '" + Type +
                "', @StkIn = '" + stkIn +
                "', @StkOut = '" + stkOut +
                "', @WhereNew = '" + WhereFilter +
                "', @FromDate= '" + FromDate +
                "',@ToDate= '" + ToDate + "'";

            var rows = await _sqlDataAccess.SelectDataDapperDynamic(sql, companyId);
            return rows.Select(MapSpRow).ToList();
        }

        private static ParcelStockRegisterReportDatatable MapSpRow(dynamic row)
        {
            var dict = (IDictionary<string, object>)row;

            string GetString(params string[] keys)
            {
                foreach (var key in keys)
                {
                    foreach (var pair in dict)
                    {
                        if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (pair.Value == null || pair.Value == DBNull.Value)
                            continue;
                        return Convert.ToString(pair.Value, CultureInfo.InvariantCulture) ?? "";
                    }
                }
                return "";
            }

            decimal GetDecimal(params string[] keys)
            {
                foreach (var key in keys)
                {
                    foreach (var pair in dict)
                    {
                        if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (pair.Value == null || pair.Value == DBNull.Value)
                            continue;
                        return Convert.ToDecimal(pair.Value, CultureInfo.InvariantCulture);
                    }
                }
                return 0;
            }

            long GetLong(params string[] keys)
            {
                foreach (var key in keys)
                {
                    foreach (var pair in dict)
                    {
                        if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (pair.Value == null || pair.Value == DBNull.Value)
                            continue;
                        return Convert.ToInt64(pair.Value, CultureInfo.InvariantCulture);
                    }
                }
                return 0;
            }

            int GetInt(params string[] keys)
            {
                foreach (var key in keys)
                {
                    foreach (var pair in dict)
                    {
                        if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (pair.Value == null || pair.Value == DBNull.Value)
                            continue;
                        return Convert.ToInt32(pair.Value, CultureInfo.InvariantCulture);
                    }
                }
                return 0;
            }

            return new ParcelStockRegisterReportDatatable
            {
                trncode = GetLong("trncode", "TrnCode"),
                invcode = GetLong("invcode", "InvCode"),
                Itemcode = GetInt("Itemcode", "ItemCode"),
                StockID = GetString("StockID", "stockID", "LotNo", "lotNo"),
                TrnDate = GetString("TrnDate", "trndate"),
                TrnType = GetString("TrnType", "trntype"),
                Particulars = GetString("Particulars", "Particular", "PartyName", "AccName", "Perticulors"),
                StockIn = GetDecimal("StockIn", "InCts"),
                StockInPrice = GetDecimal("StockInPrice", "InRate"),
                StockInAmount = GetDecimal("StockInAmount", "InAmount"),
                StockOut = GetDecimal("StockOut", "OutCts"),
                StockOutPrice = GetDecimal("StockOutPrice", "OutRate"),
                StockOutAmount = GetDecimal("StockOutAmount", "OutAmount"),
                COGS = GetDecimal("COGS"),
                BalanceStock = GetDecimal("BalanceStock", "BalCts"),
                BalancePrice = GetDecimal("BalancePrice", "BalRate"),
                BalanceAmount = GetDecimal("BalanceAmount", "BalAmount")
            };
        }
    }
}
