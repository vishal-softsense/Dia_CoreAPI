using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    public class ImportGradingRepository : IImportGradingRepository
    {
        private readonly ISqlDataAccess _db;

        public ImportGradingRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
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

            var result = await _db.SelectDataFromSP<PermissionCheck>("GetUserPermission", parameters, companyId);
            return result.Count();
        }

        public async Task<bool> UpdateMultiGradingStatusAsync(string lotNo, string action, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@LotNo", lotNo },
                { "@Action", action }
            };
            return await _db.ExecuteSP("SP_UpdateMultiGradingStatus", parameters, companyId) > 0;
        }

        public async Task<List<GradingListDto>> SelectMultiGradingAsync(string lotNo, string action, string companyId)
        {
            var sql = $" EXEC [SP_UpdateMultiGradingStatus] @LotNo = '{lotNo.Replace("'", "''")}' ,@Action='{action.Replace("'", "''")}'";
            var rows = await _db.SelectRawSql<GradingListDto>(sql, companyId);
            return rows.ToList();
        }

        public async Task<string> GetSizeAsync(string weight, string action, string companyId)
        {
            var sql = $" EXEC [SP_UpdateMultiGradingStatus] @Weight = '{weight.Replace("'", "''")}' ,@Action='{action.Replace("'", "''")}'";
            var rows = await _db.SelectRawSql<string>(sql, companyId);
            return rows.FirstOrDefault() ?? string.Empty;
        }

        public async Task<bool> UpdateImageVideoPathAsync(string lotNo, string imagePath, string videoPath, string pagePath, string action, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@LotNo", lotNo },
                { "@ImagePath", imagePath },
                { "@VideoPath", videoPath },
                { "@PagePath", pagePath },
                { "@Action", action }
            };
            return await _db.ExecuteSP("SP_UpdateImageVideoPath", parameters, companyId) > 0;
        }

        public async Task<bool> UpdateSizeIdsAsync(string companyId)
        {
            // Same as MVC SqlDataAccess.UpdateSizeIds() → SaveDataInTransaction("SP_UpdateSizeids", param)
            await _db.ExecuteSP("SP_UpdateSizeids", new Dictionary<string, object>(), companyId);
            return true;
        }

        public async Task<bool> DelRecordsAsync(string where, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@ActionType", "DeleteRec" },
                { "@Where", where }
            };
            return await _db.ExecuteSP("deletedeletedRecords", parameters, companyId) > 0;
        }

        public async Task<bool> UpdateCompanyLoginLogoAsync(string value, string action, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Value", value },
                { "@Action", action }
            };
            return await _db.ExecuteSP("SP_UpdateLogo", parameters, companyId) > 0;
        }

        public async Task<string> GetLotExistAsync(string where, string companyId)
        {
            var sql = $" EXEC [GetAllCommonData] @Type='LotExist',@Where = '{where.Replace("'", "''")}' ";
            var rows = await _db.SelectRawSql<string>(sql, companyId);
            return rows.FirstOrDefault() ?? "0";
        }

        public async Task<string> GetMCurOperatorAsync(string where, string companyId)
        {
            var sql = $" EXEC [GetCurrencyDetail] @Type='MCurOperator',@Where = '{where.Replace("'", "''")}' ";
            var rows = await _db.SelectRawSql<string>(sql, companyId);
            return rows.FirstOrDefault() ?? string.Empty;
        }

        public async Task<string> GetSCurOperatorAsync(string where, string companyId)
        {
            var sql = $" EXEC [GetCurrencyDetail] @Type='SCurOperator',@Where = '{where.Replace("'", "''")}' ";
            var rows = await _db.SelectRawSql<string>(sql, companyId);
            return rows.FirstOrDefault() ?? string.Empty;
        }

        public async Task<float> GetMCurIdAsync(string companyId)
        {
            var sql = " EXEC [GetCurrencyDetail] @Type='MCurID'";
            var rows = await _db.SelectRawSql<float>(sql, companyId);
            return rows.FirstOrDefault();
        }

        public async Task<float> GetSCurIdAsync(string companyId)
        {
            var sql = " EXEC [GetCurrencyDetail] @Type='SCurID'";
            var rows = await _db.SelectRawSql<float>(sql, companyId);
            return rows.FirstOrDefault();
        }

        public async Task<bool> UpdateAskingPurchaseRateAsync(string lotNo, string purchaseRate, string askingRate, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@LotNo", lotNo },
                { "@PurchaseRate", purchaseRate },
                { "@AskingRate", askingRate }
            };
            return await _db.ExecuteSP("SP_UpdateAskingPurchaseRate", parameters, companyId) > 0;
        }

        public async Task<string[]> GetConvertAmountAsync(float curCode, float amount, float mCurBaseRate, string mCurOperator, float sCurBaseRate, string sCurOperator, string companyId)
        {
            var mCurId = await GetMCurIdAsync(companyId);
            var sCurId = await GetSCurIdAsync(companyId);
            return Dia_CoreAPI.Services.Helpers.ImportGradingHelper.GetConvertAmount(curCode, amount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurId, sCurId);
        }

        public async Task<List<SettingMasterItemDto>> GetSettingMasterListAsync(string companyId)
        {
            const string sql = "SELECT Name, Value FROM TabSettingMaster";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            var list = new List<SettingMasterItemDto>();
            foreach (var row in rows)
            {
                list.Add(new SettingMasterItemDto
                {
                    Name = GetString(row, "Name", "name"),
                    Value = GetString(row, "Value", "value")
                });
            }
            return list;
        }

        // Legacy SaveItemMasterData / SaveAccGrpMasterData signatures are invoked from processor helpers.
        public Task<bool> SaveItemMasterData(int itemCode, int itemSrno, string itemId, string itemName, string itemGroupId, string category, string productType, float pcs, double weight, string shapeId, string colorId, string sizeId, string clarityId, float mrp, float back, float askingRate, float askingAmount, string boxName, float boxRate, string certId, string certificateNo, string haid, string propId, string polishId, string symId, string flId, string flColor, string fancyColor, string fancyColorIntensity, string fancyColorOvertone, float l, float w, float d, string diameter, string measurement, string lwRatio, float totDepth, float tablePer, float crownHeight, float crownAngle, float pavilionDepth, float pavilionHeight, float pavilionAngle, float girdlePer, string girdleCondition, float girdleMin, float girdleMax, float girdleMinMs, float girdleMaxMs, string culetId, string culetCondition, float strLn, float lrHalf, bool laserInscription, string laserInscriptionText, string instruction, string keyToSymbols, string labComments, string memberComments, string brownId, string greenId, string milkyId, string lusterId, string buthId, string nattsId, string eyeClean, string shadeId, string tableInclusion, string crownInclusion, string oit, string oic, string oip, string oig, string polishFeatures, string symFeatures, string colorDescription, string clarityDescription, string clarityStatus, string graining, string painting, string idealImagePath, string assetImagePath, string heartsImagePath, string arrowsImagePath, string imagePath, string videoPath, string b2bDnaPagePath, string b2cDnaPagePath, string certificatePath, string plottingImagePath, DateTime purchaseDate, DateTime rateReviseDate, DateTime newArrivalDate, DateTime stoneLastUnHoldDate, string publicRemarks, string privateRemarks, string adminRemarks, float purchaseBack, int singlePcs, float purchaseRate, float purchaseAmount, float actualBack, float actualRate, float actualAmount, float bottomBack, float bottomRate, float b2bBack, float b2bRate, float b2cBack, float b2cRate, string mfgSeriesId, string mfgPacketId, string roughId, string minesId, string actionType, int companyId, string sieveId, float masterCurrencyRate = 0, float masterCurrencyAmount = 0, float stockCurrencyRate = 0, float stockCurrencyAmount = 0, string companyIdStr = "")
            => ImportGradingLegacyDataHelper.SaveItemMasterDataAsync(_db, itemCode, itemSrno, itemId, itemName, itemGroupId, category, productType, pcs, weight, shapeId, colorId, sizeId, clarityId, mrp, back, askingRate, askingAmount, boxName, boxRate, certId, certificateNo, haid, propId, polishId, symId, flId, flColor, fancyColor, fancyColorIntensity, fancyColorOvertone, l, w, d, diameter, measurement, lwRatio, totDepth, tablePer, crownHeight, crownAngle, pavilionDepth, pavilionHeight, pavilionAngle, girdlePer, girdleCondition, girdleMin, girdleMax, girdleMinMs, girdleMaxMs, culetId, culetCondition, strLn, lrHalf, laserInscription, laserInscriptionText, instruction, keyToSymbols, labComments, memberComments, brownId, greenId, milkyId, lusterId, buthId, nattsId, eyeClean, shadeId, tableInclusion, crownInclusion, oit, oic, oip, oig, polishFeatures, symFeatures, colorDescription, clarityDescription, clarityStatus, graining, painting, idealImagePath, assetImagePath, heartsImagePath, arrowsImagePath, imagePath, videoPath, b2bDnaPagePath, b2cDnaPagePath, certificatePath, plottingImagePath, purchaseDate, rateReviseDate, newArrivalDate, stoneLastUnHoldDate, publicRemarks, privateRemarks, adminRemarks, purchaseBack, singlePcs, purchaseRate, purchaseAmount, actualBack, actualRate, actualAmount, bottomBack, bottomRate, b2bBack, b2bRate, b2cBack, b2cRate, mfgSeriesId, mfgPacketId, roughId, minesId, actionType, companyId, sieveId, masterCurrencyRate, masterCurrencyAmount, stockCurrencyRate, stockCurrencyAmount, string.IsNullOrEmpty(companyIdStr) ? companyId.ToString() : companyIdStr);

        public Task<bool> SaveAccGrpMasterData(int accGrpCode, string accGrpName, string sName, int uGrCode, int accTypeCode, int acLevel, string oDrCr, double oBalance, int curCode, float masterCurrencyRate, double masterCurrencyAmount, float stockCurrencyRate, double stockCurrencyAmount, string ag, string areaCode, string cityCode, string stateCode, string zipCode, string countryCode, string homePhone, string officePhone, string mobile, string email, string website, string remarks, string shipName, string shipStret, string shipAreaCode, string shipCityCode, string shipStateCode, string shipZipCode, string shipCountryCode, string shipPhone, string shipFax, int buyerCode, int salesMan1Code, float salesMan1Per, int salesMan2Code, double salesMan2Per, double interestPer, double discountPer, double creditLimit, int dueDays, int gracePeriod, string accEffCode, string accEffSide, int through, int broker, string executive, string termName, double termPer, string grNo, string pan, string cstNo, string bankAccNo, string bankAccDetail, string consigneeDetail, string tinNo, double brokerPer, double termValueBack, string fax, string skypeId, string contactPerson, string agent, string addressLine1, string addressLine2, string addressLine3, string refParty1, string refPhone1, string refComments1, string bankAccNo1, string bankRouting, string bankSwiftCode, string refParty2, string refPhone2, string refComments2, bool compilance, bool inactive, string localLangName, string localLangContactPerson, string localLangAddress, string customerType, string qqId, string cinNo, bool emailInventory, string websiteUserId, int companyId, string markup, string kam, string actionType, string companyIdStr = "")
            => ImportGradingLegacyDataHelper.SaveAccGrpMasterDataAsync(_db, accGrpCode, accGrpName, sName, uGrCode, accTypeCode, acLevel, oDrCr, oBalance, curCode, masterCurrencyRate, masterCurrencyAmount, stockCurrencyRate, stockCurrencyAmount, ag, areaCode, cityCode, stateCode, zipCode, countryCode, homePhone, officePhone, mobile, email, website, remarks, shipName, shipStret, shipAreaCode, shipCityCode, shipStateCode, shipZipCode, shipCountryCode, shipPhone, shipFax, buyerCode, salesMan1Code, salesMan1Per, salesMan2Code, salesMan2Per, interestPer, discountPer, creditLimit, dueDays, gracePeriod, accEffCode, accEffSide, through, broker, executive, termName, termPer, grNo, pan, cstNo, bankAccNo, bankAccDetail, consigneeDetail, tinNo, brokerPer, termValueBack, fax, skypeId, contactPerson, agent, addressLine1, addressLine2, addressLine3, refParty1, refPhone1, refComments1, bankAccNo1, bankRouting, bankSwiftCode, refParty2, refPhone2, refComments2, compilance, inactive, localLangName, localLangContactPerson, localLangAddress, customerType, qqId, cinNo, emailInventory, websiteUserId, companyId, markup, kam, actionType, string.IsNullOrEmpty(companyIdStr) ? companyId.ToString() : companyIdStr);

        private static string GetString(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match != null && dict[match] != null && dict[match] != DBNull.Value)
                        return dict[match]?.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }
    }
}
