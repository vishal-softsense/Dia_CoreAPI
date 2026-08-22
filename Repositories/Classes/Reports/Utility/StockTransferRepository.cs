using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    public class StockTransferRepository : IStockTransferRepository
    {
        private readonly ISqlDataAccess _db;

        public StockTransferRepository(ISqlDataAccess db)
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

        /// <summary>Matches MVC PurchaseSaleProcessor.GetStockTransferData — newest (highest Code) first.</summary>
        public async Task<List<StockTransferListDto>> GetStockTransferListAsync(string companyId)
        {
            var sql = @" EXEC [GetStockTransferDataUsingPageNo] @PageNo = '" + 1 + "' ";
            var result = await _db.SelectRawSql<StockTransferListDto>(sql, companyId);
            return result
                .OrderByDescending(x => x.Code)
                .ToList();
        }

        /// <summary>Matches MVC PurchaseSaleProcessor.GetLotNoDataForStockTransfer.</summary>
        public async Task<List<StockTranDataDto>> GetLotDetailsAsync(string lotNo, string companyId)
        {
            var where = " AND LotNo = ''" + (lotNo ?? string.Empty).Replace("'", "''") + "'' ";
            var sql = @" EXEC [GetLotNoDataForStockTransfer] @Where = '" + where + "'";
            var result = await _db.SelectRawSql<StockTranDataDto>(sql, companyId);
            return result.ToList();
        }

        /// <summary>Matches MVC PurchaseController.GetAllLotNoList → GetLotDataForDropdown.</summary>
        public async Task<IEnumerable<BasicDataDto>> GetAllLotNoListAsync(string searchby, string companyId)
        {
            if (string.IsNullOrWhiteSpace(searchby) || searchby.Trim().Length < 1)
                return Enumerable.Empty<BasicDataDto>();

            // Exact MVC pattern: EXEC with doubled quotes inside the @strWhere string literal
            var safe = searchby.Trim().Replace("'", "''");
            var strWhere = " AND (LotNo LIKE ''%" + safe + "%'' or Itemname LIKE ''%" + safe + "%'' ) ";
            var sql = @" EXEC [GetLotDataForDropdown] @strWhere = '" + strWhere + "'";
            var result = await _db.SelectRawSql<BasicDataDto>(sql, companyId);
            return result ?? Enumerable.Empty<BasicDataDto>();
        }

        /// <summary>Matches MVC StockTransferController.SaveStockTransfer with SqlTempAccess.</summary>
        public async Task<(bool Success, string Message)> SaveStockTransferAsync(
            StockTransferSaveDto model,
            int userId,
            string userName,
            int companyId,
            string companyIdStr)
        {
            var connectionString = await _db.GetClientConnectionString(companyIdStr);
            using var sql = new SqlTempAccess(connectionString);

            try
            {
                sql.StartTransaction();

                string fItemCode = sql.GetMaxItemCode(" AND LotNo = ''" + (model.Fromlotno ?? "").Replace("'", "''") + "''");
                int maxFItemCode = Convert.ToInt32(fItemCode);

                string tItemCode = sql.GetMaxItemCode(" AND LotNo = ''" + (model.Tolotno ?? "").Replace("'", "''") + "''");
                int maxTItemCode = Convert.ToInt32(tItemCode);

                sql.InsertSalePurTran(
                    model.TrnDate, "StkTrn", "", 0, 0, "",
                    float.Parse(model.Carats.ToString("N3")), 1,
                    Convert.ToDouble(model.ToPrice.ToString("N2")), 0, 0, 0, 0, 0, "", 0, 0, 0, "0", 0,
                    true, false, 0, maxFItemCode, maxTItemCode,
                    model.FromPrice, model.ToPrice, 0, 0, 0, "",
                    userId, 0, 0, 0,
                    Convert.ToDouble((0).ToString("N2")), userName, companyId);

                sql.CommitTransaction();
                return (true, "Stock Transfer Record Saved.");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                // MVC sets success=true with exception text on catch
                return (true, ex.ToString());
            }
        }

        /// <summary>Matches MVC StockTransferController.DeleteStockTransfer → DeleteStktrn.</summary>
        public async Task<(bool Success, string Message)> DeleteStockTransferAsync(int id, string companyIdStr)
        {
            var connectionString = await _db.GetClientConnectionString(companyIdStr);
            using var sql = new SqlTempAccess(connectionString);

            try
            {
                sql.StartTransaction();
                sql.DeleteStktrn(id);
                sql.CommitTransaction();
                return (true, "Record Deleted.");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return (false, ex.Message);
            }
        }
    }
}
