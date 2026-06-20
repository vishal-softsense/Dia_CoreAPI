using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction
{
    public class PurchaseParcelRepository : IPurchaseParcelRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public PurchaseParcelRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
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

            var result = await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }

        public async Task<List<InvList>> GetPurchaseParcelListAsync(int pageNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@PageNo", pageNo }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<InvList>(
                "GetPurchaseParcelInvDataUsingPageNo",
                parameters,
                companyId);

            return result.ToList();
        }

        public async Task<PurchaseParcelPageData> GetPurchaseParcelPageDataAsync(string companyId)
        {
            PurchaseParcelPageData result = new PurchaseParcelPageData();

            List<BasicData> data = await ItemMasterDropDownAsync(companyId);

            result.Shape = data.Where(x => x.Type == "Shape").ToList();
            result.Color = data.Where(x => x.Type == "Color").ToList();
            result.Size = data.Where(x => x.Type == "Size").ToList();
            result.Clarity = data.Where(x => x.Type == "Clarity").ToList();
            result.Sieve = data.Where(x => x.Type == "Sieve").ToList();
            result.ProductTypeList = data.Where(x => x.Type == "ProductType").ToList();
            result.ItemGroup = data.Where(x => x.Type == "ItemGroup").ToList();

            var transactionData = await TransactionDropDownAsync(companyId);

            result.Type = transactionData.Where(x => x.Type == "Type").ToList();
            result.SubType = transactionData.Where(x => x.Type == "SubType").ToList();
            result.Category = transactionData.Where(x => x.Type == "Category").ToList();
            result.CurrencyList = transactionData.Where(x => x.Type == "Currency").ToList();

            result.BrokerList = new List<BasicData>()
            {
                new BasicData
                {
                    ID = "",
                    Name = "Select Broker"
                }
            };

            return result;
        }

        public async Task<List<BasicData>> ItemMasterDropDownAsync(string companyId)
        {
            var parameters = new Dictionary<string, object>();

            var result = await _sqlDataAccess.SelectDataFromSP<BasicData>(
                "GetAllMasterDataForItemMaster",
                parameters,
                companyId);

            return result.ToList();
        }

        public async Task<List<BasicData>> TransactionDropDownAsync(string companyId)
        {
            var parameters = new Dictionary<string, object>();

            var result = await _sqlDataAccess.SelectDataFromSP<BasicData>(
                "AllMasterDataForPurchase",
                parameters,
                companyId);

            return result.ToList();
        }

        public async Task<PurchaseParcelGridView> AddItemInGridAsync(
            AddSingleLotParcel item,
            List<ImportExcelParcel> parcelList,
            string companyId)
        {
            double totalCarats = 0;
            double totalAmount = 0;
            double totalMRPAmount = 0;
            double totalAmountWithoutless = 0;

            string alert = "";
            string caratFormat = "0.000";
            float tempCarat = item.Carat;

            string carat3D = Convert.ToDouble(tempCarat).ToString("N3");
            string carat2D = carat3D.Substring(0, carat3D.Length - 1);

            float carat = caratFormat == "0.000"
                ? float.Parse(carat3D)
                : float.Parse(carat2D);

            float purRate = item.Rate;
            float back = item.Back;
            float issueCarats = item.IssueCarats;

            if (issueCarats <= 0)
                throw new Exception("Carat is invalid.");

            if (purRate == 0 && back == 0)
                throw new Exception("Rate and Discount both can not be 0.");

            if (purRate != 0 && issueCarats != 0)
            {
                back = (((purRate * 100) / issueCarats) - 100);
            }

            float amount = issueCarats * purRate;

            float l1 = item.Less1 != 0
                ? amount * (100 + (float)item.Less1) / 100
                : amount;

            float l2 = item.Less2 != 0
                ? l1 * (100 + (float)item.Less2) / 100
                : l1;

            float l3 = item.Less3 != 0
                ? l2 * (100 + (float)item.Less3) / 100
                : l2;

            ImportExcelParcel pur = new ImportExcelParcel
            {
                ItemName = item.ItemName,
                LotNo = item.LotNo,
                ExportNo = item.ExportNo,

                PShape = item.ShapeId,
                PColor = item.ColorId,
                PClarity = item.ClarityId,
                PSize = item.SizeId,
                PSieve = item.SieveId,

                Weight = item.Weight,
                Pcs = item.Pcs,

                PurchaseRate = purRate,
                PurchaseBack = item.PurchaseBack,
                SRate = item.SRate,

                Less1 = item.Less1,
                Less2 = item.Less2,
                Less3 = item.Less3,

                PurAmount = item.Amount,

                IssueCarats = item.IssueCarats,
                ReturnCarats = item.ReturnCarats,
                SpCarats = item.SpCarats,

                MemoTranCode = item.MemoTranCode,
                MemoTransCode = item.MemoTransCode,

                StockType = item.StockType,
                SuppLotNo = item.SuppLotNo
            };

            parcelList ??= new List<ImportExcelParcel>();

            bool isEdit = item.editIndex >= 0 && item.editIndex < parcelList.Count;

            if (isEdit)
            {
                parcelList[item.editIndex] = pur;
            }
            else
            {
                parcelList.Add(pur);
            }

            foreach (var row in parcelList)
            {
                double weight = row.Weight;

                totalCarats += row.Weight;
                totalAmount += row.PurAmount;
                totalMRPAmount += row.Weight * row.MRP;
                totalAmountWithoutless += weight * row.PurchaseRate;
            }

            return new PurchaseParcelGridView
            {
                TotalCarats = Math.Round(totalCarats, 3),
                TotalAmount = Math.Round(totalAmount, 2),
                TotalMRPAmount = Math.Round(totalMRPAmount, 3),
                TotalAmountWithoutless = Math.Round(totalAmountWithoutless, 3),
                Alert = alert,
                ExcelData = parcelList,
                IsEdit = isEdit
            };
        }

        public async Task<List<LotNoDataPPur>> GetLotDetailsAsync(string lotNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Type", "PPur" },
                { "@Where", $" AND LotNo = '{lotNo}' " }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<LotNoDataPPur>(
                "GetLotNoDataForAll",
                parameters,
                companyId);

            return result.ToList();
        }

        public async Task<(bool Success, string Message)> AddPurchaseParcelAsync(
            SalePurs salePur,
            List<ImportExcelpurAdd> items,
            string userId,
            string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int Curcode = salePur.CurCode;
                        float MCurBaseRate = salePur.BaseRate;
                        float SCurBaseRate = salePur.SBaseRate;

                        string MCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurOperator", " and ID=" + salePur.CurCode);
                        string SCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurOperator", " and ID=" + salePur.CurCode);
                        float NetAmount = salePur.NetAmount;

                        double MCurCode = 0;
                        double.TryParse(await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurID"), out MCurCode);
                        double SCurCode = 0;
                        double.TryParse(await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurID"), out SCurCode);

                        var amountRes = GetConvertAmount(Curcode, NetAmount, MCurBaseRate, MCurOperator, SCurBaseRate, SCurOperator, MCurCode, SCurCode);
                        double MCurNetAmt = amountRes.MCurAmt;
                        double SCurNeTAmt = amountRes.SCurAmt;

                        float MToSCurRate = SCurNeTAmt != 0 ? Convert.ToSingle(MCurNetAmt / SCurNeTAmt) : 0f;
                        int usercode = 0;
                        int.TryParse(userId, out usercode);

                        float FinalMAmount = salePur.FinalMAmount;
                        string MCurOperatorM = await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurOperator", " and ID=" + MCurCode);
                        string SCurOperatorM = await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurOperator", " and ID=" + MCurCode);

                        var finalAmtRes = GetConvertAmount(MCurCode, FinalMAmount, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                        double MCurFinalAmt = finalAmtRes.MCurAmt;
                        double SCurFinalAmt = finalAmtRes.SCurAmt;

                        int MaxInvNo;
                        string InvnoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''PPUR'' ");
                        int.TryParse(InvnoStr, out MaxInvNo);
                        MaxInvNo = MaxInvNo + 1;

                        bool outstanding = false;
                        bool stockEffected = true;
                        if (salePur.chkoldpur == true)
                        {
                            outstanding = true;
                            stockEffected = false;
                        }
                        else
                        {
                            outstanding = false;
                            stockEffected = true;
                        }

                        using (var cmd = new SqlCommand("SP_InsertSalePur", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@invno", MaxInvNo));
                            cmd.Parameters.Add(new SqlParameter("@billno", salePur.BillNo ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@trntype", "PPUR"));
                            cmd.Parameters.Add(new SqlParameter("@invtype", salePur.InvType ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@category", salePur.Category ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@trndate", salePur.TrnDate));
                            cmd.Parameters.Add(new SqlParameter("@duedays", salePur.DueDays));
                            cmd.Parameters.Add(new SqlParameter("@duedate", salePur.DueDate));
                            cmd.Parameters.Add(new SqlParameter("@partycode", salePur.PartyCode));
                            cmd.Parameters.Add(new SqlParameter("@sellercode", 0));
                            cmd.Parameters.Add(new SqlParameter("@term", salePur.Term ?? ""));
                            cmd.Parameters.Add(new SqlParameter("@countrycode", 0));
                            cmd.Parameters.Add(new SqlParameter("@BrokerCode", salePur.BrokerCode ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@BrokerPer", salePur.BrokerPer));
                            cmd.Parameters.Add(new SqlParameter("@BrokerAmount", salePur.BrokerAmount));
                            cmd.Parameters.Add(new SqlParameter("@adatcode", salePur.AdatCode ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@adatper", salePur.AdatPer));
                            cmd.Parameters.Add(new SqlParameter("@adatamount", salePur.AdatAmount));
                            cmd.Parameters.Add(new SqlParameter("@totcts", salePur.TotCts));
                            cmd.Parameters.Add(new SqlParameter("@totamount", Math.Round(salePur.TotAmount, 4)));
                            cmd.Parameters.Add(new SqlParameter("@netamount", Math.Round(salePur.NetAmount, 4)));
                            cmd.Parameters.Add(new SqlParameter("@PlusMinAmount", Math.Round(salePur.PlusMinusAmount, 4)));
                            cmd.Parameters.Add(new SqlParameter("@RoundAmount", Math.Round(salePur.RoundAmount, 4)));
                            cmd.Parameters.Add(new SqlParameter("@FinalAmount", Math.Round(FinalMAmount, 4)));
                            cmd.Parameters.Add(new SqlParameter("@BaseRate", Math.Round(salePur.BaseRate, 4)));
                            cmd.Parameters.Add(new SqlParameter("@Disc", salePur.InvDisc ?? ""));
                            cmd.Parameters.Add(new SqlParameter("@CurCode", Curcode.ToString()));
                            cmd.Parameters.Add(new SqlParameter("@Outstanding", outstanding));
                            cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                            cmd.Parameters.Add(new SqlParameter("@Brokerage", false));
                            cmd.Parameters.Add(new SqlParameter("@Commission", false));
                            cmd.Parameters.Add(new SqlParameter("@Cheque", false));
                            cmd.Parameters.Add(new SqlParameter("@usercode", usercode));
                            cmd.Parameters.Add(new SqlParameter("@mastercurrencyrate", Math.Round(MCurBaseRate, 4)));
                            cmd.Parameters.Add(new SqlParameter("@mastercurrencyfinalamount", Math.Round(MCurFinalAmt, 4)));
                            cmd.Parameters.Add(new SqlParameter("@stockcurrencyrate", Math.Round(SCurBaseRate, 4)));
                            cmd.Parameters.Add(new SqlParameter("@stockcurrencyfinalamount", Math.Round(SCurFinalAmt, 4)));
                            cmd.Parameters.Add(new SqlParameter("@ExpPartyCode", salePur.ExpPartyCode));
                            cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.DueDate));
                            cmd.Parameters.Add(new SqlParameter("@DivideDays", salePur.DivideDays));
                            cmd.Parameters.Add(new SqlParameter("@InterestPer", salePur.InterestPer));
                            cmd.Parameters.Add(new SqlParameter("@VatPer", salePur.VatPer));
                            cmd.Parameters.Add(new SqlParameter("@VatAmount", salePur.VatAmount));
                            cmd.Parameters.Add(new SqlParameter("@invdescription", salePur.InvDisc ?? ""));
                            cmd.Parameters.Add(new SqlParameter("@bankname", 0));
                            cmd.Parameters.Add(new SqlParameter("@subtype", salePur.SubType ?? ""));
                            cmd.Parameters.Add(new SqlParameter("@ShipVia", salePur.ShipVia ?? ""));
                            cmd.Parameters.Add(new SqlParameter("@deliverto", salePur.DeliverTo ?? ""));
                            cmd.Parameters.Add(new SqlParameter("@Tracking", salePur.Tracking ?? ""));
                            cmd.Parameters.Add(new SqlParameter("@SalesPerson", ""));
                            cmd.Parameters.Add(new SqlParameter("@Attendee", ""));
                            cmd.Parameters.Add(new SqlParameter("@labcharges", salePur.LabCharges));
                            cmd.Parameters.Add(new SqlParameter("@freightcharges", salePur.FreightCharges));
                            cmd.Parameters.Add(new SqlParameter("@duedays1", 0));
                            cmd.Parameters.Add(new SqlParameter("@duedays2", 0));
                            cmd.Parameters.Add(new SqlParameter("@CGSTPer", salePur.CGSTPer));
                            cmd.Parameters.Add(new SqlParameter("@CGSTAmt", salePur.CGSTAmt));
                            cmd.Parameters.Add(new SqlParameter("@SGSTPer", salePur.SGSTPer));
                            cmd.Parameters.Add(new SqlParameter("@SGSTAmt", salePur.SGSTAmt));
                            cmd.Parameters.Add(new SqlParameter("@IGSTPer", salePur.IGSTPer));
                            cmd.Parameters.Add(new SqlParameter("@IGSTAmt", salePur.IGSTAmt));
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
                            cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
                            cmd.Parameters.Add(new SqlParameter("@netMamount", salePur.NetMAmount));
                            cmd.Parameters.Add(new SqlParameter("@FinalPayRecAmount", salePur.FinalMPayRecAmount));
                            cmd.Parameters.Add(new SqlParameter("@Address", 1));
                            cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                            await cmd.ExecuteNonQueryAsync();
                        }

                        string MaxSPCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "SPCode");
                        int MaxSPCode = 0;
                        int.TryParse(MaxSPCodeStr, out MaxSPCode);

                        foreach (var im in items)
                        {
                            float AskingRate = im.AskingRate;
                            var askAmtRes = GetConvertAmount(Curcode, AskingRate, MCurBaseRate, MCurOperator, SCurBaseRate, SCurOperator, MCurCode, SCurCode);
                            double AkingMCurRate = askAmtRes.MCurAmt;
                            double AkingSCurRate = askAmtRes.SCurAmt;
                            double AskingSAmount = AkingSCurRate * im.Weight;

                            float ActualRate = im.ActualRate;
                            var actAmtRes = GetConvertAmount(Curcode, ActualRate, MCurBaseRate, MCurOperator, SCurBaseRate, SCurOperator, MCurCode, SCurCode);
                            double ActualMCurRate = actAmtRes.MCurAmt;
                            double ActualSCurRate = actAmtRes.SCurAmt;
                            double ActualSAmount = ActualSCurRate * im.Weight;

                            float PurRate = im.PurchaseRate;
                            var purRateRes = GetConvertAmount(Curcode, PurRate, MCurBaseRate, MCurOperator, SCurBaseRate, SCurOperator, MCurCode, SCurCode);
                            double PurMCurRate = purRateRes.MCurAmt;
                            double PurSCurRate = purRateRes.SCurAmt;

                            float PurAmount = im.PurAmount;
                            var purAmtRes = GetConvertAmount(Curcode, PurAmount, MCurBaseRate, MCurOperator, SCurBaseRate, SCurOperator, MCurCode, SCurCode);
                            double PurMCurAmt = purAmtRes.MCurAmt;
                            double PurSCurAmt = purAmtRes.SCurAmt;

                            float B2BRate = im.B2BRate;
                            var b2bRateRes = GetConvertAmount(Curcode, B2BRate, MCurBaseRate, MCurOperator, SCurBaseRate, SCurOperator, MCurCode, SCurCode);
                            double B2BRateMCur = b2bRateRes.MCurAmt;
                            double B2BRateSCur = b2bRateRes.SCurAmt;

                            float B2CRate = im.B2CRate;
                            var b2cRateRes = GetConvertAmount(Curcode, B2CRate, MCurBaseRate, MCurOperator, SCurBaseRate, SCurOperator, MCurCode, SCurCode);
                            double B2CRateMCur = b2cRateRes.MCurAmt;
                            double B2CRateSCur = b2cRateRes.SCurAmt;

                            string LotAlready = await ExecuteCommonDataScalarAsync(conn, trans, "LotExist", " AND LotNo = ''" + im.LotNo + "''");

                            string ItemSrno = await ExecuteCommonDataScalarAsync(conn, trans, "MaxItemSrNo");
                            int MaxItemSrno = 0;
                            int.TryParse(ItemSrno, out MaxItemSrno);
                            MaxItemSrno = MaxItemSrno + 1;

                            string GenerateParcelPurchaseLotno = await GetSettingValueAsync(conn, trans, "GenerateParcelPurchaseLotno");
                            string lotStr = "";

                            if (string.IsNullOrWhiteSpace(im.LotNo))
                            {
                                lotStr = GenerateParcelPurchaseLotno + MaxItemSrno.ToString();
                            }
                            else
                            {
                                lotStr = im.LotNo;
                            }

                            im.LotNo = lotStr;

                            if (LotAlready.ToString() == "0")
                            {
                                int singlerPcs = (im.Pcs == 1) ? 1 : 0;

                                using (var cmd = new SqlCommand("SP_ItemMaster", conn, trans))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@ItemCode", 0));
                                    cmd.Parameters.Add(new SqlParameter("@ItemSrno", MaxItemSrno));
                                    cmd.Parameters.Add(new SqlParameter("@LotNo", im.LotNo));
                                    cmd.Parameters.Add(new SqlParameter("@ItemName", im.ItemName ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ItemGroupId", im.PShape ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Category", salePur.Category ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ProductType", salePur.Category ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                                    cmd.Parameters.Add(new SqlParameter("@Weight", 0));
                                    cmd.Parameters.Add(new SqlParameter("@CaratCL", Convert.ToDouble(im.Weight.ToString("N3"))));
                                    cmd.Parameters.Add(new SqlParameter("@SieveId", im.PSieve ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ShapeId", im.PShape ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ColorId", im.PColor ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@SizeId", im.PSize ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ClarityId", im.PClarity ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@MRP", im.MRP));
                                    cmd.Parameters.Add(new SqlParameter("@Back", im.AskingBack));
                                    cmd.Parameters.Add(new SqlParameter("@AskingRate", Math.Round(AkingSCurRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@AskingAmount", Math.Round(AskingSAmount, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@AmountCl", Math.Round(PurSCurAmt, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@BoxName", ""));
                                    cmd.Parameters.Add(new SqlParameter("@BoxRate", 0.0));
                                    cmd.Parameters.Add(new SqlParameter("@CertId", im.LAB ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@CertificateNo", im.CertificateNo ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@HAID", im.HA ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@PropId", im.Proportion ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@PolishId", im.Polish ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@SymId", im.Symmetry ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@FLId", im.FL ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@FLColor", im.FLColor ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@FancyColor", ""));
                                    cmd.Parameters.Add(new SqlParameter("@FancyColorIntensity", ""));
                                    cmd.Parameters.Add(new SqlParameter("@FancyColorOvertone", ""));
                                    cmd.Parameters.Add(new SqlParameter("@L", Math.Round(im.Length, 2)));
                                    cmd.Parameters.Add(new SqlParameter("@W", Math.Round(im.Width, 2)));
                                    cmd.Parameters.Add(new SqlParameter("@D", Math.Round(im.Depth, 2)));
                                    cmd.Parameters.Add(new SqlParameter("@Diameter", ""));
                                    cmd.Parameters.Add(new SqlParameter("@Measurement", ""));
                                    cmd.Parameters.Add(new SqlParameter("@LWRatio", im.LW));
                                    cmd.Parameters.Add(new SqlParameter("@TotDepth", im.TotDep));
                                    cmd.Parameters.Add(new SqlParameter("@TablePer", im.TablePer));
                                    cmd.Parameters.Add(new SqlParameter("@CrownHeight", im.CH));
                                    cmd.Parameters.Add(new SqlParameter("@CrownAngle", im.CA));
                                    cmd.Parameters.Add(new SqlParameter("@PavilionPer", im.PA));
                                    cmd.Parameters.Add(new SqlParameter("@PavilionHeight", im.PH));
                                    cmd.Parameters.Add(new SqlParameter("@GirdlePer", im.GirdlePer));
                                    cmd.Parameters.Add(new SqlParameter("@GirdleCondition", im.GirdleCondition ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@GirdleMin", im.GMin ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@GirdleMax", im.GMax ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@GirdleMinMS", im.Min));
                                    cmd.Parameters.Add(new SqlParameter("@GirdleMaxMS", im.Max));
                                    cmd.Parameters.Add(new SqlParameter("@CuletId", im.Culet ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@CuletCondition", im.CuletCondition ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@StrLn", im.StrLn));
                                    cmd.Parameters.Add(new SqlParameter("@LrHalf", im.LrHalf));
                                    cmd.Parameters.Add(new SqlParameter("@LaserInscription", false));
                                    cmd.Parameters.Add(new SqlParameter("@LaserInscriptionText", im.InscriptionText ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Instruction", ""));
                                    cmd.Parameters.Add(new SqlParameter("@KeyToSymbols", im.KeyToSymbols ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@LABComments", im.LabComments ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@BrownId", im.Brown ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@GreenId", im.Green ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@MilkyId", im.Milky ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@LusterId", im.Luster ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ButhId", ""));
                                    cmd.Parameters.Add(new SqlParameter("@NattsId", im.Natts ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@EyeClean", im.EyeClean ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ShadeId", im.Shade ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@TableInclusion", im.BIT ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@CrownInclusion", im.BIS ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@OIT", im.OIT ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@OIC", im.OIC ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@OIP", im.OIP ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@OIG", im.OIG ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@PolishFeatures", ""));
                                    cmd.Parameters.Add(new SqlParameter("@SymFeatures", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ColorDescription", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ClarityDescription", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ClarityStatus", ""));
                                    cmd.Parameters.Add(new SqlParameter("@Graining", ""));
                                    cmd.Parameters.Add(new SqlParameter("@Painting", ""));
                                    cmd.Parameters.Add(new SqlParameter("@IdealImagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@AssetImagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@HeartsImagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ArrowsImagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@B2BVideoPath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@B2CVideoPath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@B2BDNAPagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@B2CDNAPagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@CertificatePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@PlottingImagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@PurchaseDate", salePur.TrnDate));
                                    cmd.Parameters.Add(new SqlParameter("@PublicRemarks", ""));
                                    cmd.Parameters.Add(new SqlParameter("@PrivateRemarks", ""));
                                    cmd.Parameters.Add(new SqlParameter("@AdminRemarks", ""));
                                    cmd.Parameters.Add(new SqlParameter("@PurchaseRate", Math.Round(PurSCurRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@PurchaseBack", im.PurchaseBack));
                                    cmd.Parameters.Add(new SqlParameter("@PurchaseCarats", Convert.ToDouble(im.Weight.ToString("N3"))));
                                    cmd.Parameters.Add(new SqlParameter("@SPPurchaseRate", Math.Round(im.PurchaseRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@SPPurchaseBack", im.PurchaseBack));
                                    cmd.Parameters.Add(new SqlParameter("@PurchaseMRP", im.MRP));
                                    cmd.Parameters.Add(new SqlParameter("@PDollorRate", Math.Round(salePur.BaseRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(MCurBaseRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", Math.Round(PurMCurAmt, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(SCurBaseRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", Math.Round(PurSCurAmt, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@AdatPer", Math.Round(salePur.AdatPer, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@Brokerper", Math.Round(salePur.BrokerPer, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@LabReportDate", im.ReportDate ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@SinglePcs", singlerPcs));
                                    cmd.Parameters.Add(new SqlParameter("@PurchaseAmount", Math.Round(PurSCurAmt, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@ActualRate", Math.Round(ActualSCurRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@ActualBack", im.ActualBack));
                                    cmd.Parameters.Add(new SqlParameter("@ActualAmount", Math.Round(ActualSAmount, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@BottomBack", im.BottomBack));
                                    cmd.Parameters.Add(new SqlParameter("@BottomRate", im.BottomRate));
                                    cmd.Parameters.Add(new SqlParameter("@B2BRate", Math.Round(B2BRateSCur, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@B2BBack", im.B2BBack));
                                    cmd.Parameters.Add(new SqlParameter("@B2CRate", Math.Round(B2CRateSCur, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@B2CBack", im.B2CBack));
                                    cmd.Parameters.Add(new SqlParameter("@MfgSeriesID", im.MfgSeriesID ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@MfgPacketID", im.SuppLotNo ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@RoughId", ""));
                                    cmd.Parameters.Add(new SqlParameter("@MinesId", ""));
                                    cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                                    cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                                    cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                                    cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
                                    cmd.Parameters.Add(new SqlParameter("@City", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ImagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@VideoPath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@PagePath", ""));
                                    cmd.Parameters.Add(new SqlParameter("@CountryOfOrigin", ""));
                                    cmd.Parameters.Add(new SqlParameter("@CountryOfMfg", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                                    await cmd.ExecuteNonQueryAsync();
                                }

                                using (var cmd = new SqlCommand("SP_UpdateSizeid", conn, trans))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@LotNo", im.LotNo));
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            string ItemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "ItemCode", " AND LotNo = ''" + im.LotNo + "''");
                            int MaxItemCode = 0;
                            int.TryParse(ItemCodeStr, out MaxItemCode);

                            string itemName = "";
                            if (!string.IsNullOrWhiteSpace(im.ItemName))
                            {
                                itemName = im.ItemName;
                            }
                            else
                            {
                                itemName = im.LotNo + " " + im.PShape + " " + im.PSize + " " + im.PColor + " " + im.PClarity;
                            }

                            using (var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@SPdate", salePur.TrnDate));
                                cmd.Parameters.Add(new SqlParameter("@TrnType", "PPUR"));
                                cmd.Parameters.Add(new SqlParameter("@InvType", salePur.InvType ?? (object)DBNull.Value));
                                cmd.Parameters.Add(new SqlParameter("@InvCode", MaxSPCode));
                                cmd.Parameters.Add(new SqlParameter("@ItemCode", MaxItemCode));
                                cmd.Parameters.Add(new SqlParameter("@ItemName", itemName));
                                cmd.Parameters.Add(new SqlParameter("@Carats", float.Parse(im.Weight.ToString("N3"))));
                                cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                                cmd.Parameters.Add(new SqlParameter("@Rate", Convert.ToDouble(im.PurchaseRate.ToString("N2"))));
                                cmd.Parameters.Add(new SqlParameter("@MRP", im.MRP));
                                cmd.Parameters.Add(new SqlParameter("@PMRPDown", Convert.ToDouble(im.PurchaseBack.ToString("N2"))));
                                cmd.Parameters.Add(new SqlParameter("@ReturnCarats", Convert.ToDouble(im.ReturnCarats.ToString("N2"))));
                                cmd.Parameters.Add(new SqlParameter("@IssueCarats", Convert.ToDouble(im.IssueCarats.ToString("N2"))));
                                cmd.Parameters.Add(new SqlParameter("@amount", Convert.ToDouble(im.PurAmount.ToString("N2"))));
                                cmd.Parameters.Add(new SqlParameter("@ExportNo", ""));
                                cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                                cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                                cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                                cmd.Parameters.Add(new SqlParameter("@CurCode", salePur.CurCode.ToString()));
                                cmd.Parameters.Add(new SqlParameter("@memotrancode", im.MemoTransCode));
                                cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                                cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
                                cmd.Parameters.Add(new SqlParameter("@spcarats", (im.MemoTransCode != 0) ? float.Parse(im.Weight.ToString("N3")) : 0f));
                                cmd.Parameters.Add(new SqlParameter("@FItemCode", 0));
                                cmd.Parameters.Add(new SqlParameter("@TItemCode", 0));
                                cmd.Parameters.Add(new SqlParameter("@FItemRate", 0.0));
                                cmd.Parameters.Add(new SqlParameter("@TItemRate", 0.0));
                                cmd.Parameters.Add(new SqlParameter("@FromSuratCode", 0));
                                cmd.Parameters.Add(new SqlParameter("@AssorterCode", 0));
                                cmd.Parameters.Add(new SqlParameter("@RoughCode", 0));
                                cmd.Parameters.Add(new SqlParameter("@MemoRemark", ""));
                                cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
                                cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(MCurBaseRate, 4)));
                                cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", Convert.ToDouble(PurMCurAmt.ToString("N2"))));
                                cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(SCurBaseRate, 4)));
                                cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", Convert.ToDouble(PurSCurAmt.ToString("N2"))));
                                cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
                                cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
                                cmd.Parameters.Add(new SqlParameter("@PShape", im.PShape ?? ""));
                                cmd.Parameters.Add(new SqlParameter("@PSize", im.PSize ?? ""));
                                cmd.Parameters.Add(new SqlParameter("@PSieve", im.PSieve ?? ""));
                                cmd.Parameters.Add(new SqlParameter("@PColor", im.PColor ?? ""));
                                cmd.Parameters.Add(new SqlParameter("@PClarity", im.PClarity ?? ""));
                                cmd.Parameters.Add(new SqlParameter("@COGS", 0.0f));
                                cmd.Parameters.Add(new SqlParameter("@StockType", im.StockType ?? ""));
                                cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                                await cmd.ExecuteNonQueryAsync();
                            }

                            if (im.MemoTransCode != 0)
                            {
                                using (var cmd = new SqlCommand("SP_UpdateMemo", conn, trans))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@code", im.MemoTransCode));
                                    cmd.Parameters.Add(new SqlParameter("@RtnDate", salePur.TrnDate));
                                    cmd.Parameters.Add(new SqlParameter("@rtncts", float.Parse(im.ReturnCarats.ToString("N3"))));
                                    cmd.Parameters.Add(new SqlParameter("@spcts", float.Parse(im.Weight.ToString("N3"))));
                                    cmd.Parameters.Add(new SqlParameter("@Actiontype", "contopurtest"));
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                            else
                            {
                                using (var cmd = new SqlCommand("SP_UpdateMemo", conn, trans))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@code", im.MemoTransCode));
                                    cmd.Parameters.Add(new SqlParameter("@RtnDate", salePur.TrnDate));
                                    cmd.Parameters.Add(new SqlParameter("@rtncts", float.Parse(im.ReturnCarats.ToString("N3"))));
                                    cmd.Parameters.Add(new SqlParameter("@spcts", float.Parse(im.Weight.ToString("N3"))));
                                    cmd.Parameters.Add(new SqlParameter("@Actiontype", "contopur"));
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            bool chkGrading = salePur.chkGrading;
                            if (chkGrading == true)
                            {
                                using (var cmd = new SqlCommand("SP_InsertMultiGrading", conn, trans))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@ItemCode", MaxItemCode));
                                    cmd.Parameters.Add(new SqlParameter("@itemsrno", MaxItemSrno));
                                    cmd.Parameters.Add(new SqlParameter("@LotNo", im.LotNo));
                                    cmd.Parameters.Add(new SqlParameter("@MfgSeriesID", im.MfgSeriesID ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@MfgPacketID", im.MfgPacketID ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ECutCode", im.PShape ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@GraderName", "Selected"));
                                    cmd.Parameters.Add(new SqlParameter("@EColorCode", im.PColor ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@EClarityCode", im.PClarity ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@carats", Convert.ToDouble(im.Weight.ToString("N3"))));
                                    cmd.Parameters.Add(new SqlParameter("@rate", Math.Round(AkingSCurRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@ActualRate", Math.Round(ActualSCurRate, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@amount", Math.Round(im.PurAmount, 4)));
                                    cmd.Parameters.Add(new SqlParameter("@MRP", im.MRP));
                                    cmd.Parameters.Add(new SqlParameter("@PMRPDown", im.AskingBack));
                                    cmd.Parameters.Add(new SqlParameter("@ActualPRapDown", im.AskingBack));
                                    cmd.Parameters.Add(new SqlParameter("@ESizeCode", ""));
                                    cmd.Parameters.Add(new SqlParameter("@CertCode", im.LAB ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@PropCode", im.Proportion ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@SymCode", im.Symmetry ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@PolishCode", im.Polish ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Diameter", ""));
                                    cmd.Parameters.Add(new SqlParameter("@D", Math.Round(im.Depth, 2)));
                                    cmd.Parameters.Add(new SqlParameter("@L", Math.Round(im.Length, 2)));
                                    cmd.Parameters.Add(new SqlParameter("@W", Math.Round(im.Width, 2)));
                                    cmd.Parameters.Add(new SqlParameter("@LW", im.LW));
                                    cmd.Parameters.Add(new SqlParameter("@TopDepth", im.TotDep));
                                    cmd.Parameters.Add(new SqlParameter("@Table", im.TablePer));
                                    cmd.Parameters.Add(new SqlParameter("@CrownHeight", im.CH));
                                    cmd.Parameters.Add(new SqlParameter("@CA", im.CA));
                                    cmd.Parameters.Add(new SqlParameter("@GerdleMin", im.GMin ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@GerdleMax", im.GMax ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@GerdleMSMin", im.Min));
                                    cmd.Parameters.Add(new SqlParameter("@GerdleMSMax", im.Max));
                                    cmd.Parameters.Add(new SqlParameter("@PavilionPer", im.PA));
                                    cmd.Parameters.Add(new SqlParameter("@PavilionHeight", im.PH));
                                    cmd.Parameters.Add(new SqlParameter("@Culet", im.Culet ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@FLCode", im.FL ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@MilkyCode", im.Milky ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@LusterCode", im.Luster ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Natts", im.Natts ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Brown", im.Brown ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Buth", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ClarityStatus", ""));
                                    cmd.Parameters.Add(new SqlParameter("@PolishFeatures", ""));
                                    cmd.Parameters.Add(new SqlParameter("@SymFeatures", ""));
                                    cmd.Parameters.Add(new SqlParameter("@OIT", im.OIT ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@OIG", im.OIG ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@OIC", im.OIC ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@OIP", im.OIP ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@TableInclusion", im.BIT ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@CrownInclusion", im.BIS ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@HACode", im.HA ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@CertificateNo", im.CertificateNo ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@KeyToSymbols", im.KeyToSymbols ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@GDate", salePur.TrnDate));
                                    cmd.Parameters.Add(new SqlParameter("@Status", "PPUR"));
                                    cmd.Parameters.Add(new SqlParameter("@UpdateFlag", false));
                                    cmd.Parameters.Add(new SqlParameter("@Sync_Date", ""));
                                    cmd.Parameters.Add(new SqlParameter("@SinglePcs", true));
                                    cmd.Parameters.Add(new SqlParameter("@FinalPurPrice", true));
                                    cmd.Parameters.Add(new SqlParameter("@ReportDate", im.ReportDate ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@LaserInscriptionText", im.InscriptionText ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@StrLn", im.StrLn));
                                    cmd.Parameters.Add(new SqlParameter("@LrHalf", im.LrHalf));
                                    cmd.Parameters.Add(new SqlParameter("@GerdlePer", im.GirdlePer));
                                    cmd.Parameters.Add(new SqlParameter("@Comments", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ControlNo", ""));
                                    cmd.Parameters.Add(new SqlParameter("@CertExpense", 0));
                                    cmd.Parameters.Add(new SqlParameter("@Remarks", im.LabComments ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Instruction", ""));
                                    cmd.Parameters.Add(new SqlParameter("@SuratRemarks", ""));
                                    cmd.Parameters.Add(new SqlParameter("@IGICut", ""));
                                    cmd.Parameters.Add(new SqlParameter("@LABLocation", ""));
                                    cmd.Parameters.Add(new SqlParameter("@EyeClean", im.EyeClean ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@FlColor", im.FLColor ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@LABJobNo", ""));
                                    cmd.Parameters.Add(new SqlParameter("@LABMemoNo", ""));
                                    cmd.Parameters.Add(new SqlParameter("@LABPaint", ""));
                                    cmd.Parameters.Add(new SqlParameter("@LABPatintingComments", ""));
                                    cmd.Parameters.Add(new SqlParameter("@LABSyntheticIndecator", ""));
                                    cmd.Parameters.Add(new SqlParameter("@Painting", ""));
                                    cmd.Parameters.Add(new SqlParameter("@LABColorDesc", ""));
                                    cmd.Parameters.Add(new SqlParameter("@GirdleCondition", im.GirdleCondition ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ReportAction", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ServiceCode", ""));
                                    cmd.Parameters.Add(new SqlParameter("@RoughId", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ShapeName", im.PShape ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Shade", im.Shade ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@VV", ""));
                                    cmd.Parameters.Add(new SqlParameter("@COl2", ""));
                                    cmd.Parameters.Add(new SqlParameter("@SugLAB", ""));
                                    cmd.Parameters.Add(new SqlParameter("@REstRate", ""));
                                    cmd.Parameters.Add(new SqlParameter("@REstCts", ""));
                                    cmd.Parameters.Add(new SqlParameter("@REstEColorCode", ""));
                                    cmd.Parameters.Add(new SqlParameter("@REstEClarityCode", ""));
                                    cmd.Parameters.Add(new SqlParameter("@HRDCut", ""));
                                    cmd.Parameters.Add(new SqlParameter("@FancyColorOvertone", im.FancyColorOvertone ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@CuletCondition", im.CuletCondition ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@Green", im.Green ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@FancyColorIntensity", im.FancyColorIntensity ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@FancyColor", im.FancyColor ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@OP", ""));
                                    cmd.Parameters.Add(new SqlParameter("@SieveId", im.PSieve ?? ""));
                                    cmd.Parameters.Add(new SqlParameter("@ClarityDescription", ""));
                                    cmd.Parameters.Add(new SqlParameter("@ColorDescription", ""));
                                    cmd.Parameters.Add(new SqlParameter("@SuratStoneID", ""));
                                    cmd.Parameters.Add(new SqlParameter("@Graining", ""));
                                    cmd.Parameters.Add(new SqlParameter("@CertResult", ""));
                                    cmd.Parameters.Add(new SqlParameter("@Measurement", ""));
                                    cmd.Parameters.Add(new SqlParameter("@TypeID", ""));
                                    cmd.Parameters.Add(new SqlParameter("@Black", ""));
                                    cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        if (salePur.chkoldpur == false)
                        {
                            long PurchaseCode = 0;
                            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Purchase''"), out PurchaseCode);
                            long VatCode = 0;
                            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Vat''"), out VatCode);
                            long SGSTCode = 0;
                            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''SGST''"), out SGSTCode);
                            long CGSTCode = 0;
                            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''CGST''"), out CGSTCode);
                            long IGSTCode = 0;
                            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''IGST''"), out IGSTCode);
                            long BrokerageCode = 0;
                            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Brokerage''"), out BrokerageCode);
                            long AdatCode = 0;
                            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Adat''"), out AdatCode);
                            long CashCode = 0;
                            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Cash''"), out CashCode);

                            float PurJvAmount = salePur.FinalMAmount;
                            var jvConvert = GetConvertAmount(MCurCode, PurJvAmount, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                            double PurJvMAmount = jvConvert.MCurAmt;
                            double PurJvSAmount = jvConvert.SCurAmt;

                            float PurchaseJVAMount = 0;
                            if (MCurOperator == "*")
                            {
                                PurchaseJVAMount = MCurBaseRate != 0 ? PurJvAmount / MCurBaseRate : 0f;
                            }
                            if (MCurOperator == "/")
                            {
                                PurchaseJVAMount = PurJvAmount * MCurBaseRate;
                            }

                            await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, PurchaseCode, salePur.PartyCode, Math.Abs(PurchaseJVAMount), Curcode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, PurJvMAmount, SCurBaseRate, PurJvSAmount, 0, usercode, userId, companyId);

                            if ((Convert.ToDouble(salePur.BrokerAmount) != 0) && salePur.BrokerCode != null)
                            {
                                float BroAmt = salePur.BrokerAmount;
                                var broConvert = GetConvertAmount(MCurCode, BroAmt, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                                double BroMAmt = broConvert.MCurAmt;
                                double BroSAmt = broConvert.SCurAmt;

                                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, BrokerageCode, salePur.BrokerCode.Value, Math.Abs(BroAmt), MCurCode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, BroMAmt, SCurBaseRate, BroSAmt, 0, usercode, userId, companyId);
                            }

                            if ((Convert.ToDouble(salePur.AdatAmount) != 0) && salePur.AdatCode != null)
                            {
                                float AdatAmt = salePur.AdatAmount;
                                var adatConvert = GetConvertAmount(MCurCode, AdatAmt, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                                double AdatMAmt = adatConvert.MCurAmt;
                                double AdatSAmt = adatConvert.SCurAmt;

                                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, AdatCode, salePur.AdatCode.Value, AdatAmt, MCurCode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, AdatMAmt, SCurBaseRate, AdatSAmt, 0, usercode, userId, companyId);
                            }

                            if (Convert.ToDouble(salePur.VatAmount) != 0)
                            {
                                float VatAmt = salePur.VatAmount;
                                var vatConvert = GetConvertAmount(MCurCode, VatAmt, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                                double VatMAmt = vatConvert.MCurAmt;
                                double VatSAmt = vatConvert.SCurAmt;

                                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, VatCode, PurchaseCode, VatAmt, MCurCode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, VatMAmt, SCurBaseRate, VatSAmt, 0, usercode, userId, companyId);
                            }

                            if (Convert.ToDouble(salePur.SGSTAmt) != 0)
                            {
                                float SGSTAmt = salePur.SGSTAmt;
                                var sgstConvert = GetConvertAmount(MCurCode, SGSTAmt, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                                double SGSTMAmt = sgstConvert.MCurAmt;
                                double SGSTSAmt = sgstConvert.SCurAmt;

                                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, SGSTCode, PurchaseCode, SGSTAmt, MCurCode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, SGSTMAmt, SCurBaseRate, SGSTSAmt, 0, usercode, userId, companyId);
                            }

                            if (Convert.ToDouble(salePur.CGSTAmt) != 0)
                            {
                                float CGSTAmt = salePur.CGSTAmt;
                                var cgstConvert = GetConvertAmount(MCurCode, CGSTAmt, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                                double CGSTMAmt = cgstConvert.MCurAmt;
                                double CGSTSAmt = cgstConvert.SCurAmt;

                                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, CGSTCode, PurchaseCode, CGSTAmt, MCurCode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, CGSTMAmt, SCurBaseRate, CGSTSAmt, 0, usercode, userId, companyId);
                            }

                            if (Convert.ToDouble(salePur.IGSTAmt) != 0)
                            {
                                float IGSTAmt = salePur.IGSTAmt;
                                var igstConvert = GetConvertAmount(MCurCode, IGSTAmt, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                                double IGSTMAmt = igstConvert.MCurAmt;
                                double IGSTSAmt = igstConvert.SCurAmt;

                                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, IGSTCode, PurchaseCode, IGSTAmt, MCurCode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, IGSTMAmt, SCurBaseRate, IGSTSAmt, 0, usercode, userId, companyId);
                            }

                            if (Convert.ToDouble(salePur.RoundAmount) > 0)
                            {
                                float RoundAmt = salePur.RoundAmount;
                                var roundConvert = GetConvertAmount(MCurCode, RoundAmt, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                                double RoundMAmt = roundConvert.MCurAmt;
                                double RoundSAmt = roundConvert.SCurAmt;

                                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, CashCode, salePur.PartyCode, salePur.RoundAmount, MCurCode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, RoundMAmt, SCurBaseRate, RoundSAmt, 0, usercode, userId, companyId);
                            }
                            else if (Convert.ToDouble(salePur.RoundAmount) < 0)
                            {
                                float RoundAmt = salePur.RoundAmount;
                                var roundConvert = GetConvertAmount(MCurCode, RoundAmt, 1, MCurOperatorM, MToSCurRate, SCurOperatorM, MCurCode, SCurCode);
                                double RoundMAmt = roundConvert.MCurAmt;
                                double RoundSAmt = roundConvert.SCurAmt;

                                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.PartyCode, CashCode, Math.Abs(RoundAmt), MCurCode.ToString(), "PPUR", MaxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, MCurBaseRate, Math.Abs(RoundMAmt), SCurBaseRate, Math.Abs(RoundSAmt), 0, usercode, userId, companyId);
                            }
                        }

                        trans.Commit();
                        return (true, "Purchase Parcel Record Saved.");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return (false, ex.Message);
                    }
                }
            }
        }

        private async Task<string> ExecuteCommonDataScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using (var cmd = new SqlCommand("GetAllCommonData", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Type", type));
                cmd.Parameters.Add(new SqlParameter("@Where", where ?? ""));
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? string.Empty;
            }
        }

        private async Task<string> ExecuteCurrencyDetailScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using (var cmd = new SqlCommand("GetCurrencyDetail", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Type", type));
                cmd.Parameters.Add(new SqlParameter("@Where", where ?? ""));
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? string.Empty;
            }
        }

        private async Task<string> GetSettingValueAsync(SqlConnection conn, SqlTransaction trans, string name)
        {
            using (var cmd = new SqlCommand("GetALLMasterList", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Type", "Setting"));
                cmd.Parameters.Add(new SqlParameter("@Where", $" and Name=''{name}''"));
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int valueCol = reader.GetOrdinal("Value");
                        return reader.GetValue(valueCol)?.ToString() ?? string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        private (double MCurAmt, double SCurAmt) GetConvertAmount(
            double curCode,
            double amount,
            double mCurBaseRate,
            string mCurOperator,
            double sCurBaseRate,
            string sCurOperator,
            double mCurCode,
            double sCurCode)
        {
            double mCurAmt = 0;
            double sCurAmt = 0;

            if (curCode == mCurCode)
            {
                mCurAmt = amount;
                if (sCurOperator == "*")
                    sCurAmt = amount * sCurBaseRate;
                else
                    sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0;
            }
            else if (curCode == sCurCode)
            {
                sCurAmt = amount;
                if (mCurOperator == "*")
                    mCurAmt = amount * mCurBaseRate;
                else
                    mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0;
            }
            else
            {
                if (mCurOperator == "*")
                    mCurAmt = amount * mCurBaseRate;
                else
                    mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0;

                if (sCurOperator == "*")
                    sCurAmt = amount * sCurBaseRate;
                else
                    sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0;
            }

            if (double.IsNaN(mCurAmt) || double.IsInfinity(mCurAmt))
                mCurAmt = 0;
            if (double.IsNaN(sCurAmt) || double.IsInfinity(sCurAmt))
                sCurAmt = 0;

            return (mCurAmt, sCurAmt);
        }

        private async Task InsertJvAsync(
            SqlConnection conn,
            SqlTransaction trans,
            DateTime jvDate,
            DateTime dueDate,
            long dAccCode,
            long cAccCode,
            double amount,
            string curCode,
            string vType,
            int vCode,
            string disc,
            string adjustVType,
            int adjustVCode,
            double baseRate,
            double masterCurrencyRate,
            double masterCurrencyAmount,
            double stockCurrencyRate,
            double stockCurrencyAmount,
            double gCode,
            int userCode,
            string createdBy,
            string companyId,
            long againstAccCode = 0)
        {
            using (var cmd = new SqlCommand("SP_insertjv", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@dacccode", dAccCode));
                cmd.Parameters.Add(new SqlParameter("@cacccode", cAccCode));
                cmd.Parameters.Add(new SqlParameter("@amount", amount));
                cmd.Parameters.Add(new SqlParameter("@JVdate", jvDate));
                cmd.Parameters.Add(new SqlParameter("@duedate", dueDate));
                cmd.Parameters.Add(new SqlParameter("@curcode", curCode ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@vtype", vType ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@vcode", vCode));
                cmd.Parameters.Add(new SqlParameter("@disc", disc ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Adjustvtype", adjustVCode));
                cmd.Parameters.Add(new SqlParameter("@Adjustvcode", adjustVCode));
                cmd.Parameters.Add(new SqlParameter("@BaseRate", baseRate));
                cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", baseRate));
                cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", masterCurrencyAmount));
                cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", stockCurrencyRate));
                cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", stockCurrencyAmount));
                cmd.Parameters.Add(new SqlParameter("@GCode", gCode));
                cmd.Parameters.Add(new SqlParameter("@UserCode", userCode));
                cmd.Parameters.Add(new SqlParameter("@CreatedBy", createdBy ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Companyid", companyId));
                cmd.Parameters.Add(new SqlParameter("@AgainstAccCode", againstAccCode));
                cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}