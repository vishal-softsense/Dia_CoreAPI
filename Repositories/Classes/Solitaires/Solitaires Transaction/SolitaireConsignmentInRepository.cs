using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitaireConsignmentInRepository : ISolitaireConsignmentInRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public SolitaireConsignmentInRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<List<SolitaireConsignmentInInvListDto>> GetListAsync(int pageNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@PageNo", pageNo }
            };

            var result = (await _sqlDataAccess.SelectDataFromSP<SolitaireConsignmentInInvListDto>(
                "GetconsignmentInvDataUsingPageNo",
                parameters,
                companyId)).ToList();

            foreach (var row in result)
            {
                row.InvNo = StripHtmlToText(row.InvNo);
                if (string.IsNullOrWhiteSpace(row.Inv) && !string.IsNullOrWhiteSpace(row.InvNo))
                    row.Inv = row.InvNo;
                else
                    row.Inv = StripHtmlToText(row.Inv);
            }

            return result;
        }

        private static string? StripHtmlToText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var text = value.Trim();
            if (!text.Contains("<", StringComparison.Ordinal))
                return text;

            var open = text.IndexOf('>');
            var close = text.LastIndexOf('<');
            if (open >= 0 && close > open)
                text = text.Substring(open + 1, close - open - 1);

            text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", string.Empty);
            return System.Net.WebUtility.HtmlDecode(text).Trim();
        }

        public async Task<SolitaireConsignmentInPageDataDto> GetPageDataAsync(string companyId)
        {
            var transactionData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "AllMasterDataForPurchase",
                new Dictionary<string, object>(),
                companyId);

            var list = transactionData.ToList();

            var defaultCurrencyCode = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'DefaultCurrency'",
                new { },
                companyId);

            return new SolitaireConsignmentInPageDataDto
            {
                Type = MapBasic(list.Where(x => x.Type == "Type")),
                SubType = MapBasic(list.Where(x => x.Type == "SubType")),
                Category = MapBasic(list.Where(x => x.Type == "Category")),
                CurrencyList = MapBasic(list.Where(x => x.Type == "Currency")),
                DefaultCurrencyCode = defaultCurrencyCode
            };
        }

        private static List<BasicDataDto> MapBasic(IEnumerable<StockBasic> source)
        {
            return source.Select(x => new BasicDataDto
            {
                ID = x.ID,
                Type = x.Type,
                Priority = x.Priority,
                Name = x.Name,
                TextName = x.TextName
            }).ToList();
        }

        public async Task<SolitaireConsignmentInDetailDto?> GetDetailAsync(int id, string companyId)
        {
            var headerParams = new Dictionary<string, object>
            {
                { "@Where", $" tsp.TrnType = 'MemoIn' AND Code = {id}" }
            };

            var header = await _sqlDataAccess.SelectDataFromSPSingle<SolitaireConsignmentInHeaderDto>(
                "GetSalepurRecord",
                headerParams,
                companyId);

            if (header == null)
                return null;

            var itemParams = new Dictionary<string, object>
            {
                { "@Where", $" AND InvCode = {id}" }
            };

            var items = (await _sqlDataAccess.SelectDataFromSP<SolitaireConsignmentInItemDto>(
                "GetSalepurTranWithStatus",
                new Dictionary<string, object> { { "@Where", $" AND InvCode = {id} AND TrnType = 'MemoIn'" } },
                companyId)).ToList();

            foreach (var item in items)
                NormalizeItem(item);

            return new SolitaireConsignmentInDetailDto
            {
                Header = header,
                Items = items
            };
        }

        public async Task<List<SolitaireConsignmentInLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Type", "Purchase" },
                { "@Where", $" AND LotNo = '{lotNo}' " }
            };

            var result = (await _sqlDataAccess.SelectDataFromSP<SolitaireConsignmentInLotDetailDto>(
                "GetLotNoDataForAll",
                parameters,
                companyId)).ToList();

            foreach (var lot in result)
                NormalizeLotDetail(lot);

            return result;
        }

        private static void NormalizeItem(SolitaireConsignmentInItemDto item)
        {
            if (item == null) return;

            if (item.Weight == 0 && item.Carats != 0)
                item.Weight = item.Carats;

            if (item.MRP == 0 && item.PurchaseMRP != 0)
                item.MRP = item.PurchaseMRP;

            if (item.PurchaseBack == 0 && item.PMRPDown != 0)
                item.PurchaseBack = item.PMRPDown;

            if (item.PurchaseRate == 0 && item.Rate != 0)
                item.PurchaseRate = item.Rate;

            if (item.PurAmount == 0 && item.Amount != 0)
                item.PurAmount = item.Amount;

            if (string.IsNullOrWhiteSpace(item.Shape) && !string.IsNullOrWhiteSpace(item.ShapeId))
                item.Shape = item.ShapeId;

            if (string.IsNullOrWhiteSpace(item.Color) && !string.IsNullOrWhiteSpace(item.ColorId))
                item.Color = item.ColorId;

            if (string.IsNullOrWhiteSpace(item.Clarity) && !string.IsNullOrWhiteSpace(item.ClarityId))
                item.Clarity = item.ClarityId;
        }

        private static void NormalizeLotDetail(SolitaireConsignmentInLotDetailDto lot)
        {
            if (lot == null) return;

            if (lot.Weight == 0 && float.TryParse(lot.Carats, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var carats) && carats != 0)
                lot.Weight = carats;

            if (lot.MRP == 0 && float.TryParse(lot.PurchaseMRP, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var mrp) && mrp != 0)
                lot.MRP = mrp;

            if (string.IsNullOrWhiteSpace(lot.Shape) && !string.IsNullOrWhiteSpace(lot.ShapeId))
                lot.Shape = lot.ShapeId;

            if (string.IsNullOrWhiteSpace(lot.Color) && !string.IsNullOrWhiteSpace(lot.ColorId))
                lot.Color = lot.ColorId;

            if (string.IsNullOrWhiteSpace(lot.Clarity) && !string.IsNullOrWhiteSpace(lot.ClarityId))
                lot.Clarity = lot.ClarityId;
        }

        public async Task<SolitaireConsignmentInGridViewDto> AddItemInGridAsync(
            SolitaireConsignmentInAddLotRequest item,
            string companyId)
        {
            double totalCarats = 0;
            double totalAmount = 0;
            double totalMRPAmount = 0;
            double totalAmountWithoutless = 0;
            string alert = "";

            float tempCarat = item.Carat;
            string carat3D = Convert.ToDouble(tempCarat).ToString("N3");
            float carat = float.Parse(carat3D);

            if (carat <= 0)
                throw new Exception("Carat is invalid.");

            float purRate = item.Rate;
            float back = item.Back;
            float mrp = item.MRP;

            if (purRate == 0 && back == 0)
                throw new Exception("Rate and Discount both can not be 0.");

            if (purRate != 0 && mrp != 0)
                back = (((purRate * 100) / mrp) - 100);

            float amount = carat * purRate;

            float l1 = item.Less1 != 0 ? amount * (100 + item.Less1) / 100 : amount;
            float l2 = item.Less2 != 0 ? l1 * (100 + item.Less2) / 100 : l1;
            float l3 = item.Less3 != 0 ? l2 * (100 + item.Less3) / 100 : l2;

            var row = new SolitaireConsignmentInItemDto
            {
                LotNo = item.LotNo,
                ItemName = item.ItemName,
                Weight = carat,
                Pcs = item.Pcs,
                MRP = mrp,
                PurchaseRate = purRate,
                PurchaseBack = back,
                Less1 = item.Less1,
                Less2 = item.Less2,
                Less3 = item.Less3,
                PurAmount = l3,
                MemoTransCode = 0,
                RtnCarats = item.RtnCarats,
                ReturnCarats = item.RtnCarats
            };

            var list = item.Items ?? new List<SolitaireConsignmentInItemDto>();
            bool isEdit = item.editIndex >= 0 && item.editIndex < list.Count;

            if (isEdit)
            {
                list[item.editIndex] = row;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(item.LotNo) &&
                    list.Any(x => string.Equals(x.LotNo, item.LotNo, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception($"Lot number {item.LotNo} already exists.");
                }

                list.Add(row);
            }

            foreach (var r in list)
            {
                double weight = Convert.ToDouble(r.Weight.ToString("N3"));
                totalCarats += weight;
                totalAmount += r.PurAmount;
                totalMRPAmount += weight * r.MRP;
                totalAmountWithoutless += weight * r.PurchaseRate;
            }

            return new SolitaireConsignmentInGridViewDto
            {
                TotalCarats = Math.Round(totalCarats, 3),
                TotalAmount = Math.Round(totalAmount, 2),
                TotalMRPAmount = Math.Round(totalMRPAmount, 3),
                TotalAmountWithoutless = Math.Round(totalAmountWithoutless, 3),
                Alert = alert,
                ExcelData = list
            };
        }

        public async Task<(bool Success, string Message)> AddAsync(
            SolitaireConsignmentInHeaderDto salePur,
            List<SolitaireConsignmentInItemDto> items,
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
                        string InvnoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''MemoIn'' ");
                        int.TryParse(InvnoStr, out MaxInvNo);
                        MaxInvNo = MaxInvNo + 1;

                        // Consignment In: Outstanding=false, StockEffected=true (MVC hardcoded)
                        bool outstanding = false;
                        bool stockEffected = true;

                        await InsertSalePurAsync(
                            conn, trans, MaxInvNo, salePur, Curcode, outstanding, stockEffected,
                            usercode, MCurBaseRate, MCurFinalAmt, SCurBaseRate, SCurFinalAmt,
                            FinalMAmount, userId, companyId);

                        string MaxSPCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "SPCode");
                        int MaxSPCode = 0;
                        int.TryParse(MaxSPCodeStr, out MaxSPCode);

                        await ProcessItemsAsync(
                            conn, trans, salePur, items, MaxSPCode, Curcode,
                            MCurBaseRate, SCurBaseRate, MCurOperator, SCurOperator,
                            MCurCode, SCurCode, stockEffected, usercode, userId, companyId,
                            allowGrading: true);

                        // NO JV entries for Consignment In
                        trans.Commit();
                        return (true, "consignment Record Saved.");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return (false, ex.Message);
                    }
                }
            }
        }

        public async Task<(bool Success, string Message)> EditAsync(
            SolitaireConsignmentInHeaderDto salePur,
            List<SolitaireConsignmentInItemDto> items,
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
                        // No grading on edit
                        salePur.chkGrading = false;

                        int Code = salePur.Code;
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

                        bool outstanding = false;
                        bool stockEffected = true;

                        int invNo = 0;
                        int.TryParse(salePur.InvNo, out invNo);

                        await UpdateSalePurAsync(
                            conn, trans, Code, invNo, salePur, Curcode, outstanding, stockEffected,
                            usercode, MCurBaseRate, MCurFinalAmt, SCurBaseRate, SCurFinalAmt,
                            FinalMAmount, userId, companyId);

                        int MaxSPCode = Code;

                        using (var cmd = new SqlCommand("SP_DeleteSalePurTran", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@InvCode", MaxSPCode));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        await ProcessItemsAsync(
                            conn, trans, salePur, items, MaxSPCode, Curcode,
                            MCurBaseRate, SCurBaseRate, MCurOperator, SCurOperator,
                            MCurCode, SCurCode, stockEffected, usercode, userId, companyId,
                            allowGrading: false);

                        // NO JV for Consignment In

                        using (var cmd = new SqlCommand("SP_UpdateItemMasterAtEditPrachase", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@InvCode", MaxSPCode));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        trans.Commit();
                        return (true, "consignment Record updated.");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return (false, ex.Message);
                    }
                }
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string payRecCountStr;
                        using (var cmd = new SqlCommand("GetPayRecCount", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Where", " And InvCode = " + id));
                            var result = await cmd.ExecuteScalarAsync();
                            payRecCountStr = result?.ToString() ?? "0";
                        }

                        int payRecCount = 0;
                        int.TryParse(payRecCountStr, out payRecCount);
                        if (payRecCount > 0)
                            return (false, "Here are Payments entries against This Invoice. So Invoice cannot be Deleted.");

                        using (var cmd = new SqlCommand("GetSalepurCode", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Where", " AND Code = " + id));
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                if (!await reader.ReadAsync())
                                    return (false, "Record not found.");
                            }
                        }

                        // Consignment In delete: SP_DeleteSalePur + SP_DeleteSalePurTran only (no JV, no memo updates)
                        using (var cmd = new SqlCommand("SP_DeleteSalePur", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@code", id));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        using (var cmd = new SqlCommand("SP_DeleteSalePurTran", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@InvCode", id));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        trans.Commit();
                        return (true, "Record Deleted.");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return (false, ex.Message);
                    }
                }
            }
        }

        #region Private insert/update helpers

        private async Task InsertSalePurAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int maxInvNo,
            SolitaireConsignmentInHeaderDto salePur,
            int curCode,
            bool outstanding,
            bool stockEffected,
            int usercode,
            float mCurBaseRate,
            double mCurFinalAmt,
            float sCurBaseRate,
            double sCurFinalAmt,
            float finalMAmount,
            string userId,
            string companyId)
        {
            using (var cmd = new SqlCommand("SP_InsertSalePur", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@invno", maxInvNo));
                cmd.Parameters.Add(new SqlParameter("@billno", salePur.BillNo ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@trntype", "MemoIn"));
                cmd.Parameters.Add(new SqlParameter("@invtype", salePur.InvType ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@category", salePur.Category ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@trndate", salePur.TrnDate));
                cmd.Parameters.Add(new SqlParameter("@duedays", salePur.DueDays));
                cmd.Parameters.Add(new SqlParameter("@duedate", salePur.DueDate));
                cmd.Parameters.Add(new SqlParameter("@partycode", salePur.PartyCode));
                cmd.Parameters.Add(new SqlParameter("@SellerCode", (object)0));
                cmd.Parameters.Add(new SqlParameter("@term", salePur.Term ?? ""));
                cmd.Parameters.Add(new SqlParameter("@countrycode", (object)0));
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
                cmd.Parameters.Add(new SqlParameter("@FinalAmount", Math.Round(finalMAmount, 4)));
                cmd.Parameters.Add(new SqlParameter("@BaseRate", Math.Round(salePur.BaseRate, 4)));
                cmd.Parameters.Add(new SqlParameter("@Disc", salePur.InvDisc ?? ""));
                cmd.Parameters.Add(new SqlParameter("@CurCode", curCode.ToString()));
                cmd.Parameters.Add(new SqlParameter("@Outstanding", outstanding));
                cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                cmd.Parameters.Add(new SqlParameter("@Brokerage", false));
                cmd.Parameters.Add(new SqlParameter("@Commission", false));
                cmd.Parameters.Add(new SqlParameter("@Cheque", false));
                cmd.Parameters.Add(new SqlParameter("@usercode", usercode));
                cmd.Parameters.Add(new SqlParameter("@mastercurrencyrate", Math.Round(mCurBaseRate, 4)));
                cmd.Parameters.Add(new SqlParameter("@mastercurrencyfinalamount", Math.Round(mCurFinalAmt, 4)));
                cmd.Parameters.Add(new SqlParameter("@stockcurrencyrate", Math.Round(sCurBaseRate, 4)));
                cmd.Parameters.Add(new SqlParameter("@stockcurrencyfinalamount", Math.Round(sCurFinalAmt, 4)));
                cmd.Parameters.Add(new SqlParameter("@ExpPartyCode", salePur.ExpPartyCode));
                cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.DueDate));
                cmd.Parameters.Add(new SqlParameter("@DivideDays", salePur.DivideDays));
                cmd.Parameters.Add(new SqlParameter("@InterestPer", salePur.InterestPer));
                cmd.Parameters.Add(new SqlParameter("@VatPer", salePur.VatPer));
                cmd.Parameters.Add(new SqlParameter("@VatAmount", salePur.VatAmount));
                cmd.Parameters.Add(new SqlParameter("@invdescription", salePur.InvDisc ?? ""));
                cmd.Parameters.Add(new SqlParameter("@bankname", (object)0));
                cmd.Parameters.Add(new SqlParameter("@subtype", salePur.SubType ?? ""));
                cmd.Parameters.Add(new SqlParameter("@ShipVia", salePur.ShipVia ?? ""));
                cmd.Parameters.Add(new SqlParameter("@deliverto", salePur.DeliverTo ?? ""));
                cmd.Parameters.Add(new SqlParameter("@Tracking", salePur.Tracking ?? ""));
                cmd.Parameters.Add(new SqlParameter("@SalesPerson", ""));
                cmd.Parameters.Add(new SqlParameter("@Attendee", ""));
                cmd.Parameters.Add(new SqlParameter("@labcharges", salePur.LabCharges));
                cmd.Parameters.Add(new SqlParameter("@freightcharges", salePur.FreightCharges));
                cmd.Parameters.Add(new SqlParameter("@duedays1", (object)0));
                cmd.Parameters.Add(new SqlParameter("@duedays2", (object)0));
                cmd.Parameters.Add(new SqlParameter("@CGSTPer", salePur.CGSTPer));
                cmd.Parameters.Add(new SqlParameter("@CGSTAmount", salePur.CGSTAmt));
                cmd.Parameters.Add(new SqlParameter("@SGSTPer", salePur.SGSTPer));
                cmd.Parameters.Add(new SqlParameter("@SGSTAmount", salePur.SGSTAmt));
                cmd.Parameters.Add(new SqlParameter("@IGSTPer", salePur.IGSTPer));
                cmd.Parameters.Add(new SqlParameter("@IGSTAmount", salePur.IGSTAmt));
                cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
                cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
                cmd.Parameters.Add(new SqlParameter("@netMamount", salePur.NetMAmount));
                cmd.Parameters.Add(new SqlParameter("@FinalPayRecAmount", salePur.FinalMPayRecAmount));
                cmd.Parameters.Add(new SqlParameter("@Address", (object)1));
                cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateSalePurAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int code,
            int invNo,
            SolitaireConsignmentInHeaderDto salePur,
            int curCode,
            bool outstanding,
            bool stockEffected,
            int usercode,
            float mCurBaseRate,
            double mCurFinalAmt,
            float sCurBaseRate,
            double sCurFinalAmt,
            float finalMAmount,
            string userId,
            string companyId)
        {
            using (var cmd = new SqlCommand("SP_UpdateSalePur", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@code", code));
                cmd.Parameters.Add(new SqlParameter("@invno", invNo));
                cmd.Parameters.Add(new SqlParameter("@ModifyBy", userId ?? ""));
                cmd.Parameters.Add(new SqlParameter("@BillNo", salePur.BillNo ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@TrnType", "MemoIn"));
                cmd.Parameters.Add(new SqlParameter("@invtype", salePur.InvType ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@SubType", salePur.SubType ?? ""));
                cmd.Parameters.Add(new SqlParameter("@BankID", (object)0));
                cmd.Parameters.Add(new SqlParameter("@Category", salePur.Category ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Trndate", salePur.TrnDate));
                cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.DueDate));
                cmd.Parameters.Add(new SqlParameter("@duedays", salePur.DueDays));
                cmd.Parameters.Add(new SqlParameter("@duedate", salePur.DueDate));
                cmd.Parameters.Add(new SqlParameter("@DivideDays", salePur.DivideDays));
                cmd.Parameters.Add(new SqlParameter("@partycode", salePur.PartyCode));
                cmd.Parameters.Add(new SqlParameter("@SellerCode", (object)0));
                cmd.Parameters.Add(new SqlParameter("@countrycode", (object)0));
                cmd.Parameters.Add(new SqlParameter("@brokercode", salePur.BrokerCode ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@brokerper", salePur.BrokerPer));
                cmd.Parameters.Add(new SqlParameter("@BrokerAmount", salePur.BrokerAmount));
                cmd.Parameters.Add(new SqlParameter("@Adatcode", salePur.AdatCode ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Adatper", salePur.AdatPer));
                cmd.Parameters.Add(new SqlParameter("@AdatAmount", salePur.AdatAmount));
                cmd.Parameters.Add(new SqlParameter("@totcts", salePur.TotCts));
                cmd.Parameters.Add(new SqlParameter("@totamount", Math.Round(salePur.TotAmount, 4)));
                cmd.Parameters.Add(new SqlParameter("@netamount", Math.Round(salePur.NetAmount, 4)));
                cmd.Parameters.Add(new SqlParameter("@NetMAmount", salePur.NetMAmount));
                cmd.Parameters.Add(new SqlParameter("@plusminamount", Math.Round(salePur.PlusMinusAmount, 4)));
                cmd.Parameters.Add(new SqlParameter("@RoundAmount", Math.Round(salePur.RoundAmount, 4)));
                cmd.Parameters.Add(new SqlParameter("@finalamount", Math.Round(finalMAmount, 4)));
                cmd.Parameters.Add(new SqlParameter("@FinalPayRecAmount", salePur.FinalMPayRecAmount));
                cmd.Parameters.Add(new SqlParameter("@baserate", Math.Round(salePur.BaseRate, 4)));
                cmd.Parameters.Add(new SqlParameter("@ExpPartyCode", salePur.ExpPartyCode));
                cmd.Parameters.Add(new SqlParameter("@ShipVia", salePur.ShipVia ?? ""));
                cmd.Parameters.Add(new SqlParameter("@Tracking", salePur.Tracking ?? ""));
                cmd.Parameters.Add(new SqlParameter("@SalesPerson", ""));
                cmd.Parameters.Add(new SqlParameter("@Attendee", ""));
                cmd.Parameters.Add(new SqlParameter("@disc", salePur.InvDisc ?? ""));
                cmd.Parameters.Add(new SqlParameter("@CurCode", curCode.ToString()));
                cmd.Parameters.Add(new SqlParameter("@OutStanding", outstanding));
                cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                cmd.Parameters.Add(new SqlParameter("@Brokerage", false));
                cmd.Parameters.Add(new SqlParameter("@Commission", false));
                cmd.Parameters.Add(new SqlParameter("@Cheque", false));
                cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
                cmd.Parameters.Add(new SqlParameter("@term", salePur.Term ?? ""));
                cmd.Parameters.Add(new SqlParameter("@InterestPer", salePur.InterestPer));
                cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(mCurBaseRate, 4)));
                cmd.Parameters.Add(new SqlParameter("@MasterCurrencyFinalAmount", Math.Round(mCurFinalAmt, 4)));
                cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(sCurBaseRate, 4)));
                cmd.Parameters.Add(new SqlParameter("@StockCurrencyFinalAmount", Math.Round(sCurFinalAmt, 4)));
                cmd.Parameters.Add(new SqlParameter("@VatPer", salePur.VatPer));
                cmd.Parameters.Add(new SqlParameter("@VatAmount", salePur.VatAmount));
                cmd.Parameters.Add(new SqlParameter("@CGSTPer", salePur.CGSTPer));
                cmd.Parameters.Add(new SqlParameter("@CGSTAmount", salePur.CGSTAmt));
                cmd.Parameters.Add(new SqlParameter("@SGSTPer", salePur.SGSTPer));
                cmd.Parameters.Add(new SqlParameter("@SGSTAmount", salePur.SGSTAmt));
                cmd.Parameters.Add(new SqlParameter("@IGSTPer", salePur.IGSTPer));
                cmd.Parameters.Add(new SqlParameter("@IGSTAmount", salePur.IGSTAmt));
                cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task ProcessItemsAsync(
            SqlConnection conn,
            SqlTransaction trans,
            SolitaireConsignmentInHeaderDto salePur,
            List<SolitaireConsignmentInItemDto> items,
            int maxSPCode,
            int curCode,
            float mCurBaseRate,
            float sCurBaseRate,
            string mCurOperator,
            string sCurOperator,
            double mCurCode,
            double sCurCode,
            bool stockEffected,
            int usercode,
            string userId,
            string companyId,
            bool allowGrading)
        {
            string generateLotno = await GetSettingValueAsync(conn, trans, "GenerateSolitairePurchaseLotno");
            string city = await GetSettingValueAsync(conn, trans, "StoneCity");

            foreach (var im in items)
            {
                // Consignment In does not convert from memo — force MemoTransCode=0
                im.MemoTransCode = 0;

                float AskingRate = im.AskingRate;
                var askAmtRes = GetConvertAmount(curCode, AskingRate, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);
                double AkingSCurRate = askAmtRes.SCurAmt;
                double AskingSAmount = AkingSCurRate * im.Weight;

                float ActualRate = im.ActualRate;
                var actAmtRes = GetConvertAmount(curCode, ActualRate, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);
                double ActualSCurRate = actAmtRes.SCurAmt;
                double ActualSAmount = ActualSCurRate * im.Weight;

                float PurRate = im.PurchaseRate;
                var purRateRes = GetConvertAmount(curCode, PurRate, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);
                double PurSCurRate = purRateRes.SCurAmt;

                float PurAmount = im.PurAmount;
                var purAmtRes = GetConvertAmount(curCode, PurAmount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);
                double PurMCurAmt = purAmtRes.MCurAmt;
                double PurSCurAmt = purAmtRes.SCurAmt;

                float B2BRate = im.B2BRate;
                var b2bRateRes = GetConvertAmount(curCode, B2BRate, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);
                double B2BRateSCur = b2bRateRes.SCurAmt;

                float B2CRate = im.B2CRate;
                var b2cRateRes = GetConvertAmount(curCode, B2CRate, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);
                double B2CRateSCur = b2cRateRes.SCurAmt;

                string LotAlready = await ExecuteCommonDataScalarAsync(conn, trans, "LotExist", " AND LotNo = ''" + im.LotNo + "''");

                string ItemSrno = await ExecuteCommonDataScalarAsync(conn, trans, "MaxItemSrNo");
                int MaxItemSrno = 0;
                int.TryParse(ItemSrno, out MaxItemSrno);
                MaxItemSrno = MaxItemSrno + 1;

                string lotStr;
                if (string.IsNullOrWhiteSpace(im.LotNo))
                    lotStr = generateLotno + MaxItemSrno.ToString();
                else
                    lotStr = im.LotNo;

                im.LotNo = lotStr;

                float rtnCarats = im.RtnCarats != 0 ? im.RtnCarats : im.ReturnCarats;

                if (LotAlready.ToString() == "0")
                {
                    int singlerPcs = (im.Pcs <= 1) ? 1 : 0;

                    DateTime parsedDate;
                    string dateString = DateTime.TryParse(im.ReportDate?.ToString(), out parsedDate)
                        ? parsedDate.ToString("yyyy-MM-dd")
                        : string.Empty;

                    using (var cmd = new SqlCommand("SP_ItemMaster", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ItemCode", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@ItemSrno", MaxItemSrno));
                        cmd.Parameters.Add(new SqlParameter("@LotNo", im.LotNo));
                        cmd.Parameters.Add(new SqlParameter("@ItemName", im.ItemName ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@ItemGroupId", im.ItemGroup ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@Category", salePur.Category ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@ProductType", salePur.Category ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                        cmd.Parameters.Add(new SqlParameter("@Weight", Convert.ToDouble(im.Weight.ToString("N3"))));
                        cmd.Parameters.Add(new SqlParameter("@CaratCL", Convert.ToDouble(im.Weight.ToString("N3"))));
                        cmd.Parameters.Add(new SqlParameter("@SieveId", ""));
                        cmd.Parameters.Add(new SqlParameter("@ShapeId", im.Shape ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@ColorId", im.Color ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@SizeId", im.Size ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@ClarityId", im.Clarity ?? ""));
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
                        cmd.Parameters.Add(new SqlParameter("@FancyColor", im.FancyColor ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@FancyColorIntensity", im.FancyColorIntensity ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@FancyColorOvertone", im.FancyColorOvertone ?? ""));
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
                        cmd.Parameters.Add(new SqlParameter("@PublicRemarks", im.MemberRemarks ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@PrivateRemarks", ""));
                        cmd.Parameters.Add(new SqlParameter("@AdminRemarks", ""));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseRate", Math.Round(PurSCurRate, 4)));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseBack", im.PurchaseBack));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseCarats", Convert.ToDouble(im.Weight.ToString("N3"))));
                        cmd.Parameters.Add(new SqlParameter("@SPPurchaseRate", Math.Round(im.PurchaseRate, 4)));
                        cmd.Parameters.Add(new SqlParameter("@SPPurchaseBack", im.PurchaseBack));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseMRP", im.MRP));
                        cmd.Parameters.Add(new SqlParameter("@PDollorRate", Math.Round(salePur.BaseRate, 4)));
                        cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(mCurBaseRate, 4)));
                        cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", Math.Round(PurMCurAmt, 4)));
                        cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(sCurBaseRate, 4)));
                        cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", Math.Round(PurSCurAmt, 4)));
                        cmd.Parameters.Add(new SqlParameter("@AdatPer", Math.Round(salePur.AdatPer, 4)));
                        cmd.Parameters.Add(new SqlParameter("@Brokerper", Math.Round(salePur.BrokerPer, 4)));
                        cmd.Parameters.Add(new SqlParameter("@LabReportDate", dateString));
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
                        cmd.Parameters.Add(new SqlParameter("@MfgPacketID", im.MfgPacketID ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@RoughId", ""));
                        cmd.Parameters.Add(new SqlParameter("@MinesId", ""));
                        cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                        cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                        cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                        cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
                        cmd.Parameters.Add(new SqlParameter("@City", city ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@ImagePath", im.ImagePath ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@VideoPath", im.VideoPath ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@PagePath", im.PagePath ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@CountryOfOrigin", im.CountryOfOrigin ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@CountryOfMfg", im.CountryOfMfg ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                string ItemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "ItemCode", " AND LotNo = ''" + im.LotNo + "''");
                int MaxItemCode = 0;
                int.TryParse(ItemCodeStr, out MaxItemCode);

                string itemName = !string.IsNullOrWhiteSpace(im.ItemName)
                    ? im.ItemName
                    : $"{im.LotNo} {im.Shape} {im.Size} {im.Color} {im.Clarity}";

                using (var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPdate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@TrnType", "MemoIn"));
                    cmd.Parameters.Add(new SqlParameter("@InvType", salePur.InvType ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@InvCode", maxSPCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemCode", MaxItemCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemName", itemName));
                    cmd.Parameters.Add(new SqlParameter("@Carats", float.Parse(im.Weight.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                    cmd.Parameters.Add(new SqlParameter("@Rate", Convert.ToDouble(im.PurchaseRate.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@MRP", im.MRP));
                    cmd.Parameters.Add(new SqlParameter("@PMRPDown", Convert.ToDouble(im.PurchaseBack.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ReturnCarats", Convert.ToDouble(rtnCarats.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@IssueCarats", 0d));
                    cmd.Parameters.Add(new SqlParameter("@amount", Convert.ToDouble(im.PurAmount.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ExportNo", ""));
                    cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                    cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                    cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                    cmd.Parameters.Add(new SqlParameter("@CurCode", salePur.CurCode.ToString()));
                    cmd.Parameters.Add(new SqlParameter("@memotrancode", 0f));
                    cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                    cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
                    cmd.Parameters.Add(new SqlParameter("@spcarats", 0f));
                    cmd.Parameters.Add(new SqlParameter("@FItemCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@TItemCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@FItemRate", 0.0));
                    cmd.Parameters.Add(new SqlParameter("@TItemRate", 0.0));
                    cmd.Parameters.Add(new SqlParameter("@FromSuratCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@AssorterCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@RoughCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@MemoRemark", ""));
                    cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
                    cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(mCurBaseRate, 4)));
                    cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", Convert.ToDouble(PurMCurAmt.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(sCurBaseRate, 4)));
                    cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", Convert.ToDouble(PurSCurAmt.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
                    cmd.Parameters.Add(new SqlParameter("@PShape", ""));
                    cmd.Parameters.Add(new SqlParameter("@PSize", ""));
                    cmd.Parameters.Add(new SqlParameter("@PSieve", ""));
                    cmd.Parameters.Add(new SqlParameter("@PColor", ""));
                    cmd.Parameters.Add(new SqlParameter("@PClarity", ""));
                    cmd.Parameters.Add(new SqlParameter("@COGS", 0.0f));
                    cmd.Parameters.Add(new SqlParameter("@StockType", ""));
                    cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                    await cmd.ExecuteNonQueryAsync();
                }

                // NO SP_UpdateMemo for Consignment In

                if (im.Size == null)
                {
                    using (var cmd = new SqlCommand("SP_UpdateSizeid", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@LotNo", im.LotNo));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                if (allowGrading && salePur.chkGrading)
                {
                    using (var cmd = new SqlCommand("SP_InsertMultiGrading", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ItemCode", MaxItemCode));
                        cmd.Parameters.Add(new SqlParameter("@itemsrno", MaxItemSrno));
                        cmd.Parameters.Add(new SqlParameter("@LotNo", im.LotNo));
                        cmd.Parameters.Add(new SqlParameter("@MfgSeriesID", im.MfgSeriesID ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@MfgPacketID", im.MfgPacketID ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@ECutCode", im.Shape ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@GraderName", "Selected"));
                        cmd.Parameters.Add(new SqlParameter("@EColorCode", im.Color ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@EClarityCode", im.Clarity ?? ""));
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
                        cmd.Parameters.Add(new SqlParameter("@Status", "PURCHASE"));
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
                        cmd.Parameters.Add(new SqlParameter("@CertExpense", (object)0));
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
                        cmd.Parameters.Add(new SqlParameter("@ShapeName", im.Shape ?? ""));
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
                        cmd.Parameters.Add(new SqlParameter("@SieveId", ""));
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
        }

        #endregion

        #region Helpers

        private async Task<string> ExecuteCommonDataScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using (var cmd = new SqlCommand("GetAllCommonData", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Type", type));
                string cleanedWhere = where?.Replace("''", "'") ?? "";
                cmd.Parameters.Add(new SqlParameter("@Where", cleanedWhere));
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
                string cleanedWhere = where?.Replace("''", "'") ?? "";
                cmd.Parameters.Add(new SqlParameter("@Where", cleanedWhere));
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
                cmd.Parameters.Add(new SqlParameter("@Where", $" and Name='{name}'"));
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

        #endregion

        public async Task<SolitaireConsignmentInPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
        {
            var safeInv = (invNo ?? "").Replace("'", "''");
            string where = $"tsp.TrnType = ''MemoIn'' AND tsp.InvNo = ''{safeInv}''";
            string sql = $"EXEC GetSalepurRecordWithDetailsForINV @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectRawSql<PrintInvRow>(sql, companyId)).ToList();
            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var result = new SolitaireConsignmentInPrintViewDto
            {
                InvNo = header.InvNo,
                BillNo = header.BillNo,
                TrnDate = header.TrnDate.HasValue ? header.TrnDate.Value.ToString("dd-MMM-yyyy") : null,
                DueDays = header.DueDays,
                PartyName = header.PartyName,
                AddressLine1 = header.AddressLine1,
                AddressLine2 = header.AddressLine2,
                AddressLine3 = header.AddressLine3,
                Items = new List<SolitaireConsignmentInPrintItemDto>()
            };

            foreach (var dr in rows)
            {
                result.Items.Add(new SolitaireConsignmentInPrintItemDto
                {
                    StockNo = dr.LotNo,
                    Description = dr.ItemName,
                    Qty = dr.Pcs,
                    Weight = dr.Carats,
                    PricePerCarat = dr.Rate,
                    Amount = dr.MasterCurrencyAmount
                });
            }

            return result;
        }

        private class PrintInvRow
        {
            public string? InvNo { get; set; }
            public string? BillNo { get; set; }
            public DateTime? TrnDate { get; set; }
            public short DueDays { get; set; }
            public string? PartyName { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public string? AddressLine3 { get; set; }
            public string? LotNo { get; set; }
            public string? ItemName { get; set; }
            public int Pcs { get; set; }
            public double Carats { get; set; }
            public double Rate { get; set; }
            public double MasterCurrencyAmount { get; set; }
        }
    }
}
