using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IImportGradingRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        Task<bool> UpdateMultiGradingStatusAsync(string lotNo, string action, string companyId);
        Task<List<GradingListDto>> SelectMultiGradingAsync(string lotNo, string action, string companyId);
        Task<string> GetSizeAsync(string weight, string action, string companyId);
        Task<string> GetIgiParameterAsync(string parameter, string parameterValue, string usedFor, string companyId);
        Task<long> GetCurCodeAsync(string where, string companyId);
        Task<long> GetJewCodeAsync(string where, string companyId);
        Task<int> GetJewStockMasterCountAsync(string where, string companyId);
        Task<bool> UpdateImageVideoPathAsync(string lotNo, string imagePath, string videoPath, string pagePath, string action, string companyId);
        Task<bool> UpdateSizeIdsAsync(string companyId);
        Task<bool> DelRecordsAsync(string where, string companyId);
        Task<bool> UpdateCompanyLoginLogoAsync(string value, string action, string companyId);
        Task<string> GetLotExistAsync(string where, string companyId);
        Task<string> GetMCurOperatorAsync(string where, string companyId);
        Task<string> GetSCurOperatorAsync(string where, string companyId);
        Task<float> GetMCurIdAsync(string companyId);
        Task<float> GetSCurIdAsync(string companyId);
        Task<bool> UpdateAskingPurchaseRateAsync(string lotNo, string purchaseRate, string askingRate, string companyId);
        Task<string[]> GetConvertAmountAsync(float curCode, float amount, float mCurBaseRate, string mCurOperator, float sCurBaseRate, string sCurOperator, string companyId);
        Task<List<SettingMasterItemDto>> GetSettingMasterListAsync(string companyId);

        Task<bool> SaveItemMasterData(
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
            System.DateTime purchaseDate, System.DateTime rateReviseDate, System.DateTime newArrivalDate,
            System.DateTime stoneLastUnHoldDate, string publicRemarks, string privateRemarks, string adminRemarks,
            float purchaseBack, int singlePcs, float purchaseRate, float purchaseAmount, float actualBack,
            float actualRate, float actualAmount, float bottomBack, float bottomRate, float b2bBack, float b2bRate,
            float b2cBack, float b2cRate, string mfgSeriesId, string mfgPacketId, string roughId, string minesId,
            string actionType, int companyId, string sieveId, float masterCurrencyRate = 0, float masterCurrencyAmount = 0,
            float stockCurrencyRate = 0, float stockCurrencyAmount = 0, string companyIdStr = "");

        Task<bool> SaveAccGrpMasterData(
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
            string companyIdStr = "");
    }
}
