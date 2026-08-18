using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    public class ImportPricingRepository : IImportPricingRepository
    {
        private readonly ISqlDataAccess _db;

        public ImportPricingRepository(ISqlDataAccess db)
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

        public async Task<string> GetMaxItemCodeAsync(string whereClause, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Type", "ItemCode" },
                { "@Where", whereClause }
            };

            var result = await _db.SelectDataFromSPSingle<string>("GetALLCommonData", parameters, companyId);
            return result ?? "0";
        }

        /// <summary>Matches MVC ImportPricingController.SavePricing with SqlTempAccess transaction.</summary>
        public async Task<(bool Success, string Data)> SavePricingAsync(
            string pricingType,
            IList<ImportPricingRowDto> items,
            int userId,
            string companyId)
        {
            if (items == null || items.Count == 0)
                return (false, "No pricing rows to save.");

            var updateType = MapPricingType(pricingType);
            if (updateType == null)
                return (false, "Invalid pricing type selected.");

            var connectionString = await _db.GetClientConnectionString(companyId);
            using var sql = new SqlTempAccess(connectionString);

            try
            {
                sql.StartTransaction();

                foreach (var item in items)
                {
                    var lotNo = item.LotNo?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(lotNo))
                        continue;

                    var rate = ResolveRate(item);
                    var itemCode = sql.GetMaxItemCode(" AND LotNo = ''" + lotNo + "''");

                    if (string.IsNullOrWhiteSpace(itemCode) || itemCode == "0")
                        throw new Exception("Item not available for Lot No : " + lotNo);

                    var param = new DynamicParameters();
                    param.Add("@Updatetype", updateType);
                    param.Add("@Lotno", lotNo);
                    param.Add("@Value", rate);
                    param.Add("@User", userId);
                    sql.SaveDataInTransaction("SP_ImportPricing", param);
                }

                sql.CommitTransaction();
                return (true, "Rate/Back Updated");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return (false, ex.Message);
            }
        }

        private static double ResolveRate(ImportPricingRowDto item)
        {
            return item.rate;
        }

        private static string? MapPricingType(string? pricingType)
        {
            if (string.IsNullOrWhiteSpace(pricingType)) return null;
            var normalized = pricingType.Trim().ToLowerInvariant();
            return normalized switch
            {
                "back" => "back",
                "rate" => "rate",
                "seller back" => "seller back",
                "seller rate" => "seller rate",
                "b2b back" => "b2b back",
                "brb rate" or "b2b rate" => "b2b rate",
                _ => null
            };
        }
    }
}
