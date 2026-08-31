using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Microsoft.AspNetCore.Connections;
using System.Data;

namespace Dia_CoreAPI.Repositories.Classes.StockAccount
{
    public class JewStockMasterRepository : IJewStockMasterRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

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

            var result =
                await _sqlDataAccess.SelectDataFromSP<PermissionCheck>("GetUserPermission", parameters, companyId);
            return result.Count();
        }

        public JewStockMasterRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }
        public async Task<IEnumerable<JewStockMasterDto>> GetMountingStockAsync(string companyId, string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Where", whereClause);
            param.Add("@Type", "JewStock");

            return await _sqlDataAccess.SelectDataFromSP<JewStockMasterDto>(
                "GetALLMasterList",
                param,
                companyId);
        }
        public async Task<IEnumerable<JewMetalMasterDto>> GetJewMetalMasterAsync(string companyId, string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Where", "");
            param.Add("@Type", "JewMetal");

            var result = await _sqlDataAccess.SelectDataFromSP<JewMetalMasterDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<IEnumerable<JewColorMasterDto>> GetJewColorMasterAsync(string companyId, string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Where", "");
            param.Add("@Type", "JewColor");

            var result = await _sqlDataAccess.SelectDataFromSP<JewColorMasterDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<IEnumerable<GetJewPurityDto>> GetJewPurityAsync(string companyId, string metalType)
        {
            var param = new Dictionary<string, object>();

            //param.Add("@Where", $"and Metal=''{metalType}''");
            param.Add("@Where", $"and Metal='{metalType}'");
            param.Add("@Type", "GetJewPurity");
            var result = await _sqlDataAccess.SelectDataFromSP<GetJewPurityDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<double> GetPureRatioAsync(string companyId, string metalType, string metalPurity)
        {
            var param = new Dictionary<string, object>();

            param.Add("@Type", "GetMetalPurity");
            param.Add("@Where",
                $"and Metal='{metalType}' and MetalPurityID='{metalPurity}'");

            return await _sqlDataAccess.GetScalarDataFromSP<double>(
                "GetALLMasterList",
                param,
                companyId);
        }
        public async Task<IEnumerable<BasicDataDto>> GetItemMasterDropdownAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            return await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("GetAllMasterDataForItemMaster", param, companyId);
        }
        public async Task<List<JewCategoryDto>> GetJewCategoryMasterAsync(string companyId,string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Type", "JewCategory");
            param.Add("@Where", whereClause);

            var result = await _sqlDataAccess.SelectDataFromSP<JewCategoryDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<List<JewSubCategoryDto>> GetJewSubCategoryMasterAsync(string companyId,string whereClause = "")
        {
            var param = new Dictionary<string, object>();

            param.Add("@Type", "JewSubCategory");
            param.Add("@Where", whereClause);

            var result = await _sqlDataAccess.SelectDataFromSP<JewSubCategoryDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<List<SettingMasterDto>> GetJewSettingValueAsync(string companyId)
        {
            var param = new Dictionary<string, object>();

            param.Add("@Type", "Setting");
            param.Add("@Where", "");

            var result = await _sqlDataAccess.SelectDataFromSP<SettingMasterDto>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        public async Task<AddMountingStockDto> GetAddMountingStockDataAsync(string companyId)
        {
            var itemMaster = (await GetItemMasterDropdownAsync(companyId)).ToList();

            var category = (await GetJewCategoryMasterAsync(companyId)).ToList();

            var subCategory = (await GetJewSubCategoryMasterAsync(companyId)).ToList();

            var metal = (await GetJewMetalMasterAsync(companyId)).ToList();

            var color = (await GetJewColorMasterAsync(companyId)).ToList();

            var setting = (await GetJewSettingValueAsync(companyId)).ToList();

            return new AddMountingStockDto
            {
                Shape = itemMaster.Where(x => x.Type == "Shape").ToList(),
                Color = itemMaster.Where(x => x.Type == "Color").ToList(),
                Size = itemMaster.Where(x => x.Type == "Size").ToList(),
                Clarity = itemMaster.Where(x => x.Type == "Clarity").ToList(),
                Sieve = itemMaster.Where(x => x.Type == "Sieve").ToList(),
                Category = itemMaster.Where(x => x.Type == "Category").ToList(),

                JewCategories = category,

                JewSubCategories = subCategory,

                JewMetalType = metal,

                JewColor = color,

                JewSettingValue = setting
                    .Where(x => x.Name == "Jew Setting Type")
                    .Select(x => x.Value)
                    .Distinct()
                    .ToList(),

                DecimalinInvoiceAmount = setting
                    .Where(x => x.Name == "JewelleryMetalAmountCalculateOnGoldWeight")
                    .Select(x => x.Value)
                    .FirstOrDefault(),

                OtherPer = setting
                    .Where(x => x.Name == "Addperinother")
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? "",

                AskingPer = setting
                    .Where(x => x.Name == "Addperinasking")
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? ""
            };
        }

        // Mounting-specific repository methods
        public async Task<JewStockMasterDto?> GetMountingStockByIdAsync(int id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Where", $" AND JewCode = {id} ");
            param.Add("@Type", "JewStock");

            return await _sqlDataAccess.SelectDataFromSPSingle<JewStockMasterDto>("GetALLMasterList", param, companyId);
        }

        public async Task<bool> CheckStockIdExistsAsync(string stockId, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Where", $" AND Stock_ID = '{stockId}' ");
            param.Add("@Type", "JewStock");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<JewStockMasterDto>("GetALLMasterList", param, companyId);
            return result != null;
        }

        public async Task<bool> CheckMountingUsedAsync(string mountingId, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "JewStock");
            param.Add("@Where", $" AND UseMounting = '{mountingId}' ");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<JewStockMasterDto>("GetALLMasterList", param, companyId);
            return result != null;
        }

        public async Task<bool> SaveMountingStockAsync(JewStockMasterDto model, List<DiamondDetails> diamond, List<SettingDetails> setting, string actionType, string companyId)
        {
            var param = new Dictionary<string, object>();

            // Stock ID generation logic
            string stockId;
            int serialNo = 0;
            string getDesignName = "";

            bool isDesignSelected = !string.IsNullOrWhiteSpace(model.DesignID);
            bool isManualStock = !string.IsNullOrWhiteSpace(model.Stock_ID);

            if (isDesignSelected)
            {
                getDesignName = await GetDesignNameAsync($" AND ID='{model.DesignID}'", companyId);
                int maxSerialNo = await GetMaxStockSerialNoAsync($" AND DesignName='{getDesignName}' AND CompanyID={companyId}", companyId);
                
                if (actionType == "insert")
                {
                    serialNo = maxSerialNo + 1;
                    stockId = getDesignName + serialNo.ToString("D3");
                }
                else
                {
                    // For update, use existing serialNo
                    serialNo = maxSerialNo;
                    stockId = model.Stock_ID?.Trim();
                }
            }
            else
            {
                stockId = model.Stock_ID?.Trim();
                serialNo = 0;
            }

            // Save JewStockMasterData parameters
            param.Add("@id", model.JewCode);
            param.Add("@Trndate", model.TrnDate ?? "");
            param.Add("@StockID", stockId);
            param.Add("@StockSerialNo", serialNo);
            param.Add("@DesignId", model.DesignID ?? "");
            param.Add("@DesignName", getDesignName);
            param.Add("@DesignPriority", model.DesignPriority);
            param.Add("@CreatedBy", model.CreatedBy ?? "");
            param.Add("@ModifyBy", model.ModifyBy ?? "");
            param.Add("@CompanyID", int.Parse(companyId));
            param.Add("@Category", model.Category ?? "");
            param.Add("@SubCategory", model.SubCategory ?? "");
            param.Add("@JewSize", model.JewSize ?? "");
            param.Add("@JewCerNo", model.JewelleryCerNo ?? "");
            param.Add("@MetalType", model.MetalType ?? "");
            param.Add("@Color", model.Color ?? "");
            param.Add("@Purity", model.Purity ?? "");
            param.Add("@PureRatio", model.PureRatio);
            param.Add("@GrossWeight", model.GrossWeight);
            param.Add("@NetWeight", model.NetWeight);
            param.Add("@MetalPriceGm", model.MetalPriceGm);
            param.Add("@MakingType", model.MakingType ?? "");
            param.Add("@MakingRate", model.MakingRate);
            param.Add("@MakingCharges", model.MakingCharges);
            param.Add("@TotalSettingCharges", model.SettingCharges);
            param.Add("@HandlingChargesPerCarat", model.HandlingChargesPerCarat);
            param.Add("@HandlingCarat", model.HandlingCarat);
            param.Add("@TotalHandlingCharges", model.TotalHandlingCharges);
            param.Add("@JewCertificate", model.JewelleryCertificate ?? "");
            param.Add("@CertificateCharges", model.TotalCertificateCharges);
            param.Add("@OtherCharges", model.OtherCharges);
            param.Add("@HallmarkCharges", model.HallmarkCharges);
            param.Add("@Totmetalamount", model.TotalMetalAmount);
            param.Add("@TotDiamondamt", model.TotalDiamondAmount);
            param.Add("@TotDiamondCarat", model.TotalDiamondCarat);
            param.Add("@TotStoneCarat", model.TotalStoneCarat);
            param.Add("@TotStoneAmount", model.TotalStoneAmount);
            param.Add("@TotMakingCharges", model.TotalMakingCharges);
            param.Add("@ImagePath", model.ImagePath ?? "");
            param.Add("@ImageBinary", model.ImageBinary ?? new byte[0]);
            param.Add("@VideoPath", model.VideoPath ?? "");
            param.Add("@CertPath", model.CertPath ?? "");
            param.Add("@finalamount", Math.Round(model.FinalAmount, 2));
            param.Add("@AskingRate", model.AskingRate);
            param.Add("@Pcs", model.Pcs);
            param.Add("@ActualRate", 0f);
            param.Add("@PurchaseRate", 0f);
            param.Add("@BottomRate", 0f);
            param.Add("@B2BRate", 0f);
            param.Add("@B2CRate", 0f);
            param.Add("@TotalDiamondQty", model.TotalDiamondQty);
            param.Add("@TotalStoneQty", model.TotalStoneQty);
            param.Add("@Description", model.Description ?? "");
            param.Add("@MountCategory", model.MountCategory ?? "");
            param.Add("@UseMounting", model.UseMounting ?? "");
            param.Add("@costamount", model.costamount ?? "");
            param.Add("@ActionType", actionType);

            int result = await _sqlDataAccess.ExecuteSP("SP_JewStockMaster", param, companyId);

            if (result <= 0) return false;

            // Get MaxJewCode
            int maxJewCode = actionType == "insert" ? await GetMaxStockCodeAsync(companyId) : model.JewCode;

            // For update: delete existing diamond and setting details first (matching MVC FINAL logic)
            if (actionType == "update")
            {
                await DeleteBothAsync(maxJewCode, "deletestockdiamondandsetting", companyId);
            }

            // Save diamond details
            if (diamond != null)
            {
                foreach (var dm in diamond)
                {
                    string itemCode = await GetMaxItemCodeAsync($" AND LotNo = '{dm.StockID}'", companyId);
                    int maxItemCode = int.TryParse(itemCode, out int code) ? code : 0;
                    await SaveStockDiamondDetailsDataAsync(model.DesignID ?? "", maxJewCode, maxItemCode, dm.StockID ?? "", dm.Type ?? "", dm.Shape ?? "", dm.Size ?? "", dm.Sieve ?? "", dm.Color ?? "", dm.Clarity ?? "", dm.Pcs, Math.Round(dm.Carat, 2), dm.Price, Math.Round(dm.Amount, 2), dm.Lab ?? "", "InsertStockDiamond", companyId);
                }
            }

            // Save setting details
            if (setting != null)
            {
                foreach (var st in setting)
                {
                    await SaveStockSettingDetailsDataAsync(model.DesignID ?? "", maxJewCode, st.Type, st.Shape, st.Size, st.Pcs, st.SettingType, st.Price, st.Amount, "InsertStockSetting", companyId);
                }
            }

            return true;
        }

        public async Task<bool> DeleteMountingStockMasterAsync(int id, string companyId)
        {
            // First check if record exists (matching MVC FINAL logic)
            var existingData = await GetMountingStockByIdAsync(id, companyId);
            if (existingData == null)
            {
                return false; // Record not found
            }

            var param = new Dictionary<string, object>();
            param.Add("@ID", id);
            param.Add("@ActionType", "deleteallstock");

            int result = await _sqlDataAccess.ExecuteSP("SP_DeleteJewDesign", param, companyId);
            return result > 0;
        }

        public async Task<string> GetNextMountingStockIdAsync(string designId, string companyId)
        {
            return await GetDesignNameAsync($" AND JewCode='{designId}'", companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetMountingStockDropdownAsync(string category, string actionType, string companyId)
        {
            string strWhere = string.Empty;

            if (category == "Finished Jewellery" || category == "Semi Jewellery")
            {
                if (actionType == "edit")
                {
                    strWhere = " AND (MountCategory = 'Mounting' OR MountCategory = 'Metal')";
                }
                else
                {
                    strWhere = " AND MountCategory IN ('Mounting','Metal')";
                }
            }

            var param = new Dictionary<string, object>();
            param.Add("@strWhere", strWhere);

            var allMountings = await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("GetlotandmetalDataForDropdown", param, companyId);

            return allMountings;
        }

        public async Task<IEnumerable<BasicDataDto>> GetAllLotNoListAllAsync(string searchby, string stoneType, string companyId)
        {
            if (string.IsNullOrWhiteSpace(searchby))
            {
                return Enumerable.Empty<BasicDataDto>();
            }

            var safeSearch = searchby.Replace("'", "''");
            string strWhere = "";

            if (stoneType == "Mounting")
            {
                strWhere += $" AND (Name LIKE '%{safeSearch}%' OR TextName LIKE '%{safeSearch}%') ";
                strWhere += " AND Type = 'MountingJew' ";
            }
            else if (stoneType == "Natural Diamond")
            {
                strWhere += $" AND (Name LIKE '%{safeSearch}%' OR TextName LIKE '%{safeSearch}%') ";
                strWhere += " AND Type = 'Item' ";
                strWhere += " AND Category = 'Natural Diamond' ";
            }
            else if (stoneType == "Lab Grown Diamond")
            {
                strWhere += $" AND (Name LIKE '%{safeSearch}%' OR TextName LIKE '%{safeSearch}%') ";
                strWhere += " AND Type = 'Item' ";
                strWhere += " AND Category = 'Lab Grown Diamond' ";
            }
            else
            {
                strWhere += $" AND (Name LIKE '%{safeSearch}%' OR TextName LIKE '%{safeSearch}%') ";
            }

            var results = new List<BasicDataDto>();

            try
            {
                var param = new Dictionary<string, object> { { "@strWhere", strWhere } };
                var spResults = await _sqlDataAccess.SelectDataFromSP<BasicDataDto>(
                    "GetlotandmetalDataForDropdown", param, companyId);
                results.AddRange(spResults);
            }
            catch
            {
                // Fall through to item-master search below.
            }

            if (stoneType != "Mounting")
            {
                var parcelLots = await GetItemMasterLotsAsync(safeSearch, stoneType, companyId, "0");
                var solitaireLots = await GetItemMasterLotsAsync(safeSearch, stoneType, companyId, "1");
                results.AddRange(parcelLots);
                results.AddRange(solitaireLots);
            }

            return results
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.ID))
                .GroupBy(x => (x.Name ?? x.ID).Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var item = g.First();
                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        item.Name = item.ID;
                    }
                    if (string.IsNullOrWhiteSpace(item.TextName))
                    {
                        item.TextName = item.Name;
                    }
                    return item;
                })
                .ToList();
        }

        private async Task<IEnumerable<BasicDataDto>> GetItemMasterLotsAsync(
            string safeSearch, string stoneType, string companyId, string singlePcs)
        {
            string whereClause =
                $"And SinglePcs= '{singlePcs}' AND (LotNo LIKE '%{safeSearch}%' OR ItemName LIKE '%{safeSearch}%')";

            if (stoneType == "Natural Diamond")
            {
                whereClause += " AND Category = 'Natural Diamond'";
            }
            else if (stoneType == "Lab Grown Diamond")
            {
                whereClause += " AND Category = 'Lab Grown Diamond'";
            }
            else if (stoneType == "Gem Stone")
            {
                whereClause += " AND Category = 'Gem Stone'";
            }

            var param = new Dictionary<string, object>
            {
                { "@PageNo", "1" },
                { "@where", whereClause }
            };

            var items = await _sqlDataAccess.SelectDataFromSP<PItemMasterDto>(
                "GetItemDataUsingPageNo", param, companyId);

            return items
                .Where(x => !string.IsNullOrWhiteSpace(x.LotNo))
                .Select(x => new BasicDataDto
                {
                    ID = x.LotNo!,
                    Name = x.LotNo!,
                    TextName = string.IsNullOrWhiteSpace(x.ItemName) ? x.LotNo! : x.ItemName!,
                    Type = "Item"
                });
        }

        public async Task<(JewStockMasterDto? data, List<DiamondDetails> diamondList)> GetAllMountingDetailsAsync(int id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Where", $" AND JewCode = {id} ");
            param.Add("@Type", "JewStock");

            var data = await _sqlDataAccess.SelectDataFromSPSingle<JewStockMasterDto>("GetALLMasterList", param, companyId);

            // Get diamond details
            var diamondParam = new Dictionary<string, object>();
            diamondParam.Add("@Where", $" AND JewCode = {id} ");
            diamondParam.Add("@Type", "GetJewStockDiamond");

            // Match MVC GetAllmountDetails — only JewStock diamond rows (not setting rows)
            var dbDiamondData = await _sqlDataAccess.SelectDataFromSP<GetJewDiamond>("GetALLMasterList", diamondParam, companyId);
            var diamondList = dbDiamondData.Select(row => new DiamondDetails
            {
                StockID = row.DiamondLotNo,
                Type = row.DiamondType,
                Shape = row.DiamondShape,
                Size = row.DiamondSize,
                Sieve = row.DiamondSieve,
                Color = row.DiamondColor,
                Clarity = row.DiamondClarity,
                Pcs = row.DiamondPcs,
                Carat = row.DiamondCarat,
                Price = row.DiamondPrice,
                Amount = row.DiamondAmount,
                Lab = row.DiamondLab
            }).ToList();

            return (data, diamondList);
        }

        public async Task<int> GetMaxStockCodeAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "StockCode");
            param.Add("@Where", "");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetALLCommonData", param, companyId);
            return int.TryParse(result, out int code) ? code : 0;
        }

        public async Task<string> GetMaxItemCodeAsync(string whereClause, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "ItemCode");
            param.Add("@Where", whereClause);

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetALLCommonData", param, companyId);
            return result ?? "0";
        }

        public async Task<string> GetDesignNameAsync(string whereClause, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Where", whereClause);
            param.Add("@Type", "DesignName");

            return await _sqlDataAccess.SelectDataFromSPSingle<string>("GetALLMasterList", param, companyId) ?? "";
        }

        public async Task<int> GetMaxStockSerialNoAsync(string whereClause, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Where", whereClause);
            param.Add("@Type", "MaxStockSerianNo");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetALLCommonData", param, companyId);
            return int.TryParse(result, out int serialNo) ? serialNo : 0;
        }

        public async Task<bool> SaveStockDiamondDetailsDataAsync(string designId, int jewCode, int itemCode, string stockId, string type, string shape, string size, string sieve, string color, string clarity, int pcs, double carat, float price, double amount, string lab, string actionType, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@DesignID", designId);
            param.Add("@JewCode", jewCode);
            param.Add("@diamonditemcode", itemCode);
            param.Add("@StockID", stockId);
            param.Add("@Type", type);
            param.Add("@Shape", shape);
            param.Add("@Size", size);
            param.Add("@Sieve", sieve);
            param.Add("@Color", color);
            param.Add("@Clarity", clarity);
            param.Add("@Pcs", pcs);
            param.Add("@Carat", carat);
            param.Add("@Price", price);
            param.Add("@Amount", amount);
            param.Add("@Lab", lab);
            param.Add("@ActionType", actionType);
            param.Add("@CompanyID", int.Parse(companyId));

            int result = await _sqlDataAccess.ExecuteSP("SP_DiamondDetails", param, companyId);
            return result > 0;
        }

        public async Task<bool> SaveStockSettingDetailsDataAsync(string designId, int jewCode, string type, string shape, string size, int pcs, string settingType, float price, double amount, string actionType, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@DesignID", designId);
            param.Add("@JewCode", jewCode);
            param.Add("@Type", type);
            param.Add("@Shape", shape);
            param.Add("@Size", size);
            param.Add("@Pcs", pcs);
            param.Add("@SettingType", settingType);
            param.Add("@Price", price);
            param.Add("@Amount", amount);
            param.Add("@ActionType", actionType);
            param.Add("@CompanyID", int.Parse(companyId));

            int result = await _sqlDataAccess.ExecuteSP("SP_SettingDetails", param, companyId);
            return result > 0;
        }

        public async Task<bool> DeleteBothAsync(int jewCode, string actionType, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@ID", jewCode);
            param.Add("@ActionType", actionType);

            int result = await _sqlDataAccess.ExecuteSP("SP_DeleteJewDesign", param, companyId);
            return result > 0;
        }

        public async Task<List<LotNoDataJew>> GetJewLotNoDataAsync(string lotNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Type", "GetJewStockDetails" },
                { "@Where", $" AND LotNo = '{lotNo}' " }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<LotNoDataJew>(
                "GetLotNoDataForAll",
                parameters,
                companyId);

            return result.ToList();
        }
    }
}