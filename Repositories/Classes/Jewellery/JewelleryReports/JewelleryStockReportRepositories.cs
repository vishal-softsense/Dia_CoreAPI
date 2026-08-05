using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Repositories.Classes.Jewellery.JewelleryReports
{
    public class JewelleryStockReportRepositories : IJewelleryStockReportRepository
    {
        private readonly ISqlDataAccess _db;

        public JewelleryStockReportRepositories(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<JewelleryStockSearchResponseDto>> GetJewelleryStockReportAsync(JewelleryStockSearchFilterDto filter, string companyId)
        {
            string dateVal = string.IsNullOrEmpty(filter.Date) ? DateTime.Now.ToString("yyyy-MM-dd") : filter.Date;
            string where = "";

            // Show Filter
            string? show = filter.Show;
            if (!string.IsNullOrEmpty(show) && show != "All")
            {
                if (show == "Sold")
                {
                    where += " And (CurStatus = ''Sold'' or Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) = 0) ";
                }
                else if (show == "Unsold")
                {
                    where += " And (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05) ";
                }
            }

            // Status Filter
            string? status = filter.Status;
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "OnHand")
                {
                    where += " And onhand>0.01 And CurStatus in (''S'') ";
                }
                else if (status == "OnMemo")
                {
                    where += " And CurStatus in (''M'',''I'',''L'',''PS'') ";
                }
                else if (status == "OnHand&Int.Memo")
                {
                    where += " And CurStatus in (''S'',''I'') ";
                }
                else if (status == "OnHand + Memo In")
                {
                    where += " And CurStatus in (''S'',''MI'') ";
                }
                else if (status == "OnHand + Party Hold")
                {
                    where += " And onhand>0.01 And CurStatus in (''S'',''PH'') ";
                }
                else if (status == "Memo In")
                {
                    where += " And CurStatus in (''MI'') ";
                }
                else if (status == "Party Hold")
                {
                    where += " And CurStatus in (''PH'') ";
                }
                else if (status == "All - Party Hold")
                {
                    where += " And CurStatus not in (''PH'') ";
                }
            }

            // Asking Rate / Amount Calculations
            float backPer = filter.BackPer;
            float backPlusMinus = filter.BackPlusMinus;
            float ratePer = filter.RatePer;
            float ratePlusMinus = filter.RatePlusMinus;

            string askingQry = "";
            if (backPer != 0)
            {
                askingQry = $" Rapa+(Rapa*({backPer})/100) AS AskingRate,(Rapa+(Rapa*({backPer})/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,{backPer} As AskingBack ";
            }
            else if (backPlusMinus != 0)
            {
                askingQry = $" Rapa+(Rapa*(ViewStockReport.back+{backPlusMinus})/100) AS AskingRate,(Rapa+(Rapa*(ViewStockReport.back+{backPlusMinus})/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,ViewStockReport.Back+{backPlusMinus} As AskingBack ";
            }
            else if (ratePer != 0)
            {
                askingQry = $" (ROUND(((Rate+(Rate*({ratePer})/100))/10)+.49,0))*10 AS AskingRate,(ROUND(((Rate+(Rate*({ratePer})/100))/10)+.49,0))*10 * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE (ROUND(((Rate+(Rate*({ratePer})/100))/10)+.49,0))*10 * 100 / Rapa - 100 END AS AskingBack ";
            }
            else if (ratePlusMinus != 0)
            {
                askingQry = $" Rate+{ratePlusMinus} AS AskingRate, ((rate+{ratePlusMinus}) * Round((CONVERT(decimal(18,3),TotCarats + MemoIn )),3,1)) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE ((((Rate+{ratePlusMinus} )*100) / Rapa) - 100) END As AskingBack ";
            }

            // StockId / LotNo / CertNo Filters
            if (!string.IsNullOrEmpty(filter.StockId))
            {
                string formattedStockId = filter.StockId.Replace("\r\n", ",").Replace("\n", ",").Replace(",", "'',''");
                if (filter.Filters == "LotNo")
                {
                    where += $" AND (LotNo IN (''{formattedStockId}''))";
                }
                else if (filter.Filters == "LotNo Not")
                {
                    where += $" AND (LotNo NOT IN (''{formattedStockId}''))";
                }
                else if (filter.Filters == "LotNo Like")
                {
                    where += $" AND (LotNo Like (''{formattedStockId}%''))";
                }
                else if (filter.Filters == "CertNo")
                {
                    where += $" AND ([JewelleryCerNo] IN (''{formattedStockId}''))";
                }
                else if (filter.Filters == "CertNo Not")
                {
                    where += $" AND ([JewelleryCerNo] NOT IN (''{formattedStockId}''))";
                }
            }

            // Jewellery Master Attributes
            if (!string.IsNullOrEmpty(filter.JewDesign))
            {
                where += $" And DesignName In ( ''{filter.JewDesign}'' )";
            }
            if (!string.IsNullOrEmpty(filter.JewCategory))
            {
                where += $" And Category In ( ''{filter.JewCategory}'' )";
            }
            if (!string.IsNullOrEmpty(filter.JewSubCategory))
            {
                where += $" And SubCategory In ( ''{filter.JewSubCategory}'' )";
            }
            if (filter.JewSize != 0)
            {
                where += $" And JewSize In ( ''{filter.JewSize}'' )";
            }
            if (!string.IsNullOrEmpty(filter.JewCertificate))
            {
                where += $" And LAB In ( ''{filter.JewCertificate}'' )";
            }
            if (!string.IsNullOrEmpty(filter.JewCertificateNo))
            {
                where += $" And JewelleryCerNo In ( ''{filter.JewCertificateNo}'' )";
            }
            if (!string.IsNullOrEmpty(filter.JewMetalType))
            {
                where += $" And MetalType In ( ''{filter.JewMetalType}'' )";
            }
            if (!string.IsNullOrEmpty(filter.JewColor))
            {
                where += $" And Color In ( ''{filter.JewColor}'' )";
            }
            if (!string.IsNullOrEmpty(filter.JewPurity))
            {
                where += $" And Purity In ( ''{filter.JewPurity}'' )";
            }

            // Diamond Sub-details Filters
            if (!string.IsNullOrEmpty(filter.DiamondType))
            {
                string diamondTypes = $"''{filter.DiamondType.Replace(",", "','")}''";
                where += $" AND JewCode IN ( SELECT D.JewCode FROM TabJewStockDiamondDetails D WHERE D.DiamondType IN ({diamondTypes}) )";
            }

            if (filter.DiamondShape != null && filter.DiamondShape.Count > 0)
            {
                string shapeStr = string.Join(",", filter.DiamondShape.Select(s => $"''{s.Trim()}''"));
                if (!string.IsNullOrEmpty(shapeStr))
                {
                    where += $" AND JewCode IN ( SELECT D.JewCode FROM TabJewStockDiamondDetails D WHERE D.DiamondShape IN ({shapeStr}) )";
                }
            }

            if (filter.DiamondSize != null && filter.DiamondSize.Count > 0)
            {
                string sizeStr = string.Join(",", filter.DiamondSize.Select(s => $"''{s.Trim()}''"));
                if (!string.IsNullOrEmpty(sizeStr))
                {
                    where += $" AND JewCode IN ( SELECT D.JewCode FROM TabJewStockDiamondDetails D WHERE D.DiamondSize IN ({sizeStr}) )";
                }
            }

            if (filter.DiamondColor != null && filter.DiamondColor.Count > 0)
            {
                string colorStr = string.Join(",", filter.DiamondColor.Select(s => $"''{s.Trim()}''"));
                if (!string.IsNullOrEmpty(colorStr))
                {
                    where += $" AND JewCode IN ( SELECT D.JewCode FROM TabJewStockDiamondDetails D WHERE D.DiamondColor IN ({colorStr}) )";
                }
            }

            if (filter.DiamondClarity != null && filter.DiamondClarity.Count > 0)
            {
                string clarityStr = string.Join(",", filter.DiamondClarity.Select(s => $"''{s.Trim()}''"));
                if (!string.IsNullOrEmpty(clarityStr))
                {
                    where += $" AND JewCode IN ( SELECT D.JewCode FROM TabJewStockDiamondDetails D WHERE D.DiamondClarity IN ({clarityStr}) )";
                }
            }

            string lotno = filter.JewLotNo ?? "";
            string shape = "";
            string size = "";
            string color = "";
            string clarity = "";
            string diamondtype = "";

            string sql = $@" EXEC [GetJewelleryStockReportData] @Date = '''{dateVal}''',@AskingQry = '{askingQry}',@Where = '{where}',@ActionType = 'JewelleryStockReportNew',@LotNoFilter='{lotno}'," +
                $"@ShapeFilter = {PrepareFilter(shape)}," +
                $"@SizeFilter = {PrepareFilter(size)}," +
                $"@ColorFilter = {PrepareFilter(color)}," +
                $"@ClarityFilter = {PrepareFilter(clarity)}," +
                $"@DiamondType = {PrepareFilter(diamondtype)}";

            var reportData = (await _db.SelectRawSql<JewelleryStockReportItemDto>(sql, companyId)).ToList();

            var resultList = new List<JewelleryStockSearchResponseDto>();
            if (reportData.Count > 0)
            {
                // Optimization: Fetch all diamond details in a single query for all returned JewCodes
                var jewCodes = reportData.Select(x => x.JewCode).Distinct().ToList();
                var allDiamondDetails = new List<JewelleryStockDiamondDetailDto>();

                if (jewCodes.Count > 0)
                {
                    string jewCodeInClause = string.Join(",", jewCodes);
                    string diamondSql = $@" EXEC [GetALLMasterList] @Type='GetJewStockDiamond', @Where = ' AND JewCode IN ({jewCodeInClause})' ";
                    allDiamondDetails = (await _db.SelectRawSql<JewelleryStockDiamondDetailDto>(diamondSql, companyId)).ToList();
                }

                var diamondGroup = allDiamondDetails.GroupBy(x => x.JewCode).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in reportData)
                {
                    List<JewelleryStockDiamondDetailDto> itemDiamonds = diamondGroup.TryGetValue(item.JewCode, out var details) ? details : new List<JewelleryStockDiamondDetailDto>();

                    var diamondDetails = itemDiamonds
                        .Where(x => x.DiamondType == "Natural Diamond" || x.DiamondType == "Lab Grown Diamond")
                        .ToList();

                    var stoneDetails = itemDiamonds
                        .Where(x => x.DiamondType == "Gem Stone")
                        .ToList();

                    resultList.Add(new JewelleryStockSearchResponseDto
                    {
                        Jewellery = item,
                        DiamondDetails = diamondDetails,
                        StoneDetails = stoneDetails
                    });
                }
            }

            return resultList;
        }

        public async Task<JewelleryStockDropdownsDto> GetJewelleryStockDropdownsAsync(string companyId)
        {
            // Typed ADO.NET mapping (same GetALLMasterList SPs as MVC) so JSON exposes CategoryID / MetalID / JewColorID correctly
            var categoriesTask = _db.SelectRawSql<JewCategoryMaster>("EXEC [GetALLMasterList] @Type='JewCategory', @Where = ''", companyId);
            var subCategoriesTask = _db.SelectRawSql<JewSubCategoryMaster>("EXEC [GetALLMasterList] @Type='JewSubCategory', @Where = ''", companyId);
            var metalsTask = _db.SelectRawSql<JewMetalMaster>("EXEC [GetALLMasterList] @Type='JewMetal', @Where = ''", companyId);
            var colorsTask = _db.SelectRawSql<JewColorMaster>("EXEC [GetALLMasterList] @Type='JewColor', @Where = ''", companyId);
            var masterDataTask = _db.SelectDataFromSP<BasicDataDto>("AllMasterDataForAdminStockSearch", new Dictionary<string, object>(), companyId);
            var itemMasterTask = _db.SelectDataFromSP<BasicDataDto>("GetAllMasterDataForItemMaster", new Dictionary<string, object>(), companyId);

            await Task.WhenAll(categoriesTask, subCategoriesTask, metalsTask, colorsTask, masterDataTask, itemMasterTask);

            return new JewelleryStockDropdownsDto
            {
                CategoryList = await categoriesTask,
                SubCategoryList = await subCategoriesTask,
                MetalTypeList = await metalsTask,
                JewColorList = await colorsTask,
                MasterDropdowns = await masterDataTask,
                ItemMasterDropdowns = await itemMasterTask
            };
        }

        public async Task<IEnumerable<BasicDataDto>> GetStockIdsAsync(string searchText, string companyId)
        {
            string strWhere = "";
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Replace("\r\n", ",").Replace("\n", ",");
                strWhere += " AND Name IN (''" + searchText.Replace(",", "'',''") + "'') ";
            }

            string sql = $@" EXEC [GetJewStockDropdown] @strWhere = '{strWhere}' ";
            return await _db.SelectRawSql<BasicDataDto>(sql, companyId);
        }

        private static string PrepareFilter(string val)
        {
            if (string.IsNullOrEmpty(val)) return "''";
            return "'" + val.Replace("'", "''") + "'";
        }
    }
}
