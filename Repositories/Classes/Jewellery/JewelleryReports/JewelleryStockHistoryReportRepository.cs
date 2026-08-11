using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Repositories.Classes.Jewellery.JewelleryReports
{
    public class JewelleryStockHistoryReportRepository : IJewelleryStockHistoryReportRepository
    {
        private readonly ISqlDataAccess _db;

        public JewelleryStockHistoryReportRepository(ISqlDataAccess db)
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

            var result = await _db.SelectDataFromSP<Dia_CoreAPI.DTOs.Masters.PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }

        public async Task<IEnumerable<BasicDataDto>> GetJewStockIdListAsync(string searchby, string companyId)
        {
            string strWhere = "";
            if (!string.IsNullOrEmpty(searchby))
            {
                searchby = searchby.Replace("\r\n", ",").Replace("\n", ",");
                strWhere += " AND Name IN (''" + searchby.Replace(",", "'',''") + "'') ";
            }

            string sql = $@" EXEC [GetJewStockDropdown] @strWhere = '{strWhere}' ";
            return await _db.SelectRawSql<BasicDataDto>(sql, companyId);
        }

        public Task<IEnumerable<BasicDataDto>> GetPurchaseReportDropDownAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<IEnumerable<JewelleryStockHistoryReportDatatableDto>> GetJewStockRegisterReportAsync(
            string Type,
            string stkIn,
            string stkOut,
            string WhereFilter,
            string FromDate,
            string ToDate,
            string companyId)
        {
            var sql = $"EXEC [SP_JewStockRegisterReport] @Type = '{Type}', @StkIn = '{stkIn}', @StkOut = '{stkOut}', @WhereNew = '{WhereFilter}', @FromDate= '{FromDate}', @ToDate= '{ToDate}'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapSpRow).ToList();
        }

        private static JewelleryStockHistoryReportDatatableDto MapSpRow(dynamic row)
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

            return new JewelleryStockHistoryReportDatatableDto
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
