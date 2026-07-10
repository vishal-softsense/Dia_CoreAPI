using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;

namespace Dia_CoreAPI.Repositories.Classes.StockAccount
{
    public class SolitaireItemMasterRepository : ISolitaireItemMasterRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public SolitaireItemMasterRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        #region Solitaire

        public async Task<IEnumerable<ItemMaster>> GetItemMasterListAsync(string whereClause, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@PageNo", "1");
            param.Add("@where", whereClause);

            return await _sqlDataAccess.SelectDataFromSP<ItemMaster>("GetItemDataUsingPageNo", param, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> ItemMasterDropDownAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            return await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("GetAllMasterDataForItemMaster", param, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> FillCityStateCountryItemMasterDropDownAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            return await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("FillCityStateCountry", param, companyId);
        }

        public async Task<SolitaireItemMasterDto?> GetItemMasterFullListAsync(string id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@where", $" and ItemCode = {id} ");

            return await _sqlDataAccess.SelectDataFromSPSingle<SolitaireItemMasterDto>("GetItemDataForEdit", param, companyId);
        }

        public async Task<string> GetLotExistAsync(string whereClause, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "LotExist");
            param.Add("@Where", whereClause);

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAllCommonData", param, companyId);
            return result ?? "0";
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

        public async Task<bool> SaveItemMasterDataAsync(SolitaireItemMasterDto model, string actionType, string companyId, int singlePcs)
        {
            var param = new Dictionary<string, object>();
            param.Add("@ItemCode", model.ItemCode);
            param.Add("@ItemSrno", model.ItemCode == 0 ? await GetMaxItemSrNoAsync(companyId) + 1 : 0);
            param.Add("@LotNo", model.LotNo ?? "");
            param.Add("@ItemName", model.ItemName ?? "");
            param.Add("@ItemGroupId", model.ItemGroupID ?? "");
            param.Add("@Category", model.Category ?? "");
            param.Add("@ProductType", model.ProductType ?? "");
            param.Add("@Pcs", model.Pcs);
            param.Add("@Weight", 0);
            param.Add("@CaratCL", model.Weight);
            param.Add("@SieveId", model.SieveId ?? "");
            param.Add("@ShapeId", model.ShapeId ?? "");
            param.Add("@ColorId", model.ColorId ?? "");
            param.Add("@SizeId", model.SizeId ?? "");
            param.Add("@ClarityId", model.ClarityId ?? "");
            param.Add("@MRP", model.MRP);
            param.Add("@Back", model.Back);
            param.Add("@AskingRate", model.AskingRate);
            param.Add("@AskingAmount", model.AskingAmount);
            param.Add("@BoxName", model.BoxName ?? "");
            param.Add("@BoxRate", model.BoxRate);
            param.Add("@CertId", model.LAB ?? "");
            param.Add("@CertificateNo", model.CertificateNo ?? "");
            param.Add("@HAID", model.HAID ?? "");
            param.Add("@PropId", model.PropId ?? "");
            param.Add("@PolishId", model.PolishId ?? "");
            param.Add("@SymId", model.SymId ?? "");
            param.Add("@FLId", model.FLId ?? "");
            param.Add("@FLColor", model.FlColor ?? "");
            param.Add("@FancyColor", model.FancyColor ?? "");
            param.Add("@FancyColorIntensity", model.FancyColorIntensity ?? "");
            param.Add("@FancyColorOvertone", model.FancyColorOvertone ?? "");
            param.Add("@L", model.Length);
            param.Add("@W", model.Width);
            param.Add("@D", model.Depth);
            param.Add("@Diameter", model.Diameter ?? "");
            param.Add("@Measurement", model.Measurement ?? "");
            param.Add("@LWRatio", model.LWRatio ?? "");
            param.Add("@TotDepth", model.TotalDepthPer);
            param.Add("@TablePer", model.TablePer);
            param.Add("@CrownHeight", model.CrownHeight);
            param.Add("@CrownAngle", model.CrownAngle);
            param.Add("@PavilionDepth", model.PavilionDepth);
            param.Add("@PavilionHeight", model.PavilionHeight);
            param.Add("@PavilionPer", model.PavilionPer);
            param.Add("@GirdlePer", model.GirdlePer);
            param.Add("@GirdleCondition", model.GirdleCondition ?? "");
            param.Add("@GirdleMin", model.GirdleMinPer);
            param.Add("@GirdleMax", model.GirdleMaxPer);
            param.Add("@GirdleMinMS", model.GirdleMinMM);
            param.Add("@GirdleMaxMS", model.GirdleMaxMM);
            param.Add("@CuletId", model.CuletId ?? "");
            param.Add("@CuletCondition", model.CuletCondition ?? "");
            param.Add("@StrLn", model.StrLn);
            param.Add("@LrHalf", model.LrHalf);
            param.Add("@LaserInscription", model.LaserInscribed);
            param.Add("@LaserInscriptionText", model.LaserInscriptionText ?? "");
            param.Add("@Instruction", model.Instruction ?? "");
            param.Add("@KeyToSymbols", model.KeyToSymbols ?? "");
            param.Add("@LABComments", model.LABComments ?? "");
            param.Add("@MemberComments", model.MemberComments ?? "");
            param.Add("@BrownId", model.BrownId ?? "");
            param.Add("@GreenId", model.GreenId ?? "");
            param.Add("@MilkyId", model.MilkyId ?? "");
            param.Add("@LusterId", model.LusterId ?? "");
            param.Add("@ButhId", model.ButhId ?? "");
            param.Add("@NattsId", model.NattsId ?? "");
            param.Add("@EyeClean", model.EyeClean ?? "");
            param.Add("@ShadeId", model.ShadeId ?? "");
            param.Add("@TableInclusion", model.TableInclusion ?? "");
            param.Add("@CrownInclusion", model.CrownInclusion ?? "");
            param.Add("@OIT", model.OIT ?? "");
            param.Add("@OIC", model.OIC ?? "");
            param.Add("@OIP", model.OIP ?? "");
            param.Add("@OIG", model.OIG ?? "");
            param.Add("@PolishFeatures", model.PolishFeatures ?? "");
            param.Add("@SymFeatures", model.SymmetryFeatures ?? "");
            param.Add("@ColorDescription", model.ColorDescription ?? "");
            param.Add("@ClarityDescription", model.ClarityDescription ?? "");
            param.Add("@ClarityStatus", model.ClarityStatus ?? "");
            param.Add("@Graining", model.Graining ?? "");
            param.Add("@Painting", model.Painting ?? "");
            param.Add("@IdealImagePath", model.IdealImagePath ?? "");
            param.Add("@AssetImagePath", model.AssetImagePath ?? "");
            param.Add("@HeartsImagePath", model.HeartsImagePath ?? "");
            param.Add("@ArrowsImagePath", model.ArrowsImagePath ?? "");
            param.Add("@ImagePath", model.ImagePath ?? "");
            param.Add("@VideoPath", model.VideoPath ?? "");
            param.Add("@B2BDNAPagePath", model.B2BDNAPage ?? "");
            param.Add("@B2CDNAPagePath", model.B2CDNAPage ?? "");
            param.Add("@CertificatePath", model.CertificatePath ?? "");
            param.Add("@PlottingImagePath", model.PlottingImagePath ?? "");
            param.Add("@PurchaseDate", model.PurchaseDate == default ? DateTime.Now : model.PurchaseDate);
            param.Add("@RateReviseDate", model.RateReviseDate == default ? DateTime.Now : model.RateReviseDate);
            param.Add("@NewArrivalDate", model.NewArrivalDate == default ? DateTime.Now : model.NewArrivalDate);
            param.Add("@StoneLastUnHoldDate", model.StoneLastUnholdDate == default ? DateTime.Now : model.StoneLastUnholdDate);
            param.Add("@PublicRemarks", model.PublicCommnets ?? "");
            param.Add("@PrivateRemarks", model.PrivateCommnets ?? "");
            param.Add("@AdminRemarks", model.AdminCommnets ?? "");
            param.Add("@PurchaseRate", model.PurchaseRate);
            param.Add("@PurchaseBack", model.PurchaseBack);
            param.Add("@SinglePcs", singlePcs);
            param.Add("@PurchaseAmount", model.PurchaseAmount);
            param.Add("@ActualRate", model.ActualRate);
            param.Add("@ActualBack", model.ActualBack);
            param.Add("@ActualAmount", model.ActualAmount);
            param.Add("@BottomBack", model.BottomBack);
            param.Add("@BottomRate", model.BottomRate);
            param.Add("@B2BRate", model.B2BRate);
            param.Add("@B2BBack", model.B2BBack);
            param.Add("@B2CRate", model.B2CRate);
            param.Add("@B2CBack", model.B2CBack);
            param.Add("@MfgSeriesID", model.MfgSeriesID ?? "");
            param.Add("@MfgPacketID", model.MfgPacketID ?? "");
            param.Add("@RoughId", model.RoughID ?? "");
            param.Add("@MinesId", model.MinesID ?? "");
            
            param.Add("@CompanyID", int.Parse(companyId));
            param.Add("@ActionType", actionType);

            int result = await _sqlDataAccess.ExecuteSP("SP_ItemMaster", param, companyId);
            return result > 0 || true;
        }

        public async Task<string> GetItemCountAsync(string where1, string where2, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@ActionType", "DeleteItem");
            param.Add("@Where", where1);
            param.Add("@Where2", where2);

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAccandItemCount", param, companyId);
            return result ?? "0";
        }

        public async Task<bool> DeleteItemMasterDataAsync(int id, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>();
            param.Add("@ItemCode", id);
            param.Add("@LotNo", "");
            param.Add("@ActionType", actionType);

            int result = await _sqlDataAccess.ExecuteSP("SP_ItemMaster", param, companyId);
            return result > 0 || true;
        }

        #endregion
    }
}
