using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;

namespace Dia_CoreAPI.Repositories.Implementations.StockAccount
{
    public class ParcelItemMasterRepository : IParcelItemMasterRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ParcelItemMasterRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<IEnumerable<PItemMasterDto>> GetParcelItemMasterListAsync(string companyId, string whereClause = "")
        {
            var param = new Dictionary<string, object>();
            param.Add("@PageNo", "1");
            param.Add("@where", whereClause);

            return await _sqlDataAccess.SelectDataFromSP<PItemMasterDto>("GetItemDataUsingPageNo", param, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetItemMasterDropdownAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            return await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("GetAllMasterDataForItemMaster", param, companyId);
        }

        public async Task<ParcelItemMasterDto?> GetParcelItemMasterByIdAsync(int id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@where", $" and ItemCode = {id} ");

            return await _sqlDataAccess.SelectDataFromSPSingle<ParcelItemMasterDto>("GetItemDataForEdit", param, companyId);
        }

        public async Task<bool> CheckLotNoExistsAsync(string lotNo, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "LotExist");
            param.Add("@Where", $" AND LotNo = '{lotNo}'");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAllCommonData", param, companyId);
            return result != "0" && !string.IsNullOrEmpty(result);
        }

        public async Task<int> GetMaxItemSrNoAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "MaxItemSrNo");
            param.Add("@Where", "");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAllCommonData", param, companyId);
            if (int.TryParse(result, out int maxSrNo))
            {
                return maxSrNo;
            }
            return 0;
        }

        public async Task<bool> SaveParcelItemMasterAsync(ParcelItemMasterDto model, string actionType, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@ItemCode", model.ItemCode);
            param.Add("@LotNo", model.LotNo ?? "");
            param.Add("@ItemSrNo", model.SrNo != null && int.TryParse(model.SrNo, out var srNo) ? srNo : 0);
            param.Add("@ShapeId", model.ShapeId ?? "");
            param.Add("@ColorId", model.ColorId ?? "");
            param.Add("@SizeId", model.SizeId ?? "");
            param.Add("@ClarityId", model.ClarityId ?? "");
            param.Add("@SieveId", model.SieveId ?? "");
            param.Add("@ItemName", model.ItemName ?? "");
            param.Add("@ItemGroupId", model.ItemGroupID ?? "");
            param.Add("@Category", model.Category ?? "");
            param.Add("@Instruction", model.Instruction ?? ""); // StockSubType
            param.Add("@ProductType", model.ProductType ?? ""); // Grown/Process Type
            param.Add("@Weight", model.Weight);
            param.Add("@Pcs", model.Pcs);
            
            param.Add("@PDollorRate", model.PDollorRate);
            param.Add("@PurchaseRate", model.PurchaseRate);
            param.Add("@PurchaseAmount", model.PurchaseAmount);
            param.Add("@MasterCurrencyRate", model.MasterCurrencyRate);
            param.Add("@MasterCurrencyAmount", model.MasterCurrencyAmount);
            param.Add("@AskingRate", model.AskingRate);
            param.Add("@AskingAmount", model.AskingAmount);
            param.Add("@PublicRemarks", model.PublicCommnets ?? "");
            param.Add("@CompanyID", int.Parse(companyId));
            param.Add("@ActionType", actionType);

            int result = await _sqlDataAccess.ExecuteSP("SP_ItemMaster", param, companyId);
            return result > 0 || true;
        }

        public async Task<string> GetAccandItemCountAsync(int id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@ActionType", "DeleteItem");
            param.Add("@Where", $" And ItemCode= {id}");
            param.Add("@Where2", "");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAccandItemCount", param, companyId);
            return result ?? "0";
        }

        public async Task<bool> DeleteParcelItemMasterAsync(int id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@ItemCode", id);
            param.Add("@LotNo", "");
            param.Add("@ActionType", "delete");

            int result = await _sqlDataAccess.ExecuteSP("SP_ItemMaster", param, companyId);
            return result > 0 || true;
        }
    }
}
