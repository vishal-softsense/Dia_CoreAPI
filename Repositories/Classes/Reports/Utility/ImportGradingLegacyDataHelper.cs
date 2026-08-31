using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    /// <summary>Legacy SP helpers ported from MVC ItemMasterProcessor / AccGrpMasterProcessor.</summary>
    internal static class ImportGradingLegacyDataHelper
    {
        public static async Task<bool> SaveItemMasterDataAsync(
            ISqlDataAccess db,
            int itemCode, int itemSrno, string itemId, string itemName, string itemGroupId, string category,
            string productType, float pcs, double weight, string shapeId, string colorId, string sizeId, string clarityId,
            float mrp, float back, float askingRate, float askingAmount, string boxName, float boxRate, string certId,
            string certificateNo, string haid, string propId, string polishId, string symId, string flId, string flColor,
            string fancyColor, string fancyColorIntensity, string fancyColorOvertone, float l, float w, float d,
            string diameter, string measurement, string lwRatio, float totDepth, float tablePer, float crownHeight,
            float crownAngle, float pavilionDepth, float pavilionHeight, float pavilionAngle, float girdlePer,
            string girdleCondition, float girdleMin, float girdleMax, float girdleMinMs, float girdleMaxMs,
            string culetId, string culetCondition, float strLn, float lrHalf, bool laserInscription,
            string laserInscriptionText, string instruction, string keyToSymbols, string labComments, string memberComments,
            string brownId, string greenId, string milkyId, string lusterId, string buthId, string nattsId,
            string eyeClean, string shadeId, string tableInclusion, string crownInclusion, string oit, string oic,
            string oip, string oig, string polishFeatures, string symFeatures, string colorDescription,
            string clarityDescription, string clarityStatus, string graining, string painting, string idealImagePath,
            string assetImagePath, string heartsImagePath, string arrowsImagePath, string imagePath, string videoPath,
            string b2bDnaPagePath, string b2cDnaPagePath, string certificatePath, string plottingImagePath,
            DateTime purchaseDate, DateTime rateReviseDate, DateTime newArrivalDate, DateTime stoneLastUnHoldDate,
            string publicRemarks, string privateRemarks, string adminRemarks, float purchaseBack, int singlePcs,
            float purchaseRate, float purchaseAmount, float actualBack, float actualRate, float actualAmount,
            float bottomBack, float bottomRate, float b2bBack, float b2bRate, float b2cBack, float b2cRate,
            string mfgSeriesId, string mfgPacketId, string roughId, string minesId, string actionType, int companyId,
            string sieveId, float masterCurrencyRate = 0, float masterCurrencyAmount = 0, float stockCurrencyRate = 0,
            float stockCurrencyAmount = 0, string companyIdStr = "")
        {
            var parameters = new Dictionary<string, object>
            {
                { "@ItemCode", itemCode }, { "@ItemSrno", itemSrno }, { "@LotNo", itemId }, { "@ItemName", itemName },
                { "@ItemGroupId", itemGroupId }, { "@Category", category }, { "@ProductType", productType }, { "@Pcs", pcs },
                { "@Weight", 0 }, { "@CaratCL", weight }, { "@SieveId", sieveId }, { "@ShapeId", shapeId },
                { "@ColorId", colorId }, { "@SizeId", sizeId }, { "@ClarityId", clarityId }, { "@MRP", mrp },
                { "@Back", back }, { "@AskingRate", askingRate }, { "@AskingAmount", askingAmount }, { "@BoxName", boxName },
                { "@BoxRate", boxRate }, { "@CertId", certId }, { "@CertificateNo", certificateNo }, { "@HAID", haid },
                { "@PropId", propId }, { "@PolishId", polishId }, { "@SymId", symId }, { "@FLId", flId }, { "@FLColor", flColor },
                { "@FancyColor", fancyColor }, { "@FancyColorIntensity", fancyColorIntensity }, { "@FancyColorOvertone", fancyColorOvertone },
                { "@L", l }, { "@W", w }, { "@D", d }, { "@Diameter", diameter }, { "@Measurement", measurement }, { "@LWRatio", lwRatio },
                { "@TotDepth", totDepth }, { "@TablePer", tablePer }, { "@CrownHeight", crownHeight }, { "@CrownAngle", crownAngle },
                { "@PavilionDepth", pavilionDepth }, { "@PavilionHeight", pavilionHeight }, { "@PavilionPer", pavilionAngle },
                { "@GirdlePer", girdlePer }, { "@GirdleCondition", girdleCondition }, { "@GirdleMin", girdleMin }, { "@GirdleMax", girdleMax },
                { "@GirdleMinMS", girdleMinMs }, { "@GirdleMaxMS", girdleMaxMs }, { "@CuletId", culetId }, { "@CuletCondition", culetCondition },
                { "@StrLn", strLn }, { "@LrHalf", lrHalf }, { "@LaserInscription", laserInscription }, { "@LaserInscriptionText", laserInscriptionText },
                { "@Instruction", instruction }, { "@KeyToSymbols", keyToSymbols }, { "@LABComments", labComments }, { "@MemberComments", memberComments },
                { "@BrownId", brownId }, { "@GreenId", greenId }, { "@MilkyId", milkyId }, { "@LusterId", lusterId }, { "@ButhId", buthId },
                { "@NattsId", nattsId }, { "@EyeClean", eyeClean }, { "@ShadeId", shadeId }, { "@TableInclusion", tableInclusion },
                { "@CrownInclusion", crownInclusion }, { "@OIT", oit }, { "@OIC", oic }, { "@OIP", oip }, { "@OIG", oig },
                { "@PolishFeatures", polishFeatures }, { "@SymFeatures", symFeatures }, { "@ColorDescription", colorDescription },
                { "@ClarityDescription", clarityDescription }, { "@ClarityStatus", clarityStatus }, { "@Graining", graining }, { "@Painting", painting },
                { "@IdealImagePath", idealImagePath }, { "@AssetImagePath", assetImagePath }, { "@HeartsImagePath", heartsImagePath },
                { "@ArrowsImagePath", arrowsImagePath }, { "@ImagePath", imagePath }, { "@VideoPath", videoPath },
                { "@B2BDNAPagePath", b2bDnaPagePath }, { "@B2CDNAPagePath", b2cDnaPagePath }, { "@CertificatePath", certificatePath },
                { "@PlottingImagePath", plottingImagePath }, { "@PurchaseDate", purchaseDate }, { "@RateReviseDate", rateReviseDate },
                { "@NewArrivalDate", newArrivalDate }, { "@StoneLastUnHoldDate", stoneLastUnHoldDate }, { "@PublicRemarks", publicRemarks },
                { "@PrivateRemarks", privateRemarks }, { "@AdminRemarks", adminRemarks }, { "@PurchaseRate", purchaseRate },
                { "@PurchaseBack", purchaseBack }, { "@SinglePcs", singlePcs }, { "@PurchaseAmount", purchaseAmount },
                { "@ActualRate", actualRate }, { "@ActualBack", actualBack }, { "@ActualAmount", actualAmount },
                { "@BottomBack", bottomBack }, { "@BottomRate", bottomRate }, { "@B2BRate", b2bRate }, { "@B2BBack", b2bBack },
                { "@B2CRate", b2cRate }, { "@B2CBack", b2cBack }, { "@MfgSeriesID", mfgSeriesId }, { "@MfgPacketID", mfgPacketId },
                { "@RoughId", roughId }, { "@MinesId", minesId }, { "@MasterCurrencyRate", masterCurrencyRate },
                { "@MasterCurrencyAmount", masterCurrencyAmount }, { "@StockCurrencyRate", stockCurrencyRate },
                { "@StockCurrencyAmount", stockCurrencyAmount }, { "@CompanyID", companyId }, { "@ActionType", actionType }
            };

            var cid = string.IsNullOrEmpty(companyIdStr) ? companyId.ToString() : companyIdStr;
            return await db.ExecuteSP("SP_ItemMaster", parameters, cid) > 0;
        }

        public static async Task<bool> SaveAccGrpMasterDataAsync(
            ISqlDataAccess db,
            int accGrpCode, string accGrpName, string sName, int uGrCode, int accTypeCode, int acLevel,
            string oDrCr, double oBalance, int curCode, float masterCurrencyRate, double masterCurrencyAmount,
            float stockCurrencyRate, double stockCurrencyAmount, string ag, string areaCode, string cityCode,
            string stateCode, string zipCode, string countryCode, string homePhone, string officePhone, string mobile,
            string email, string website, string remarks, string shipName, string shipStret, string shipAreaCode,
            string shipCityCode, string shipStateCode, string shipZipCode, string shipCountryCode, string shipPhone,
            string shipFax, int buyerCode, int salesMan1Code, float salesMan1Per, int salesMan2Code, double salesMan2Per,
            double interestPer, double discountPer, double creditLimit, int dueDays, int gracePeriod, string accEffCode,
            string accEffSide, int through, int broker, string executive, string termName, double termPer, string grNo,
            string pan, string cstNo, string bankAccNo, string bankAccDetail, string consigneeDetail, string tinNo,
            double brokerPer, double termValueBack, string fax, string skypeId, string contactPerson, string agent,
            string addressLine1, string addressLine2, string addressLine3, string refParty1, string refPhone1,
            string refComments1, string bankAccNo1, string bankRouting, string bankSwiftCode, string refParty2,
            string refPhone2, string refComments2, bool compilance, bool inactive, string localLangName,
            string localLangContactPerson, string localLangAddress, string customerType, string qqId, string cinNo,
            bool emailInventory, string websiteUserId, int companyId, string markup, string kam, string actionType,
            string companyIdStr = "")
        {
            var parameters = new Dictionary<string, object>
            {
                { "@AccGrpCode", accGrpCode }, { "@AccGrpName", accGrpName }, { "@SName", sName }, { "@UGrCode", uGrCode },
                { "@AccTypeCode", accTypeCode }, { "@AcLevel", acLevel }, { "@ODrCr", oDrCr }, { "@OBalance", oBalance },
                { "@CurCode", curCode }, { "@MasterCurrencyRate", masterCurrencyRate }, { "@MasterCurrencyAmount", masterCurrencyAmount },
                { "@StockCurrencyRate", stockCurrencyRate }, { "@StockCurrencyAmount", stockCurrencyAmount }, { "@AG", ag },
                { "@street", ag }, { "@AreaCode", areaCode }, { "@CityCode", cityCode }, { "@StateCode", stateCode },
                { "@ZipCode", zipCode }, { "@CountryCode", countryCode }, { "@HomePhone", homePhone }, { "@OfficePhone", officePhone },
                { "@Mobile", mobile }, { "@Email", email }, { "@Website", website }, { "@Remarks", remarks }, { "@ShipName", shipName },
                { "@ShipStreet", shipStret }, { "@ShipAreaCode", shipAreaCode }, { "@ShipCityCode", shipCityCode },
                { "@ShipStateCode", shipStateCode }, { "@ShipZipCode", shipZipCode }, { "@ShipCountryCode", shipCountryCode },
                { "@ShipPhone", shipPhone }, { "@ShipFax", shipFax }, { "@BuyerCode", buyerCode }, { "@SalesMan1Code", salesMan1Code },
                { "@SalesMan1Per", salesMan1Per }, { "@SalesMan2Code", salesMan2Code }, { "@SalesMan2Per", salesMan2Per },
                { "@InterestPer", interestPer }, { "@DiscountPer", discountPer }, { "@CreditLimit", creditLimit },
                { "@DueDays", dueDays }, { "@GracePeriod", gracePeriod }, { "@AccEffCode", accEffCode }, { "@AccEffSide", accEffSide },
                { "@Through", through }, { "@Broker", broker }, { "@Executive", executive }, { "@TermName", termName },
                { "@TermPer", termPer }, { "@GrNo", grNo }, { "@PAN", pan }, { "@CSTNo", cstNo }, { "@BankAccNo", bankAccNo },
                { "@BankAccDetail", bankAccDetail }, { "@ConsigneeDetail", consigneeDetail }, { "@TINNo", tinNo },
                { "@BrokerPer", brokerPer }, { "@TermValueBack", termValueBack }, { "@Fax", fax }, { "@SkypeId", skypeId },
                { "@ContactPerson", contactPerson }, { "@Agent", agent }, { "@AddressLine1", addressLine1 },
                { "@AddressLine2", addressLine2 }, { "@AddressLine3", addressLine3 }, { "@RefParty1", refParty1 },
                { "@RefPhone1", refPhone1 }, { "@RefComments1", refComments1 }, { "@BankAccNo1", bankAccNo1 },
                { "@BankRouting", bankRouting }, { "@BankSwiftCode", bankSwiftCode }, { "@RefParty2", refParty2 },
                { "@RefPhone2", refPhone2 }, { "@RefComments2", refComments2 }, { "@Compilance", compilance },
                { "@Inactive", inactive }, { "@LocalLangName", localLangName }, { "@LocalLangContactPerson", localLangContactPerson },
                { "@LocalLangAddress", localLangAddress }, { "@CustomerType", customerType }, { "@QQID", qqId },
                { "@CINNO", cinNo }, { "@EmailInventory", emailInventory }, { "@WebsiteUserID", websiteUserId },
                { "@CompanyID", companyId }, { "@Markup", markup }, { "@KAM", kam }, { "@ActionType", actionType }
            };

            var cid = string.IsNullOrEmpty(companyIdStr) ? companyId.ToString() : companyIdStr;
            return await db.ExecuteSP("SP_AccGrpMaster", parameters, cid) > 0;
        }
    }
}
