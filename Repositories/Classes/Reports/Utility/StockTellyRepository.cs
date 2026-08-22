using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.Reports.Utility;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    public class StockTellyRepository : IStockTellyRepository
    {
        private readonly ISqlDataAccess _db;

        public StockTellyRepository(ISqlDataAccess db)
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

        /// <summary>Matches MVC AdminStockSearchDropDown → AllMasterDataForAdminStockSearch</summary>
        public async Task<List<BasicDataDto>> GetStockSearchDropdownAsync(string companyId)
        {
            var result = await _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForAdminStockSearch",
                new Dictionary<string, object>(),
                companyId);

            return result.ToList();
        }

        /// <summary>Matches MVC ReportProcessor.GetParcelStockSearchData → ParcelGetStockReportData</summary>
        public async Task<List<ParcelStockrpt>> GetParcelStockSearchDataAsync(
            string date,
            string where,
            string askingQry,
            string companyId)
        {
            // MVC builds raw EXEC with embedded quoted params — same pattern as ReportProcessor.GetParcelStockSearchData
            var sql =
                "EXEC [ParcelGetStockReportData] @Date = '''" + date.Replace("'", "''") +
                "''',@AskingQry = '" + askingQry.Replace("'", "''") +
                "',@Where = '" + where.Replace("'", "''") + "'";

            var result = await _db.SelectRawSql<ParcelStockrpt>(sql, companyId);
            return result.ToList();
        }

        /// <summary>Matches MVC SqlTempAccess.GetLotNo inside import transaction.</summary>
        public async Task<string?> GetLotNoAsync(string whereClause, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Type", "LotNo" },
                { "@Where", whereClause }
            };

            return await _db.SelectDataFromSPSingle<string>("GetALLCommonData", parameters, companyId);
        }

        /// <summary>Matches MVC StockTellyController.SaveStockTelly with SqlTempAccess transaction.</summary>
        public async Task<(bool Success, string Message)> SaveStockTellyAsync(
            StockTellySaveRequestDto request,
            int userId,
            string userName,
            int companyId,
            string companyIdStr)
        {
            if (request?.spList == null || request.spList1 == null)
                return (false, "Invalid data received.");

            var connectionString = await _db.GetClientConnectionString(companyIdStr);
            using var sql = new SqlTempAccess(connectionString);

            try
            {
                sql.StartTransaction();

                foreach (var from in request.spList)
                {
                    int maxFItemCode = from.ItemCode;
                    int maxTItemCode = Convert.ToInt32(sql.GetLOTNO("AND LotNo=''TempMixingLot''"));

                    if (from.Carats > 0)
                    {
                        sql.InsertSalePurTran(
                            from.TrnDate, "StkTrn", "", 0, 0, "",
                            float.Parse(from.Carats.ToString("N3")), from.Pcs,
                            Convert.ToDouble(from.Rate.ToString("N2")), 0, 0, 0, 0,
                            Convert.ToDouble(from.Amount.ToString("N2")), "", 0, 0, 0, "0", 0,
                            true, false, 0, maxFItemCode, maxTItemCode,
                            from.FromPrice, from.ToPrice, 0, 0, 0, "",
                            userId, 0, 0, 0, 0, userName, companyId);
                    }
                    else if (from.Carats < 0)
                    {
                        sql.InsertSalePurTran(
                            from.TrnDate, "StkTrn", "", 0, 0, "",
                            float.Parse(from.Carats.ToString("N3")) * -1, from.Pcs,
                            Convert.ToDouble(from.Rate.ToString("N2")), 0, 0, 0, 0,
                            Convert.ToDouble(from.Amount.ToString("N2")), "", 0, 0, 0, "0", 0,
                            true, false, 0, maxTItemCode, maxFItemCode,
                            from.FromPrice, from.ToPrice, 0, 0, 0, "",
                            userId, 0, 0, 0, 0, userName, companyId);
                    }
                }

                foreach (var to in request.spList1)
                {
                    int maxFItemCode = Convert.ToInt32(sql.GetLOTNO("AND LotNo=''TempMixingLot''"));
                    int maxTItemCode = to.ItemCode;

                    if (to.Carats > 0)
                    {
                        sql.InsertSalePurTran(
                            to.TrnDate, "StkTrn", "", 0, 0, "",
                            float.Parse(to.Carats.ToString("N3")), to.Pcs,
                            Convert.ToDouble(to.Rate.ToString("N2")), 0, 0, 0, 0,
                            Convert.ToDouble(to.Amount.ToString("N2")), "", 0, 0, 0, "0", 0,
                            true, false, 0, maxFItemCode, maxTItemCode,
                            to.FromPrice, to.ToPrice, 0, 0, 0, "",
                            userId, 0, 0, 0, 0, userName, companyId);
                    }
                }

                sql.CommitTransaction();
                return (true, "Stock transactions saved successfully.");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return (false, "Error: " + ex.Message);
            }
        }
    }
}
