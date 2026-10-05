using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;

namespace Dia_CoreAPI.DataAccess
{
    public class SqlTempAccess : IDisposable
    {
        public bool InsertSalePur(int Invno, string BillNo, string Trntype, string Invtype, string Category, DateTime Trndate, int Duedays, DateTime DueDate, long Partycode, long Sellercode, string term,
            long Countrycode, long? BrokerCode, float BrokerPer, float BrokerAmount, long? Adatcode, float Adatper, float AdatAmount, float TotCarats, double TotAmount, double NetAmount, double PlusMinAmount,
            double RoundAmount, double FinalAmount, double BaseRate, string Disc, string Curcode, bool outstanding, bool Stockeffected, bool Brokerage, bool Commission, bool Cheque, long usercode,
            double mastercurruncyrate, double mastercurrencyfinalamount, double stockcurrencyrate, double stockcurrencyfinalamount, long exportpartycode, [Optional] DateTime RoundDate,
            [Optional] int DivideDays, [Optional] float interestper, [Optional] float vatper, [Optional] float vatamount, [Optional] string invdescription, [Optional] long bankname, [Optional] string subtype,  string shipvia,
            [Optional] string deliverto,  string tracking,  string saleperson,[Optional] string Attendee, [Optional] float labcharges, [Optional] float freightcharges, [Optional] int duedays1, [Optional] int duedays2,
            [Optional] float CGSTPer, [Optional] float CGSTAmt, [Optional] float SGSTPer, [Optional] float SGSTAmt, [Optional] float IGSTPer, [Optional] float IGSTAmt, string createdby, [Optional] int CompanyID, [Optional] double MCurNetAmount, [Optional] double MCurFinalPayRecAmount, [Optional] int Address)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@invno", Invno);
            param.Add("@billno", BillNo);
            param.Add("@trntype", Trntype);
            param.Add("@invtype", Invtype);
            param.Add("@category", Category);
            param.Add("@SubType", subtype);
            param.Add("@Address", Address);
            param.Add("@trndate", Trndate);
            param.Add("@RoundDate", RoundDate);
            param.Add("@duedays", Duedays);
            param.Add("@duedate", DueDate);
            param.Add("@DivideDays", DivideDays);
            param.Add("@partycode", Partycode);
            param.Add("@SellerCode", Sellercode);
            param.Add("@countrycode", Countrycode);
            param.Add("@BrokerCode", BrokerCode);
            param.Add("@BrokerPer", BrokerPer);
            param.Add("@BrokerAmount", BrokerAmount);
            param.Add("@adatcode", Adatcode);
            param.Add("@adatper", Adatper);
            param.Add("@adatamount", AdatAmount);
            param.Add("@totcts", TotCarats);
            param.Add("@totamount", TotAmount);
            param.Add("@netamount", NetAmount);
            param.Add("@netMamount", MCurNetAmount);
            param.Add("@PlusMinAmount", PlusMinAmount);
            param.Add("@RoundAmount", RoundAmount);
            param.Add("@FinalAmount", FinalAmount);
            param.Add("@FinalPayRecAmount", MCurFinalPayRecAmount);
            param.Add("@BaseRate", BaseRate);
            param.Add("@ExpPartyCode", exportpartycode);
            param.Add("@ShipVia", shipvia);
            param.Add("@Tracking", tracking);
            param.Add("@SalesPerson", saleperson);
            param.Add("@Attendee", Attendee);

            param.Add("@Disc", Disc);
            param.Add("@CurCode", Curcode);
            param.Add("@Outstanding", outstanding);
            param.Add("@StockEffected", Stockeffected);
            param.Add("@Brokerage", Brokerage);
            param.Add("@Commission", Commission);
            param.Add("@Cheque", Cheque);
            param.Add("@usercode", usercode);
            param.Add("@term", term);
            param.Add("@InterestPer", interestper);
            param.Add("@mastercurrencyrate", mastercurruncyrate);
            param.Add("@mastercurrencyfinalamount", mastercurrencyfinalamount);
            param.Add("@stockcurrencyrate", stockcurrencyrate);
            param.Add("@stockcurrencyfinalamount", stockcurrencyfinalamount);
            param.Add("@VatPer", vatper);
            param.Add("@VatAmount", vatamount);
            param.Add("@CGSTPer", CGSTPer);
            param.Add("@CGSTAmount", CGSTAmt);
            param.Add("@SGSTPer", SGSTPer);
            param.Add("@SGSTAmount", SGSTAmt);
            param.Add("@IGSTPer", IGSTPer);
            param.Add("@IGSTAmount", IGSTAmt);
            param.Add("@CreatedBy", createdby);
            param.Add("@CompanyID", CompanyID);
            param.Add("@ActionType", "insert");
            #endregion


            return SaveDataInTransaction("SP_InsertSalePur", param);
        }


        public bool InsertSalePurQuat(int Invno, string BillNo, string Trntype, string Invtype, string Category, DateTime Trndate, int Duedays, DateTime DueDate, long Partycode, long Sellercode, string term,
            long Countrycode, long? BrokerCode, float BrokerPer, float BrokerAmount, long? Adatcode, float Adatper, float AdatAmount, float TotCarats, double TotAmount, double NetAmount, double PlusMinAmount,
            double RoundAmount, double FinalAmount, double BaseRate, string Disc, string Curcode, bool outstanding, bool Stockeffected, bool Brokerage, bool Commission, bool Cheque, long usercode,
            double mastercurruncyrate, double mastercurrencyfinalamount, double stockcurrencyrate, double stockcurrencyfinalamount, long exportpartycode, [Optional] DateTime RoundDate,
            [Optional] int DivideDays, [Optional] float interestper, [Optional] float vatper, [Optional] float vatamount, [Optional] string invdescription, [Optional] long bankname, [Optional] string subtype, string shipvia,
            [Optional] string deliverto, string tracking, string saleperson, [Optional] float labcharges, [Optional] float freightcharges, [Optional] int duedays1, [Optional] int duedays2,
            [Optional] float CGSTPer, [Optional] float CGSTAmt, [Optional] float SGSTPer, [Optional] float SGSTAmt, [Optional] float IGSTPer, [Optional] float IGSTAmt, string createdby, [Optional] int CompanyID,
            [Optional] double MCurNetAmount, [Optional] double MCurFinalPayRecAmount, [Optional] int Address, [Optional] float GoldRate, [Optional] string DiamondCostType, [Optional] float LabourCost)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@invno", Invno);
            param.Add("@billno", BillNo);
            param.Add("@trntype", Trntype);
            param.Add("@invtype", Invtype);
            param.Add("@category", Category);
            param.Add("@SubType", subtype);
            param.Add("@Address", Address);
            param.Add("@trndate", Trndate);
            param.Add("@RoundDate", RoundDate);
            param.Add("@duedays", Duedays);
            param.Add("@duedate", DueDate);
            param.Add("@DivideDays", DivideDays);
            param.Add("@partycode", Partycode);
            param.Add("@sellercode", Sellercode);
            param.Add("@countrycode", Countrycode);
            param.Add("@BrokerCode", BrokerCode);
            param.Add("@BrokerPer", BrokerPer);
            param.Add("@BrokerAmount", BrokerAmount);
            param.Add("@adatcode", Adatcode);
            param.Add("@adatper", Adatper);
            param.Add("@adatamount", AdatAmount);
            param.Add("@totcts", TotCarats);
            param.Add("@totamount", TotAmount);
            param.Add("@netamount", NetAmount);
            param.Add("@netMamount", MCurNetAmount);
            param.Add("@PlusMinAmount", PlusMinAmount);
            param.Add("@RoundAmount", RoundAmount);
            param.Add("@FinalAmount", FinalAmount);
            param.Add("@FinalPayRecAmount", MCurFinalPayRecAmount);
            param.Add("@BaseRate", BaseRate);
            param.Add("@ExpPartyCode", exportpartycode);
            param.Add("@ShipVia", shipvia);
            param.Add("@Tracking", tracking);
            param.Add("@SalesPerson", saleperson);

            param.Add("@Disc", Disc);
            param.Add("@CurCode", Curcode);
            param.Add("@Outstanding", outstanding);
            param.Add("@StockEffected", Stockeffected);
            param.Add("@Brokerage", Brokerage);
            param.Add("@Commission", Commission);
            param.Add("@Cheque", Cheque);
            param.Add("@usercode", usercode);
            param.Add("@term", term);
            param.Add("@InterestPer", interestper);
            param.Add("@mastercurrencyrate", mastercurruncyrate);
            param.Add("@mastercurrencyfinalamount", mastercurrencyfinalamount);
            param.Add("@stockcurrencyrate", stockcurrencyrate);
            param.Add("@stockcurrencyfinalamount", stockcurrencyfinalamount);
            param.Add("@VatPer", vatper);
            param.Add("@VatAmount", vatamount);
            param.Add("@CGSTPer", CGSTPer);
            param.Add("@CGSTAmount", CGSTAmt);
            param.Add("@SGSTPer", SGSTPer);
            param.Add("@SGSTAmount", SGSTAmt);
            param.Add("@IGSTPer", IGSTPer);
            param.Add("@IGSTAmount", IGSTAmt);
            param.Add("@CreatedBy", createdby);
            param.Add("@CompanyID", CompanyID);
            param.Add("@GoldRate", GoldRate);
            param.Add("@DiamondCostType", DiamondCostType);
            param.Add("@LabourCost", LabourCost);
            param.Add("@ActionType", "insert");
            #endregion



            return SaveDataInTransaction("SP_InsertSalePurQuat", param);
        }
        public string GetLotExist(string Where)
        {
           // string sql = @" Select count(LotNO) From TabItemMaster WHERE 1=1 " + Where;

            

            string sql = @" EXEC [GetAllCommonData] @Type='LotExist',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }


        public bool SaveItemMasterDataForPurchase(int ItemCode, string LotNo, int ItemSrno, string ItemName, string ItemGroupId, string Category, string ProductType, double Pcs,
            double Weight,
            string SieveId, string ShapeId, string ColorId, string SizeId, string ClarityId, double MRP, double Back, double AskingRate, double AskingAmount,
            string BoxName, double BoxRate, string CertId, string CertificateNo, string HAID, string PropId, string PolishId, string SymId, string FLId, string FLColor,
            string FancyColor, string FancyColorIntensity, string FancyColorOvertone, double L, double W, double D, string Diameter, string Measurement, double LWRatio,
            double TotDepth, double TablePer, double CrownHeight, double CrownAngle, double PavilionDepth, double PavilionHeight, double GirdlePer, string GirdleCondition,
            string GirdleMin, string GirdleMax, double GirdleMinMS, double GirdleMaxMS, string CuletId, string CuletCondition, double StrLn, double LrHalf, bool LaserInscription,
            string LaserInscriptionText, string Instruction, string KeyToSymbols, string LABComments, string BrownId, string GreenId, string MilkyId, string LusterId, string ButhId,
            string NattsId, string EyeClean, string ShadeId, string TableInclusion, string CrownInclusion, string OIT, string OIC, string OIP, string OIG, string PolishFeatures,
            string SymFeatures, string ColorDescription, string ClarityDescription, string ClarityStatus, string Graining, string Painting, string IdealImagePath, string AssetImagePath,
            string HeartsImagePath, string ArrowsImagePath, 
            string B2BVideoPath, string B2CVideoPath, string B2BDNAPagePath, string B2CDNAPagePath, string CertificatePath, string PlottingImagePath,
            string PublicRemarks, string PrivateRemarks, string AdminRemarks, double PurchaseBack, double PurchaseRate,
            double PurchaseAmount, double ActualBack, double ActualRate, double ActualAmount, double BottomBack, double BottomRate,
            double B2BBack, double B2BRate, double B2CBack, double B2CRate, string MfgSeriesID, string MfgPacketID, string RoughId, string MinesId, DateTime PurchaseDate,
            double Less1, double Less2, double Less3, int CompanyID, string ActionType, double PurchaseCarats, double SPPurchaseRate, double SPPurchaseBack, double PurchaseMRP,
            double PDollarrate, double MasterCurrencyRate, double MasterCurrencyAmount, double StockCurrencyRate, double StockCurrencyAmount, double AdatPer, double Brokerper, string ReportDate, int SinglePcs, [Optional] string City, [Optional] string ImagePath, [Optional] string VideoPath, [Optional] string PagePath, [Optional] string CountryOfOrigin, [Optional] string CountryOfMfg)

        //DateTime PurchaseDate, DateTime RateReviseDate, DateTime NewArrivalDate, DateTime StoneLastUnHoldDate, 
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@ItemCode", ItemCode);
            param.Add("@ItemSrno", ItemSrno);
            param.Add("@LotNo", LotNo);
            param.Add("@ItemName", ItemName);
            param.Add("@ItemGroupId", ItemGroupId);
            param.Add("@Category", Category);
            param.Add("@ProductType", ProductType);
            param.Add("@Pcs", Pcs);
            param.Add("@Weight", 0);
            param.Add("@CaratCL", Weight);

            param.Add("@SieveId", SieveId);
            param.Add("@ShapeId", ShapeId);
            param.Add("@ColorId", ColorId);
            param.Add("@SizeId", SizeId);
            param.Add("@ClarityId", ClarityId);
            param.Add("@MRP", MRP);

            param.Add("@Back", Back);
            param.Add("@AskingRate", AskingRate);
            param.Add("@AskingAmount", AskingAmount);
            param.Add("@AmountCl", PurchaseAmount);
            param.Add("@BoxName", BoxName);
            param.Add("@BoxRate", BoxRate);
            param.Add("@CertId", CertId);
            param.Add("@CertificateNo", CertificateNo);
            param.Add("@HAID", HAID);
            param.Add("@PropId", PropId);
            param.Add("@PolishId", PolishId);
            param.Add("@SymId", SymId);
            param.Add("@FLId", FLId);
            param.Add("@FLColor", FLColor);
            param.Add("@FancyColor", FancyColor);
            param.Add("@FancyColorIntensity", FancyColorIntensity);
            param.Add("@FancyColorOvertone", FancyColorOvertone);
            param.Add("@L", L);
            param.Add("@W", W);
            param.Add("@D", D);
            param.Add("@Diameter", Diameter);
            param.Add("@Measurement", Measurement);
            param.Add("@LWRatio", LWRatio);
            param.Add("@TotDepth", TotDepth);
            param.Add("@TablePer", TablePer);
            param.Add("@CrownHeight", CrownHeight);
            param.Add("@CrownAngle", CrownAngle);
            //param.Add("@PavilionDepth", PavilionDepth);
            param.Add("@PavilionPer", PavilionDepth);
            param.Add("@PavilionHeight", PavilionHeight);            
            param.Add("@GirdlePer", GirdlePer);
            param.Add("@GirdleCondition", GirdleCondition);
            param.Add("@GirdleMin", GirdleMin);
            param.Add("@GirdleMax", GirdleMax);
            param.Add("@GirdleMinMS", GirdleMinMS);
            param.Add("@GirdleMaxMS", GirdleMaxMS);
            param.Add("@CuletId", CuletId);
            param.Add("@CuletCondition", CuletCondition);
            param.Add("@StrLn", StrLn);
            param.Add("@LrHalf", LrHalf);
            param.Add("@LaserInscription", LaserInscription);
            param.Add("@LaserInscriptionText", LaserInscriptionText);
            param.Add("@Instruction", Instruction);
            param.Add("@KeyToSymbols", KeyToSymbols);
            param.Add("@LABComments", LABComments);
            param.Add("@BrownId", BrownId);
            param.Add("@GreenId", GreenId);
            param.Add("@MilkyId", MilkyId);
            param.Add("@LusterId", LusterId);
            param.Add("@ButhId", ButhId);
            param.Add("@NattsId", NattsId);
            param.Add("@EyeClean", EyeClean);
            param.Add("@ShadeId", ShadeId);
            param.Add("@TableInclusion", TableInclusion);
            param.Add("@CrownInclusion", CrownInclusion);
            param.Add("@OIT", OIT);
            param.Add("@OIC", OIC);
            param.Add("@OIP", OIP);
            param.Add("@OIG", OIG);
            param.Add("@PolishFeatures", PolishFeatures);
            param.Add("@SymFeatures", SymFeatures);
            param.Add("@ColorDescription", ColorDescription);
            param.Add("@ClarityDescription", ClarityDescription);
            param.Add("@ClarityStatus", ClarityStatus);
            param.Add("@Graining", Graining);
            param.Add("@Painting", Painting);
            param.Add("@IdealImagePath", IdealImagePath);
            param.Add("@AssetImagePath", AssetImagePath);
            param.Add("@HeartsImagePath", HeartsImagePath);
            param.Add("@ArrowsImagePath", ArrowsImagePath);
            param.Add("@B2BVideoPath", B2BVideoPath);
            param.Add("@B2CVideoPath", B2CVideoPath);
            param.Add("@B2BDNAPagePath", B2BDNAPagePath);
            param.Add("@B2CDNAPagePath", B2CDNAPagePath);
            param.Add("@CertificatePath", CertificatePath);
            param.Add("@PlottingImagePath", PlottingImagePath);
            param.Add("@PurchaseDate", PurchaseDate);
            //param.Add("@RateReviseDate", RateReviseDate);
            //param.Add("@NewArrivalDate", NewArrivalDate);
            //param.Add("@StoneLastUnHoldDate", StoneLastUnHoldDate);
            param.Add("@PublicRemarks", PublicRemarks);
            param.Add("@PrivateRemarks", PrivateRemarks);
            param.Add("@AdminRemarks", AdminRemarks);
            param.Add("@PurchaseRate", PurchaseRate);
            param.Add("@PurchaseBack", PurchaseBack);
            param.Add("@PurchaseCarats", PurchaseCarats);
            param.Add("@SPPurchaseRate", SPPurchaseRate);
            param.Add("@SPPurchaseBack", SPPurchaseBack);
            param.Add("@PurchaseMRP", PurchaseMRP);
            param.Add("@PDollorRate", PDollarrate);
            param.Add("@MasterCurrencyRate", MasterCurrencyRate);
            param.Add("@MasterCurrencyAmount", MasterCurrencyAmount);
            param.Add("@StockCurrencyRate", StockCurrencyRate);
            param.Add("@StockCurrencyAmount", StockCurrencyAmount);
            param.Add("@AdatPer", AdatPer);
            param.Add("@Brokerper", Brokerper);
            param.Add("@LabReportDate", ReportDate);
            param.Add("@SinglePcs", SinglePcs);
            param.Add("@PurchaseAmount", PurchaseAmount);
            param.Add("@ActualRate", ActualRate);
            param.Add("@ActualBack", ActualBack);
            param.Add("@ActualAmount", ActualAmount);
            param.Add("@BottomBack", BottomBack);
            param.Add("@BottomRate", BottomRate);
            param.Add("@B2BRate", B2BRate);
            param.Add("@B2BBack", B2BBack);
            param.Add("@B2CRate", B2CRate);
            param.Add("@B2CBack", B2CBack);
            param.Add("@MfgSeriesID", MfgSeriesID);
            param.Add("@MfgPacketID", MfgPacketID);
            param.Add("@RoughId", RoughId);
            param.Add("@MinesId", MinesId);
            param.Add("@Less1", Less1);
            param.Add("@Less2", Less2);
            param.Add("@Less3", Less3);
            param.Add("@CompanyID", CompanyID);

            param.Add("@City", City);
            param.Add("@ImagePath", ImagePath);
            param.Add("@VideoPath", VideoPath);
            param.Add("@PagePath", PagePath);
            param.Add("@CountryOfOrigin", CountryOfOrigin);
            param.Add("@CountryOfMfg", CountryOfMfg);
            param.Add("@ActionType", ActionType);
            #endregion

            return SaveDataInTransaction("SP_ItemMaster", param);
        }

        public bool UpdateSizeid(string LotNo)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@LotNo", LotNo);
            #endregion

            return SaveDataInTransaction("SP_UpdateSizeid", param);
        }
        public bool SaveGradingDataForPurchase(int ItemCode, string LotNo, int ItemSrno, double Weight, string SieveId, string ShapeId, string ColorId, string SizeId, string ClarityId, double MRP, double Back, double AskingRate, double AskingAmount,
         string BoxName, double BoxRate, string CertId, string CertificateNo, string HAID, string PropId, string PolishId, string SymId, string FLId, string FLColor,
         string FancyColor, string FancyColorIntensity, string FancyColorOvertone, double L, double W, double D, string Diameter, string Measurement, double LWRatio,
         double TotDepth, double TablePer, double CrownHeight, double CrownAngle, double PavilionDepth, double PavilionHeight, double GirdlePer, string GirdleCondition,
         string GirdleMin, string GirdleMax, double GirdleMinMS, double GirdleMaxMS, string CuletId, string CuletCondition, double StrLn, double LrHalf, bool LaserInscription,
         string LaserInscriptionText, string Instruction, string KeyToSymbols, string LABComments, string BrownId, string GreenId, string MilkyId, string LusterId, string ButhId,
         string NattsId, string EyeClean, string ShadeId, string TableInclusion, string CrownInclusion, string OIT, string OIC, string OIP, string OIG, string PolishFeatures,
         string SymFeatures, string ColorDescription, string ClarityDescription, string ClarityStatus, string Graining, string Painting, double PurchaseBack, double PurchaseRate,
         double PurchaseAmount, double ActualBack, double ActualRate, double ActualAmount, string MfgSeriesID, string MfgPacketID, string RoughId, string MinesId, DateTime PurchaseDate,
         int CompanyID, string ReportDate,string Gradername,string status)

        //DateTime PurchaseDate, DateTime RateReviseDate, DateTime NewArrivalDate, DateTime StoneLastUnHoldDate, 
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@ItemCode", ItemCode);
            param.Add("@itemsrno", ItemSrno);

            param.Add("@LotNo", LotNo);
            param.Add("@MfgSeriesID", MfgSeriesID);
            param.Add("@MfgPacketID", MfgPacketID);
            param.Add("@ECutCode", ShapeId);
            param.Add("@GraderName", Gradername);
            param.Add("@EColorCode", ColorId);
            param.Add("@EClarityCode", ClarityId);
            param.Add("@carats", Weight);
            param.Add("@rate", AskingRate);
            param.Add("@ActualRate", ActualRate);
            param.Add("@amount", ActualAmount);
            param.Add("@MRP", MRP);
            param.Add("@PMRPDown ", Back);
            param.Add("@ActualPRapDown ", Back);
            param.Add("@ESizeCode", SizeId);
            param.Add("@CertCode", CertId);
            param.Add("@PropCode", PropId);
            param.Add("@SymCode", SymId);
            param.Add("@PolishCode", PolishId);
            param.Add("@Diameter", Diameter);
            param.Add("@D", D);
            param.Add("@L", L);
            param.Add("@W", W);
            param.Add("@LW", LWRatio);
            param.Add("@TopDepth", TotDepth);
            param.Add("@Table", TablePer);
            param.Add("@CrownHeight", CrownHeight);
            param.Add("@CA", CrownAngle);
            param.Add("@GerdleMin", GirdleMin);
            param.Add("@GerdleMax", GirdleMax);
            param.Add("@GerdleMSMin", GirdleMinMS);
            param.Add("@GerdleMSMax", GirdleMaxMS);
            param.Add("@PavilionPer", PavilionDepth);
            param.Add("@PavilionHeight", PavilionHeight);
            param.Add("@Culet", CuletId);
            param.Add("@FLCode", FLId);
            param.Add("@MilkyCode", MilkyId);
            param.Add("@LusterCode", LusterId);
            param.Add("@Natts", NattsId);
            param.Add("@Brown", BrownId);
            param.Add("@Buth", ButhId);
            param.Add("@ClarityStatus", ClarityStatus);
            param.Add("@PolishFeatures", PolishFeatures);
            param.Add("@SymFeatures", SymFeatures);
            param.Add("@OIT", OIT);
            param.Add("@OIG", OIG);
            param.Add("@OIC", OIC);
            param.Add("@OIP", OIP);
            param.Add("@TableInclusion", TableInclusion);
            param.Add("@CrownInclusion", CrownInclusion);
            param.Add("@HACode", HAID);
            param.Add("@CertificateNo", CertificateNo);
            param.Add("@KeyToSymbols", KeyToSymbols);
            param.Add("@GDate",PurchaseDate);
            param.Add("@Status", status);
            param.Add("@UpdateFlag", false);
            param.Add("@Sync_Date", "");
            param.Add("@SinglePcs", true);
            param.Add("@FinalPurPrice", true);
            param.Add("@ReportDate", ReportDate);
            param.Add("@LaserInscriptionText", LaserInscriptionText);
            param.Add("@StrLn", StrLn);
            param.Add("@LrHalf", LrHalf);
            param.Add("@GerdlePer", GirdlePer);
            param.Add("@Comments", "");
            param.Add("@ControlNo", "");
            param.Add("@CertExpense", 0);
            param.Add("@Remarks", LABComments);
            param.Add("@Instruction", Instruction);
            param.Add("@SuratRemarks", "");
            param.Add("@IGICut", "");
            param.Add("@LABLocation", "");
            param.Add("@EyeClean", EyeClean);
            param.Add("@FlColor", FLColor);
            param.Add("@LABJobNo", "");
            param.Add("@LABMemoNo", "");
            param.Add("@LABPaint", "");
            param.Add("@LABPatintingComments", "");
            param.Add("@LABSyntheticIndecator", "");
            param.Add("@Painting", Painting);
            param.Add("@LABColorDesc", "");
            param.Add("@GirdleCondition", GirdleCondition);
            param.Add("@ReportAction", "");
            param.Add("@ServiceCode", "");
            param.Add("@RoughId", RoughId);
            param.Add("@ShapeName", ShapeId);
            param.Add("@Shade", ShadeId);
            param.Add("@VV", "");
            param.Add("@COl2 ", "");
            param.Add("@SugLAB", "");
            param.Add("@REstRate", "");
            param.Add("@REstCts", "");
            param.Add("@REstEColorCode", "");
            param.Add("@REstEClarityCode ", "");
            param.Add("@HRDCut", "");
            param.Add("@FancyColorOvertone", FancyColorOvertone);
            param.Add("@CuletCondition", CuletCondition);
            param.Add("@Green", GreenId);
            param.Add("@FancyColorIntensity", FancyColorIntensity);
            param.Add("@FancyColor", FancyColor);
            param.Add("@OP", "");
            param.Add("@SieveId", SieveId);
            param.Add("@ClarityDescription", ClarityDescription);
            param.Add("@ColorDescription", ColorDescription);
            param.Add("@SuratStoneID", "");
            param.Add("@Graining", Graining);
            param.Add("@CertResult", "");
            param.Add("@Measurement ", Measurement);
            param.Add("@TypeID", "");
            param.Add("@Black", "");
            param.Add("@CompanyID", CompanyID);
           
            #endregion

            return SaveDataInTransaction("SP_InsertMultiGrading", param);
        }


        public bool UpdateonsignmentDetails(DateTime SPdate, float Carats,string lotno)
        {
            #region Parameter List
            try
            {
                var param = new DynamicParameters();
                param.Add("@Trndate", SPdate);
                param.Add("@Carats", Carats);
                param.Add("@lotno", lotno);

                return SaveDataInTransaction("SP_UpdateSpcaratsforConsignment", param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion

        }


        public bool InsertSalePurTran(DateTime SPdate, string TrnType, string InvType, int InvCode, int ItemCode, string ItemName, float Carats, float Pcs, double Rate, double MRP,
            double PMRPDown, double ReturnCarats, double IssueCarats, double amount, string ExportNo, double Less1, double Less2, double Less3, string CurCode, float memotrancode, bool StockEffected,
            bool ExportDone, double spcarats, int FItemCode, int TItemCode, double FItemRate, double TItemRate, int FromSuratCode, int AssorterCode, int RoughCode,
            string MemoRemark, int UserCode, double MasterCurrencyRate, double MasterCurrencyAmount, double StockCurrencyRate, double StockCurrencyAmount, string CreatedBy, int CompanyID, [Optional] string PShape, [Optional] string PSize, [Optional] string PSieve, [Optional] string PColor, [Optional] string PClarity ,[Optional] float COGS, [Optional] string StockType, [Optional] string InvDescription)
        {
            #region Parameter List
            try
            {
                var param = new DynamicParameters();
                param.Add("@SPdate", SPdate);
                param.Add("@TrnType", TrnType);
                param.Add("@InvType", InvType);
                param.Add("@InvCode", InvCode);
                param.Add("@ItemCode", ItemCode);
                param.Add("@ItemName", ItemName);
                param.Add("@Carats", Carats);
                param.Add("@Pcs", Pcs);
                param.Add("@Rate", Rate);
                param.Add("@MRP", MRP);
                param.Add("@PMRPDown", PMRPDown);
                param.Add("@ReturnCarats", ReturnCarats);
                param.Add("@IssueCarats", IssueCarats);
                param.Add("@amount", amount);
                param.Add("@ExportNo", ExportNo);
                param.Add("@Less1", Less1);
                param.Add("@Less2", Less2);
                param.Add("@Less3", Less3);
                param.Add("@CurCode", CurCode);
                param.Add("@memotrancode", memotrancode);
                param.Add("@StockEffected", StockEffected);
                param.Add("@ExportDone", ExportDone);
                param.Add("@spcarats", spcarats);
                
                param.Add("@FItemCode", FItemCode);
                param.Add("@TItemCode", TItemCode);
                param.Add("@FItemRate", FItemRate);
                param.Add("@TItemRate", TItemRate);
                param.Add("@FromSuratCode", FromSuratCode);
                param.Add("@AssorterCode", AssorterCode);
                param.Add("@RoughCode", RoughCode);
                param.Add("@MemoRemark", MemoRemark);
                param.Add("@UserCode", UserCode);
                param.Add("@MasterCurrencyRate", MasterCurrencyRate);
                param.Add("@MasterCurrencyAmount", MasterCurrencyAmount);
                param.Add("@StockCurrencyRate", StockCurrencyRate);
                param.Add("@StockCurrencyAmount", StockCurrencyAmount);
                param.Add("@CreatedBy", CreatedBy);
                param.Add("@CompanyID", CompanyID);
                param.Add("@PShape", PShape);
                param.Add("@PSize", PSize);
                param.Add("@PSieve", PSieve);
                param.Add("@PColor", PColor);
                param.Add("@PClarity", PClarity);

                param.Add("@COGS", COGS);
                param.Add("@StockType", StockType);
                param.Add("@InvDescription", InvDescription);
                param.Add("@ActionType", "insert");
                return SaveDataInTransaction("SP_InsertSalePurTran", param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion

        }

        public bool UpdateSalePur(int Code, int Invno, string BillNo, string Trntype, string Invtype, string Category, DateTime Trndate, int Duedays, DateTime DueDate, long Partycode, long Sellercode, string term,
            long Countrycode, long? BrokerCode, float BrokerPer, double BrokerAmount, long? Adatcode, float Adatper, float AdatAmount, float TotCarats, double TotAmount, double NetAmount, double PlusMinAmount,
            double RoundAmount, double FinalAmount, double BaseRate, string Disc, string Curcode, bool outstanding, bool Stockeffected, bool Brokerage, bool Commission, bool Cheque, int usercode,
            string ModifyBy, string createdon, double mastercurruncyrate, double mastercurrencyfinalamount, double stockcurrencyrate, double stockcurrencyfinalamount, long exportpartycode, [Optional] DateTime RoundDate,
            [Optional] int DivideDays, [Optional] float interestper, [Optional] float vatper, [Optional] float vatamount, [Optional] string invdescription, [Optional] long bankname, [Optional] string subtype,  string shipvia,
            [Optional] string deliverto,  string tracking,  string saleperson,[Optional]string Attendee, [Optional] float labcharges, [Optional] float freightcharges, [Optional] int duedays1, [Optional] int duedays2,
            [Optional] float CGSTPer, [Optional] float CGSTAmt, [Optional] float SGSTPer, [Optional] float SGSTAmt, [Optional] float IGSTPer, [Optional] float IGSTAmt, [Optional] int Comapnyid, [Optional] double MCurNetAmount, [Optional] double MCurFinalPayRecAmount, [Optional] int BankID)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@Code", Code);
            param.Add("@invno", Invno);
            param.Add("@ModifyBy", ModifyBy);
            param.Add("@billno", BillNo);
            param.Add("@trntype", Trntype);
            param.Add("@invtype", Invtype);
            param.Add("@subtype", subtype);
            param.Add("@BankID", BankID);
            param.Add("@category", Category);
            param.Add("@trndate", Trndate);
            param.Add("@RoundDate", RoundDate);
            param.Add("@duedays", Duedays);
            param.Add("@duedate", DueDate);
            param.Add("@DivideDays", DivideDays);
            param.Add("@partycode", Partycode);
            param.Add("@SellerCode", Sellercode);
            param.Add("@countrycode", Countrycode);
            param.Add("@BrokerCode", BrokerCode);
            param.Add("@BrokerPer", BrokerPer);
            param.Add("@BrokerAmount", BrokerAmount);
            param.Add("@adatcode", Adatcode);
            param.Add("@adatper", Adatper);
            param.Add("@adatamount", AdatAmount);
            param.Add("@totcts", TotCarats);
            param.Add("@totamount", TotAmount);
            param.Add("@netamount", NetAmount);
            param.Add("@netMamount", MCurNetAmount);
            param.Add("@PlusMinAmount", PlusMinAmount);
            param.Add("@RoundAmount", RoundAmount);
            param.Add("@FinalAmount", FinalAmount);
            param.Add("@FinalPayRecAmount", MCurFinalPayRecAmount);
            param.Add("@BaseRate", BaseRate);
            param.Add("@ExpPartyCode", exportpartycode);
            param.Add("@ShipVia", shipvia);
            param.Add("@Tracking", tracking);
            param.Add("@SalesPerson", saleperson);
            param.Add("@Attendee", Attendee);
            param.Add("@Disc", Disc);
            param.Add("@CurCode", Curcode);
            param.Add("@Outstanding", outstanding);
            param.Add("@StockEffected", Stockeffected);
            param.Add("@Brokerage", Brokerage);
            param.Add("@Commission", Commission);
            param.Add("@Cheque", Cheque);
            param.Add("@usercode", usercode);
            param.Add("@term", term);
            param.Add("@InterestPer", interestper);
            param.Add("@mastercurrencyrate", mastercurruncyrate);
            param.Add("@mastercurrencyfinalamount", mastercurrencyfinalamount);
            param.Add("@stockcurrencyrate", stockcurrencyrate);
            param.Add("@stockcurrencyfinalamount", stockcurrencyfinalamount);

            param.Add("@VatPer", vatper);
            param.Add("@VatAmount", vatamount);
            param.Add("@CGSTPer", CGSTPer);
            param.Add("@CGSTAmount", CGSTAmt);
            param.Add("@SGSTPer", SGSTPer);
            param.Add("@SGSTAmount", SGSTAmt);
            param.Add("@IGSTPer", IGSTPer);
            param.Add("@IGSTAmount", IGSTAmt);
            param.Add("@CompanyID", Comapnyid);

            #endregion
            return SaveDataInTransaction("SP_UpdateSalePur", param);
        }

        public bool UpdateSalePurQuat(int Code, int Invno, string BillNo, string Trntype, string Invtype, string Category, DateTime Trndate, int Duedays, DateTime DueDate, long Partycode, long Sellercode, string term,
            long Countrycode, long? BrokerCode, float BrokerPer, double BrokerAmount, long? Adatcode, float Adatper, float AdatAmount, float TotCarats, double TotAmount, double NetAmount, double PlusMinAmount,
            double RoundAmount, double FinalAmount, double BaseRate, string Disc, string Curcode, bool outstanding, bool Stockeffected, bool Brokerage, bool Commission, bool Cheque, int usercode,
            string ModifyBy, string createdon, double mastercurruncyrate, double mastercurrencyfinalamount, double stockcurrencyrate, double stockcurrencyfinalamount, long exportpartycode, [Optional] DateTime RoundDate,
            [Optional] int DivideDays, [Optional] float interestper, [Optional] float vatper, [Optional] float vatamount, [Optional] string invdescription, [Optional] long bankname, [Optional] string subtype, string shipvia,
            [Optional] string deliverto, string tracking, string saleperson, [Optional] float labcharges, [Optional] float freightcharges, [Optional] int duedays1, [Optional] int duedays2,
            [Optional] float CGSTPer, [Optional] float CGSTAmt, [Optional] float SGSTPer, [Optional] float SGSTAmt, [Optional] float IGSTPer, [Optional] float IGSTAmt, [Optional] int Comapnyid, [Optional] double MCurNetAmount, [Optional] double MCurFinalPayRecAmount, [Optional] int BankID, [Optional] float GoldRate, [Optional] string DiamondCostType, [Optional] float LabourCost)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@Code", Code);
            param.Add("@invno", Invno);
            param.Add("@ModifyBy", ModifyBy);
            param.Add("@billno", BillNo);
            param.Add("@trntype", Trntype);
            param.Add("@invtype", Invtype);
            param.Add("@subtype", subtype);
            param.Add("@BankID", BankID);
            param.Add("@category", Category);
            param.Add("@trndate", Trndate);
            param.Add("@RoundDate", RoundDate);
            param.Add("@duedays", Duedays);
            param.Add("@duedate", DueDate);
            param.Add("@DivideDays", DivideDays);
            param.Add("@partycode", Partycode);
            param.Add("@sellercode", Sellercode);
            param.Add("@countrycode", Countrycode);
            param.Add("@BrokerCode", BrokerCode);
            param.Add("@BrokerPer", BrokerPer);
            param.Add("@BrokerAmount", BrokerAmount);
            param.Add("@adatcode", Adatcode);
            param.Add("@adatper", Adatper);
            param.Add("@adatamount", AdatAmount);
            param.Add("@totcts", TotCarats);
            param.Add("@totamount", TotAmount);
            param.Add("@netamount", NetAmount);
            param.Add("@netMamount", MCurNetAmount);
            param.Add("@PlusMinAmount", PlusMinAmount);
            param.Add("@RoundAmount", RoundAmount);
            param.Add("@FinalAmount", FinalAmount);
            param.Add("@FinalPayRecAmount", MCurFinalPayRecAmount);
            param.Add("@BaseRate", BaseRate);
            param.Add("@ExpPartyCode", exportpartycode);
            param.Add("@ShipVia", shipvia);
            param.Add("@Tracking", tracking);
            param.Add("@SalesPerson", saleperson);
            param.Add("@Disc", Disc);
            param.Add("@CurCode", Curcode);
            param.Add("@Outstanding", outstanding);
            param.Add("@StockEffected", Stockeffected);
            param.Add("@Brokerage", Brokerage);
            param.Add("@Commission", Commission);
            param.Add("@Cheque", Cheque);
            param.Add("@usercode", usercode);
            param.Add("@term", term);
            param.Add("@InterestPer", interestper);
            param.Add("@mastercurrencyrate", mastercurruncyrate);
            param.Add("@mastercurrencyfinalamount", mastercurrencyfinalamount);
            param.Add("@stockcurrencyrate", stockcurrencyrate);
            param.Add("@stockcurrencyfinalamount", stockcurrencyfinalamount);

            param.Add("@VatPer", vatper);
            param.Add("@VatAmount", vatamount);
            param.Add("@CGSTPer", CGSTPer);
            param.Add("@CGSTAmount", CGSTAmt);
            param.Add("@SGSTPer", SGSTPer);
            param.Add("@SGSTAmount", SGSTAmt);
            param.Add("@IGSTPer", IGSTPer);
            param.Add("@IGSTAmount", IGSTAmt);
            param.Add("@CompanyID", Comapnyid);
            param.Add("@GoldRate", GoldRate);
            param.Add("@DiamondCostType", DiamondCostType);
            param.Add("@LabourCost", LabourCost);

            #endregion
            return SaveDataInTransaction("SP_UpdateSalePurQuat", param);
        }

        public bool Insertjv(DateTime JVdate, DateTime duedate, long dacccode, long cacccode, float amount, string CurCode, string vtype, int vcode, string disc, string Adjustvtype, int Adjustvcode,
            double BaseRate, double MasterCurrencyRate, double MasterCurrencyAmount, double StockCurrencyRate, double StockCurrencyAmount, double Gcode, string VouNo, double ChequeNo, int UserCode,
            string CreatedBy,  [Optional] string ComapnyId,[Optional] long AgainstAccCode)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@dacccode", dacccode);
            param.Add("@cacccode", cacccode);
            param.Add("@amount", amount);
            param.Add("@JVdate", JVdate);
            param.Add("@duedate", duedate);
            param.Add("@curcode", CurCode);
            param.Add("@vtype", vtype);
            param.Add("@vcode", vcode);
            param.Add("@disc", disc);
            param.Add("@Adjustvtype", Adjustvcode);
            param.Add("@Adjustvcode", Adjustvcode);
            param.Add("@BaseRate", BaseRate);
            param.Add("@MasterCurrencyRate", BaseRate);
            param.Add("@MasterCurrencyAmount", MasterCurrencyAmount);
            param.Add("@StockCurrencyRate", StockCurrencyRate);
            param.Add("@StockCurrencyAmount", StockCurrencyAmount);
            param.Add("@GCode", Gcode);
            param.Add("@UserCode", UserCode);
            param.Add("@CreatedBy", CreatedBy);
            param.Add("@Companyid", ComapnyId);
            param.Add("@AgainstAccCode", AgainstAccCode);
            param.Add("@ActionType", "insert");
            #endregion
            return SaveDataInTransaction("SP_insertjv", param);
        }

        public bool Updatejv(DateTime JVdate, DateTime duedate, long dacccode, long cacccode, float amount, string CurCode, string vtype, int vcode, string disc, string Adjustvtype, int Adjustvcode,
            double BaseRate, double MasterCurrencyRate, double MasterCurrencyAmount, double StockCurrencyRate, double StockCurrencyAmount, double Gcode, string VouNo, double ChequeNo, int UserCode,
            string CreatedBy, [Optional] string ComapnyId, [Optional] long AgainstAccCode)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@dacccode", dacccode);
            param.Add("@cacccode", cacccode);
            param.Add("@amount", amount);
            param.Add("@JVdate", JVdate);
            param.Add("@duedate", duedate);
            param.Add("@curcode", CurCode);
            param.Add("@vtype", vtype);
            param.Add("@vcode", vcode);
            param.Add("@disc", disc);
            param.Add("@Adjustvtype", Adjustvcode);
            param.Add("@Adjustvcode", Adjustvcode);
            param.Add("@BaseRate", BaseRate);
            param.Add("@MasterCurrencyRate", BaseRate);
            param.Add("@MasterCurrencyAmount", MasterCurrencyAmount);
            param.Add("@StockCurrencyRate", StockCurrencyRate);
            param.Add("@StockCurrencyAmount", StockCurrencyAmount);
            param.Add("@GCode", Gcode);
            param.Add("@UserCode", UserCode);
            param.Add("@CreatedBy", CreatedBy);
            param.Add("@Companyid", ComapnyId);
            param.Add("@AgainstAccCode", AgainstAccCode);
            param.Add("@ActionType", "update");
            #endregion
            return SaveDataInTransaction("SP_insertjv", param);
        }


        public bool InsertPayRec(DateTime date, long invcode, long Jvcode, long PostCheque, double amount, string curcode, double BaseRate, string disc, double MasterCurrencyRate, double MasterCurrencyAmount, double StockCurrencyRate, double StockCurrencyAmount, int UserCode,string CreatedBy, [Optional] string ComapnyId)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@Date", date);
            param.Add("@InvCode", invcode);
            param.Add("@JVCode", Jvcode);
            param.Add("@PostCheque", PostCheque);
            param.Add("@Amount", amount);
            param.Add("@CurCode", curcode);
            param.Add("@BaseRate", BaseRate);
            param.Add("@Disc", disc);
            param.Add("@MasterCurrencyRate", MasterCurrencyRate);
            param.Add("@MasterCurrencyAmount", MasterCurrencyAmount);
            param.Add("@StockCurrencyRate", StockCurrencyRate);
            param.Add("@StockCurrencyAmount", StockCurrencyAmount);
            param.Add("@UserCode", UserCode);
            param.Add("@CreatedBy", CreatedBy);
            param.Add("@Companyid", ComapnyId);
           
            #endregion
            return SaveDataInTransaction("SP_InsertPayRec", param);
        }

        public bool Updateitem(int InvCode)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@InvCode", InvCode);
           
            #endregion
            return SaveDataInTransaction("SP_UpdateItemMasterAtEditPrachase", param);
        }

        public bool DeleteJV(int vcode, string vtype)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@vcode", vcode);
            param.Add("@vtype", vtype);

            #endregion

            bool result = SaveDataInTransaction("SP_DeleteJV", param);
            return result;
        }

        public bool DeleteJVOnlyCode(int code)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@code", code);
          
            #endregion

            bool result = SaveDataInTransaction("SP_DeleteJV", param);
            return result;
        }

        public bool DeletePayRec(int code)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@JVcode", code);

            #endregion

            bool result = SaveDataInTransaction("SP_DeletePayRec", param);
            return result;
        }
        public bool DeleteStktrn(int code)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@code", code);

            #endregion

            return SaveDataInTransaction("SP_DeleteStkTran", param);
        }

        public bool UpdateMemo(float code, DateTime Date,float rtncts, string type)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@code", code);
            param.Add("@RtnDate", Date);
            param.Add("@rtncts", rtncts);
            param.Add("@Actiontype", type);
            #endregion

            bool result = SaveDataInTransaction("SP_UpdateMemo", param);
            return result;
        }

        public bool holdunholdrequest(string Where, string actiontype)
        {
            #region Parameter List
            var param = new DynamicParameters();

            param.Add("@Where", Where);
            param.Add("@actiontype", actiontype);

            #endregion

            bool result = SaveDataInTransaction("TestingGetItemsByNotShow", param);
            return result;
        }
        public bool UpdateMemoForSale(float code, DateTime Date, float spcts, float rtncts, string type)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@code", code);
            param.Add("@RtnDate", Date);
            param.Add("@rtncts", rtncts);
            param.Add("@spcts", spcts);
            param.Add("@Actiontype", type);
            #endregion

            bool result = SaveDataInTransaction("SP_UpdateMemo", param);
            return result;
        }

        public bool DeleteSalePurTran(int Invcode)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@InvCode", Invcode);
            
            #endregion

            bool result = SaveDataInTransaction("SP_DeleteSalePurTran", param);
            return result;
        }

        public bool DeleteIncomeExpense(int GCode)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@GCode", GCode);

            #endregion

            bool result = SaveDataInTransaction("SP_DeleteIncomeExpense", param);
            return result;
        }

        public bool DeleteSalePur(int code)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@code", code);

            #endregion

            return SaveDataInTransaction("SP_DeleteSalePur", param);
        }
     

        public string GetMaxInvNo(string Where)
        {
            //string sql = @" SELECT Max(InvNo) FROM TabSalePur WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='Inv',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }


        public string GetMaxInvNoMemoRtn(string Where)
        {
            // string sql = @" SELECT max(invcode) FROM TabSalePurTran WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='InvCode',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetMaxGcode(string Where)
        {
           // string sql = @" Select Max(ISNUll(GCode,0)) From TabJV WHERE 1=1 " + Where;


            string sql = @" EXEC [GetAllCommonData] @Type='Gcode',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetVouNo(string Where)
        {
           // string sql = @" Select max (VouNo ) from TabJV WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='VouNo',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }


        public string GetParty(string Where)
        {
            //string sql = @" Select AccGrpCode From TabAccGrpMaster WHERE 1=1 " + Where;
            string sql = @" EXEC [GetAllCommonData] @Type='Party',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }
        public string GetMaxItemSrNo()
        {
            //string sql = @" Select isnull(MAX(ItemSrno),0) from TabItemMaster Where itemsrno between 0 and 999999 ";
            string sql = @" EXEC [GetAllCommonData] @Type='MaxItemSrNo'";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }
      
        public string GetMaxItemCode(string Where)
        {
            //string sql = @" SELECT Max(ItemCode) FROM TabItemMaster WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='ItemCode',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).FirstOrDefault();
        }

        public string GetMaxItemSrnum(string Where)
        {           
            string sql = @" EXEC [GetAllCommonData] @Type='ItemSrnum',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }


        public string GetMaxSPCode(string Where)
        {
            //string sql = @" SELECT Max(Code) FROM TabSalePur WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='SPCode',@Where = '" + Where + "' ";
             return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetMaxJVCode(string Where)
        {
            //string sql = @" SELECT Max(Code) FROM TabJv WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='JVCode',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }
        public string GetMCurOperator(string Where)
        {
            //string sql = @" SELECT MasterCurrencyOperator FROM TabCurrencyMaster WHERE 1=1 " + Where;

            string sql = @" EXEC [GetCurrencyDetail] @Type='MCurOperator',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }
        public string GetSCurOperator(string Where)
        {
          //  string sql = @" SELECT StockCurrencyOperator FROM TabCurrencyMaster WHERE 1=1 " + Where;

            string sql = @" EXEC [GetCurrencyDetail] @Type='SCurOperator',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }
        public double GetScurBaseRate(string where)
        {

          //  string sql = @" SELECT StockCurrencyRate FROM TabCurrencyMaster WHERE 1=1 and id=" + where;

            string sql = @" EXEC [GetCurrencyDetail] @Type='ScurBaseRate',@Where = '" + where + "' ";
            return _connection.Query<double>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }
        public long GetMCurID()
        {
           // string sql = @" SELECT ID FROM TabCurrencyMaster WHERE 1=1 and MasterCurrencyRate=1 ";

            string sql = @" EXEC [GetCurrencyDetail] @Type='MCurID'";
            return _connection.Query<long>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }
        public long GetSCurID()
        {
            // string sql = @" SELECT ID FROM TabCurrencyMaster WHERE 1=1 and StockCurrencyRate=1 ";

            string sql = @" EXEC [GetCurrencyDetail] @Type='SCurID'";
            return _connection.Query<long>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }
        public string GetAccountCode(string Where)
        {
            //string sql = @" SELECT AccGrpCode FROM TabAccGrpMaster WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='Party',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).FirstOrDefault();
        }
        public string GetLOTNO(string Where)
        {
            //string sql = @" SELECT AccGrpCode FROM TabAccGrpMaster WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='LotNo',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetShipVia(string Where)
        {            
            string sql = @" EXEC [GetAllCommonData] @Type='ShipVia',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetAccountTypeCode(string Where)
        {
            //string sql = @" SELECT AccGrpCode FROM TabAccGrpMaster WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='AccountType',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetAccountLevel(string Where)
        {
            //string sql = @" SELECT AccGrpCode FROM TabAccGrpMaster WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='AccountLevel',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetLotNo(string Where)
        {            

            string sql = @" EXEC [GetAllCommonData] @Type='LotNo',@Where = '" + Where + "' ";

            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).FirstOrDefault();
        }

        
        private readonly string _connectionString;

        public SqlTempAccess(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool isClosed = false;
        private const int CommandTimeoutSeconds = 180;


        public SqlConnection _connections;
        public void Connection() {
             
        string connectionString = _connectionString;
        _connections = new SqlConnection(connectionString);
        _connections.Open();            
        }


        public void StartTransaction()
        {
            string connectionString = _connectionString;
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();

            isClosed = false;
        }

        public bool SaveDataInTransaction(string sp_name, DynamicParameters parameter)
        {
            var result = _connection.Execute(sp_name, parameter, commandType: CommandType.StoredProcedure, transaction: _transaction, commandTimeout: CommandTimeoutSeconds);

            if (result > 0)
                return true;
            else
                return false;
        }

        public long GetJewCode(string where)
        {
            string sql = @" EXEC [GetALLMasterList] @Type='GetJewCode',@Where = '" + where + "' ";
            return _connection.Query<long>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).FirstOrDefault();
        }

        public bool JewStockExists(string where)
        {
            string sql = @" EXEC [GetALLMasterList] @Type='JewStock', @Where = '" + where + "' ";
            return _connection.Query(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).Any();
        }

        //public bool UpdatePayRecByCodes(long JVCode, long InvCode, double Amount, string CurCode, double BaseRate, string Disc, double MasterCurrencyAmount, double StockCurrencyRate, double StockCurrencyAmount)
        //{
        //    var param = new DynamicParameters();
        //    param.Add("@JVCode", JVCode);
        //    param.Add("@InvCode", InvCode);
        //    param.Add("@Amount", Amount);
        //    param.Add("@CurCode", CurCode);
        //    param.Add("@BaseRate", BaseRate);
        //    param.Add("@Disc", Disc);
        //    param.Add("@MasterCurrencyAmount", MasterCurrencyAmount);
        //    param.Add("@StockCurrencyRate", StockCurrencyRate);
        //    param.Add("@StockCurrencyAmount", StockCurrencyAmount);
        //    return SaveDataInTransaction("SP_UpdatePayRecByCodes", param);
        //}

        public string GetStringData(string sql)
        {
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).FirstOrDefault();
        }

        public void CommitTransaction()
        {
            _transaction?.Commit();
            _connection?.Close();

            isClosed = true;
        }

        public void RollbackTransaction()
        {
            try
            {
                _transaction?.Rollback();
            }
            catch { }

            try
            {
                _connection?.Close();
            }
            catch { }

            isClosed = true;
        }

        public void Dispose()
        {
            if (isClosed == false)
            {
                try
                {
                    CommitTransaction();
                }
                catch (Exception)
                {

                }
            }
            _transaction = null;
            _connection = null;
            //throw new NotImplementedException();
        }

        public bool UpdatePartyHold(float code, DateTime Date, float rtncts, string type)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@code", code);
            param.Add("@RtnDate", Date);
            param.Add("@rtncts", rtncts);
            param.Add("@Actiontype", type);
            #endregion

            bool result = SaveDataInTransaction("SP_UpdatePartyhold", param);
            return result;
        }

        public bool SaveJewDesignMasterData(int id, string DesignId, string DesignName, float DesignPriority, string CreatedBy, string ModifyBy, int CompanyID, string category, string subcategory, int jewsize, string metaltype, string color, string purity, double pureratio, double grossweight, double netweight, double metalpricegm,
           string makingtype, float makingrate, float totalmakingcharges, float totalsettingcharges, float handlingchargespercarat, float handlingcarat, float totalhandlingcharges, string jewcertificate, float certificatecharges, float othercharges, float hallmarkcharges,
           double totmetalamount, double totdiamondamt, double totdiamondcarat, float totstonecarat, float totstoneamount, float totmakingcharges, string Image, string Video, string Cert, double finalamount, int diamondqty, int stoneqty, string description, string certificateno, string ActionType)
        {
            var param = new DynamicParameters();
            param.Add("@id", id);
            param.Add("@DesignId", DesignId);
            param.Add("@DesignName", DesignName);
            param.Add("@DesignPriority", DesignPriority);
            param.Add("@CreatedBy", CreatedBy);
            param.Add("@ModifyBy", ModifyBy);
            param.Add("@CompanyID", CompanyID);
            param.Add("@ActionType", ActionType);
            param.Add("@Category", category);
            param.Add("@SubCategory", subcategory);
            param.Add("@JewSize", jewsize);
            param.Add("@MetalType", metaltype);
            param.Add("@Color", color);
            param.Add("@Purity", purity);
            param.Add("@PureRatio", pureratio);
            param.Add("@GrossWeight", grossweight);
            param.Add("@NetWeight", netweight);
            param.Add("@MetalPriceGm", metalpricegm);
            param.Add("@MakingType", makingtype);
            param.Add("@MakingRate", makingrate);
            param.Add("@MakingCharges", totalmakingcharges);
            param.Add("@TotalSettingCharges", totalsettingcharges);
            param.Add("@HandlingChargesPerCarat", handlingchargespercarat);
            param.Add("@HandlingCarat", handlingcarat);
            param.Add("@TotalHandlingCharges", totalhandlingcharges);
            param.Add("@JewCertificate", jewcertificate);
            param.Add("@CertificateCharges", certificatecharges);
            param.Add("@OtherCharges", othercharges);
            param.Add("@HallmarkCharges", hallmarkcharges);
            param.Add("@Totmetalamount", totmetalamount);
            param.Add("@TotDiamondamt", totdiamondamt);
            param.Add("@TotDiamondCarat", totdiamondcarat);
            param.Add("@TotStoneCarat", totstonecarat);
            param.Add("@TotStoneAmount", totstoneamount);
            param.Add("@TotMakingCharges", totmakingcharges);
            //param.Add("@Totamount", totamount);
            //param.Add("@cgstper", cgstper);
            param.Add("@ImagePath", Image);
            param.Add("@VideoPath", Video);
            param.Add("@CertPath", Cert);
            param.Add("@finalamount", finalamount);

            param.Add("@DiamondQty", diamondqty);
            param.Add("@StoneQty", stoneqty);
            param.Add("@Description", description);
            param.Add("@JewelleryCerNo", certificateno);
            return SaveDataInTransaction("SP_JewDesignMaster", param);
        }

        public string GetMaxJewCode(string Where)
        {
            //string sql = @" SELECT Max(Code) FROM TabSalePur WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='JewCode',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public bool SaveDiamondDetailsData(string designid, int jewcode, int diamonditemcode, string stockid, string type, string shape, string size, string sieve, string color, string clarity, int pcs, double carat, float price, double amount, string lab, string ActionType, int CompanyID)
        {
            var param = new DynamicParameters();
            param.Add("@designid", designid);
            param.Add("@jewcode", jewcode);
            param.Add("@diamonditemcode", diamonditemcode);
            param.Add("@stockid", stockid);
            param.Add("@type", type);
            param.Add("@shape", shape);
            param.Add("@size", size);
            param.Add("@sieve", sieve);
            param.Add("@color", color);
            param.Add("@clarity", clarity);
            param.Add("@pcs", pcs);
            param.Add("@carat", carat);
            param.Add("@price", price);
            param.Add("@amount", amount);
            param.Add("@lab", lab);
            param.Add("@ActionType", ActionType);
            param.Add("@CompanyID", CompanyID);
            return SaveDataInTransaction("SP_DiamondDetails", param);
        }

        public bool SaveSettingDetailsData(string designid, int jewcode, string type, string shape, string size, int pcs, string settingtype, float price, float amount, string ActionType, int CompanyID)
        {
            var param = new DynamicParameters();

            param.Add("@designid", designid);
            param.Add("@jewcode", jewcode);
            param.Add("@type", type);
            param.Add("@shape", shape);
            param.Add("@size", size);
            param.Add("@pcs", pcs);
            param.Add("@settingtype", settingtype);
            param.Add("@price", price);
            param.Add("@amount", amount);
            param.Add("@ActionType", ActionType);
            param.Add("@CompanyID", CompanyID);

            return SaveDataInTransaction("SP_SettingDetails", param);
        }

        public bool DeleteJewDesign(int id, string actiontype)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@ID", id);
            param.Add("@ActionType", actiontype);

            #endregion

            bool result = SaveDataInTransaction("SP_DeleteJewDesign", param);
            return result;
        }

        public bool Deleteboth(int id, string actiontype)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@ID", id);
            param.Add("@ActionType", actiontype);

            #endregion

            bool result = SaveDataInTransaction("SP_DeleteJewDesign", param);
            return result;
        }

        public string GetDesignName(string Where)
        {
            //string sql = @" SELECT Max(Code) FROM TabSalePur WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='GetDesignName',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).FirstOrDefault();
        }

        public int GetMaxStockSerialNo(string Where)
        {
            //string sql = @" SELECT Max(Code) FROM TabSalePur WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='MaxStockSerianNo',@Where = '" + Where + "' ";
            return _connection.Query<int>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetMaxStockCode(string Where)
        {
            //string sql = @" SELECT Max(Code) FROM TabSalePur WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='StockCode',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetMaxJewStockCode(string Where)
        {
            //string sql = @" SELECT Max(ItemCode) FROM TabItemMaster WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='StockCode',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).First();
        }

        public string GetCheckTrntype(string Where)
        {
            //string sql = @" SELECT Max(Code) FROM TabSalePur WHERE 1=1 " + Where;

            string sql = @" EXEC [GetAllCommonData] @Type='CheckTrnTypeAndbillNo',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).FirstOrDefault();
        }

        public bool SaveStockDiamondDetailsData(string designid, int jewcode, int diamondtemcode, string stockid, string type, string shape, string size, string sieve, string color, string clarity, int pcs, double carat, float price, double amount, string lab, string ActionType, int CompanyID)
        {
            var param = new DynamicParameters();
            param.Add("@designid", designid);
            param.Add("@jewcode", jewcode);
            param.Add("@diamonditemcode", diamondtemcode);
            param.Add("@stockid", stockid);
            param.Add("@type", type);
            param.Add("@shape", shape);
            param.Add("@size", size);
            param.Add("@sieve", sieve);
            param.Add("@color", color);
            param.Add("@clarity", clarity);
            param.Add("@pcs", pcs);
            param.Add("@carat", carat);
            param.Add("@price", price);
            param.Add("@amount", amount);
            param.Add("@lab", lab);
            param.Add("@ActionType", ActionType);
            param.Add("@CompanyID", CompanyID);
            return SaveDataInTransaction("SP_DiamondDetails", param);
        }

        public bool SaveStockSettingDetailsData(string designid, int jewcode, string type, string shape, string size, int pcs, string settingtype, float price, float amount, string ActionType, int CompanyID)
        {
            var param = new DynamicParameters();

            param.Add("@designid", designid);
            param.Add("@jewcode", jewcode);
            param.Add("@type", type);
            param.Add("@shape", shape);
            param.Add("@size", size);
            param.Add("@pcs", pcs);
            param.Add("@settingtype", settingtype);
            param.Add("@price", price);
            param.Add("@amount", amount);
            param.Add("@ActionType", ActionType);
            param.Add("@CompanyID", CompanyID);

            return SaveDataInTransaction("SP_SettingDetails", param);
        }

        public bool SaveJewStockMasterData(int id, string Trndate, string stockid, int serialno, string DesignId, string DesignName, float DesignPriority, string CreatedBy, string ModifyBy, int CompanyID, string category, string subcategory, string jewsize, string JewCertificateNo, string metaltype, string color, string purity, double pureratio, double grossweight, double netweight, double metalpricegm,
           string makingtype, float makingrate, float totalmakingcharges, float totalsettingcharges, float handlingchargespercarat, float handlingcarat, float totalhandlingcharges, string jewcertificate, float certificatecharges, float othercharges, float hallmarkcharges,
           double totmetalamount, float totdiamondamt, float totdiamondcarat, float totstonecarat, float totstoneamount, float totmakingcharges, string Image, byte[] ImageBinary, string Video, string Cert, double finalamount, float askingrate, float actualrate, float purchaserate, float bottomrate, float b2brate, float b2crate, int totaldiamondqty, int totalstoneqty, double pcs, string Description,  [Optional] string MountCategory, [Optional] string UseMounting, double costamount, string ActionType)
        {
            var param = new DynamicParameters();
            param.Add("@id", id);
            param.Add("@Trndate", Trndate);
            param.Add("@StockID", stockid);
            param.Add("@StockSerialNo", serialno);
            param.Add("@DesignId", DesignId);
            param.Add("@DesignName", DesignName);
            param.Add("@DesignPriority", DesignPriority);
            param.Add("@CreatedBy", CreatedBy);
            param.Add("@ModifyBy", ModifyBy);
            param.Add("@CompanyID", CompanyID);
            param.Add("@ActionType", ActionType);
            param.Add("@Category", category);
            param.Add("@SubCategory", subcategory);
            param.Add("@JewSize", jewsize);
            param.Add("@JewCerNo", JewCertificateNo);
            param.Add("@MetalType", metaltype);
            param.Add("@Color", color);
            param.Add("@Purity", purity);
            param.Add("@PureRatio", pureratio);
            param.Add("@GrossWeight", grossweight);
            param.Add("@NetWeight", netweight);
            param.Add("@MetalPriceGm", metalpricegm);
            param.Add("@MakingType", makingtype);
            param.Add("@MakingRate", makingrate);
            param.Add("@MakingCharges", totalmakingcharges);
            param.Add("@TotalSettingCharges", totalsettingcharges);
            param.Add("@HandlingChargesPerCarat", handlingchargespercarat);
            param.Add("@HandlingCarat", handlingcarat);
            param.Add("@TotalHandlingCharges", totalhandlingcharges);
            param.Add("@JewCertificate", jewcertificate);
            param.Add("@CertificateCharges", certificatecharges);
            param.Add("@OtherCharges", othercharges);
            param.Add("@HallmarkCharges", hallmarkcharges);
            param.Add("@Totmetalamount", totmetalamount);
            param.Add("@TotDiamondamt", totdiamondamt);
            param.Add("@TotDiamondCarat", totdiamondcarat);
            param.Add("@TotStoneCarat", totstonecarat);
            param.Add("@TotStoneAmount", totstoneamount);
            param.Add("@TotMakingCharges", totmakingcharges);
            //param.Add("@Totamount", totamount);
            //param.Add("@cgstper", cgstper);
            param.Add("@ImagePath", Image);
            param.Add("@ImageBinary", ImageBinary);
            param.Add("@VideoPath", Video);
            param.Add("@CertPath", Cert);
            param.Add("@finalamount", finalamount);
            param.Add("@AskingRate", askingrate);
            param.Add("@Pcs", pcs);
            param.Add("@ActualRate", actualrate);
            param.Add("@PurchaseRate", purchaserate);
            param.Add("@BottomRate", bottomrate);
            param.Add("@B2BRate", b2brate);
            param.Add("@B2CRate", b2crate);
            param.Add("@TotalDiamondQty", totaldiamondqty);
            param.Add("@TotalStoneQty", totalstoneqty);
            param.Add("@Description", Description);
            param.Add("@MountCategory", MountCategory);            
            param.Add("@UseMounting", UseMounting);
            param.Add("@costamount", costamount);

            return SaveDataInTransaction("SP_JewStockMaster", param);
        }

        public bool UpdateMemo1(float code, DateTime Date, float rtnPcs, string type)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@code", code);
            param.Add("@RtnDate", Date);
            param.Add("@sppcs", rtnPcs);
            param.Add("@Actiontype", type);
            #endregion

            bool result = SaveDataInTransaction("SP_UpdateJewMemo", param);
            return result;
        }

        public bool UpdateJewMemoIn(float code, DateTime Date, float Pcs, string type)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@code", code);
            param.Add("@RtnDate", Date);
            param.Add("@sppcs", Pcs);
            param.Add("@Actiontype", type);
            #endregion

            bool result = SaveDataInTransaction("SP_UpdateJewMemoIn", param);
            return result;
        }


        public bool UpdatePayRecByCodes(DateTime date, long invcode, long Jvcode, long PostCheque, double amount, string curcode, double BaseRate, string disc, double MasterCurrencyRate, double MasterCurrencyAmount, double StockCurrencyRate, double StockCurrencyAmount, int UserCode, string CreatedBy, [Optional] string ComapnyId)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@Date", date);
            param.Add("@InvCode", invcode);
            param.Add("@JVCode", Jvcode);
            param.Add("@PostCheque", PostCheque);
            param.Add("@Amount", amount);
            param.Add("@CurCode", curcode);
            param.Add("@BaseRate", BaseRate);
            param.Add("@Disc", disc);
            param.Add("@MasterCurrencyRate", MasterCurrencyRate);
            param.Add("@MasterCurrencyAmount", MasterCurrencyAmount);
            param.Add("@StockCurrencyRate", StockCurrencyRate);
            param.Add("@StockCurrencyAmount", StockCurrencyAmount);
            param.Add("@UserCode", UserCode);
            param.Add("@CreatedBy", CreatedBy);
            param.Add("@Companyid", ComapnyId);


            #endregion
            return SaveDataInTransaction("SP_UpdatePayRecByCodes", param);
        }



        public bool ResetMemoStatus(float invCode, string trnType = "PMemoOut")
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@InvCode", invCode);
            param.Add("@TrnType", trnType);
            param.Add("@ActionType", "RESET"); // custom action
            #endregion

            bool result = SaveDataInTransaction("SP_UpdateMemoStatus", param);
            return result;
        }


        public bool UpdateSPcaratsusingmemotrancode(float memoTranCode)
        {
            #region Parameter List
            var param = new DynamicParameters();
            param.Add("@MemoTranCode", memoTranCode);            

            #endregion

            return SaveDataInTransaction("SP_UpdateSpCaratsUsingMemoTranCode", param);
        }

        public string GetMountingName(string Where)
        {
            //string sql = @" SELECT Max(Code) FROM TabSalePur WHERE 1=1 " + Where;

            string sql = @" EXEC [GetALLMasterList] @Type='GetMountingName',@Where = '" + Where + "' ";
            return _connection.Query<string>(sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).FirstOrDefault();
        }

        /// <summary>MVC SqlTempAccess.GetLotJewData — GetLotNoDataForAll @Type='GetJewStockDetails'.</summary>
        public List<Dia_CoreAPI.DTOs.StockAccount.LotNoDataJew> GetLotJewData(string strWhere)
        {
            string sql = @" EXEC [GetLotNoDataForAll] @Type = 'GetJewStockDetails',@Where = '" + strWhere + "'";
            return _connection.Query<Dia_CoreAPI.DTOs.StockAccount.LotNoDataJew>(
                sql, transaction: _transaction, commandTimeout: CommandTimeoutSeconds).ToList();
        }

    }
}
