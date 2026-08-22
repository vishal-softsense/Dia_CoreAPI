using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction
{
    public class MixPurchaseRepository : IMixPurchaseRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public MixPurchaseRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<List<MixPurchaseInvListDto>> GetListAsync(int pageNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@PageNo", pageNo }
            };

            var result = (await _sqlDataAccess.SelectDataFromSP<MixPurchaseInvListDto>(
                "GetMixPurchaseInvDataUsingPageNo",
                parameters,
                companyId)).ToList();

            foreach (var row in result)
            {
                // SP may return InvNo as HTML anchor; keep clean invoice # for React
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

            // Prefer inner text of first tag pair, e.g. <a ...>243</a>
            var open = text.IndexOf('>');
            var close = text.LastIndexOf('<');
            if (open >= 0 && close > open)
                text = text.Substring(open + 1, close - open - 1);

            text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", string.Empty);
            return System.Net.WebUtility.HtmlDecode(text).Trim();
        }

        public async Task<MixPurchasePageDataDto> GetPageDataAsync(string companyId)
        {
            var transactionData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "AllMasterDataForPurchase",
                new Dictionary<string, object>(),
                companyId);

            var list = transactionData.ToList();

            var itemMasterData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "GetAllMasterDataForItemMaster",
                new Dictionary<string, object>(),
                companyId);
            var itemList = itemMasterData.ToList();

            var defaultCurrencyCode = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'DefaultCurrency'",
                new { },
                companyId);

            return new MixPurchasePageDataDto
            {
                Type = MapBasic(list.Where(x => x.Type == "Type")),
                SubType = MapBasic(list.Where(x => x.Type == "SubType")),
                Category = MapBasic(list.Where(x => x.Type == "Category")),
                CurrencyList = MapBasic(list.Where(x => x.Type == "Currency")),
                Shape = MapBasic(itemList.Where(x => x.Type == "Shape")),
                Color = MapBasic(itemList.Where(x => x.Type == "Color")),
                Clarity = MapBasic(itemList.Where(x => x.Type == "Clarity")),
                Size = MapBasic(itemList.Where(x => x.Type == "Size")),
                Sieve = MapBasic(itemList.Where(x => x.Type == "Sieve")),
                ProductType = MapBasic(itemList.Where(x => x.Type == "ProductType")),
                ItemGroup = MapBasic(itemList.Where(x => x.Type == "ItemGroup")),
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

        public async Task<MixPurchaseDetailDto?> GetDetailAsync(int id, string companyId)
        {
            var headerParams = new Dictionary<string, object>
            {
                { "@Where", $" tsp.TrnType = 'CPUR' AND Code = {id}" }
            };

            var header = await _sqlDataAccess.SelectDataFromSPSingle<MixPurchaseHeaderDto>(
                "GetSalepurRecord",
                headerParams,
                companyId);

            if (header == null)
                return null;

            var itemParams = new Dictionary<string, object>
            {
                { "@Where", $" AND InvCode = {id}" }
            };

            var items = (await _sqlDataAccess.SelectDataFromSP<MixPurchaseItemDto>(
                "GetSalepurTran",
                itemParams,
                companyId)).ToList();

            foreach (var item in items)
                NormalizePurchaseItem(item);

            return new MixPurchaseDetailDto
            {
                Header = header,
                Items = items
            };
        }

        public async Task<List<MixPurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Type", "MixPur" },
                { "@Where", $" AND LotNo = '{lotNo}' " }
            };

            var result = (await _sqlDataAccess.SelectDataFromSP<MixPurchaseLotDetailDto>(
                "GetLotNoDataForAll",
                parameters,
                companyId)).ToList();

            foreach (var lot in result)
                NormalizeLotDetail(lot);

            return result;
        }

        private static void NormalizePurchaseItem(MixPurchaseItemDto item)
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

        private static void NormalizeLotDetail(MixPurchaseLotDetailDto lot)
        {
            if (lot == null) return;

            // GetLotNoDataForAll Type=MixPur returns Carats / PurchaseMRP (MVC LotNoData)
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

        public async Task<List<MixConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Party", partyCode ?? "" },
                { "@invno", memoNo ?? "" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<MixConsignmentRowDto>(
                "Sp_SelectMixConsignmentForMixPurchase",
                parameters,
                companyId);

            return result.ToList();
        }

        public async Task<MixPurchaseGridViewDto> AddItemInGridAsync(
            MixPurchaseAddLotRequest item,
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
            float back = item.Back != 0 ? item.Back : item.PurchaseBack;
            float mrp = item.MRP;
            float issueCarats = item.IssueCarats != 0 ? item.IssueCarats : carat;
            float returnCarats = item.ReturnCarats != 0 ? item.ReturnCarats : item.RtnCarats;
            bool isJewellery = string.Equals(item.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase);

            if (purRate == 0 && back == 0)
                throw new Exception("Rate and Discount both can not be 0.");

            if (purRate != 0 && issueCarats != 0)
                back = (((purRate * 100) / issueCarats) - 100);

            float amount = isJewellery ? item.Pcs * purRate : carat * purRate;
            if (item.Amount != 0)
                amount = item.Amount;

            float l1 = item.Less1 != 0 ? amount * (100 + item.Less1) / 100 : amount;
            float l2 = item.Less2 != 0 ? l1 * (100 + item.Less2) / 100 : l1;
            float l3 = item.Less3 != 0 ? l2 * (100 + item.Less3) / 100 : l2;

            var pur = new MixPurchaseItemDto
            {
                LotNo = item.LotNo,
                ItemName = item.ItemName,
                Shape = item.ShapeId,
                ShapeId = item.ShapeId,
                PShape = item.ShapeId,
                Size = item.SizeId,
                PSize = item.SizeId,
                Color = item.ColorId,
                ColorId = item.ColorId,
                PColor = item.ColorId,
                Clarity = item.ClarityId,
                ClarityId = item.ClarityId,
                PClarity = item.ClarityId,
                Sieve = item.SieveId,
                SieveId = item.SieveId,
                PSieve = item.SieveId,
                Weight = item.Weight != 0 ? item.Weight : carat,
                Carats = item.Weight != 0 ? item.Weight : carat,
                IssueCarats = issueCarats,
                Pcs = item.Pcs,
                MRP = mrp,
                PurchaseRate = purRate,
                Rate = purRate,
                PurchaseBack = back,
                Less1 = item.Less1,
                Less2 = item.Less2,
                Less3 = item.Less3,
                PurAmount = l3,
                Amount = l3,
                MemoTransCode = item.MemoTransCode,
                RtnCarats = returnCarats,
                ReturnCarats = returnCarats,
                StockType = string.IsNullOrWhiteSpace(item.StockType) ? "Diamond" : item.StockType,
                SuppLotNo = item.SuppLotNo,
                AskingRate = item.AskingRate,
                ActualRate = item.ActualRate,
                GroupId = item.GroupId,
                StoneType = item.StoneType,
                RMK = item.RMK
            };

            var list = item.Items ?? new List<MixPurchaseItemDto>();
            bool isEdit = item.editIndex >= 0 && item.editIndex < list.Count;

            if (isEdit)
            {
                list[item.editIndex] = pur;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(item.LotNo) &&
                    list.Any(x => string.Equals(x.LotNo, item.LotNo, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception($"Lot number {item.LotNo} already exists.");
                }

                list.Add(pur);
            }

            foreach (var row in list)
            {
                double weight = Convert.ToDouble(row.Weight.ToString("N3"));
                totalCarats += weight;
                totalAmount += row.PurAmount;
                totalMRPAmount += weight * row.MRP;
                totalAmountWithoutless += weight * row.PurchaseRate;
            }

            return new MixPurchaseGridViewDto
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
            MixPurchaseHeaderDto salePur,
            List<MixPurchaseItemDto> items,
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
                        string InvnoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''CPUR'' ");
                        int.TryParse(InvnoStr, out MaxInvNo);
                        MaxInvNo = MaxInvNo + 1;

                        bool outstanding = salePur.chkoldpur;
                        bool stockEffected = !salePur.chkoldpur;

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
                            MCurCode, SCurCode, stockEffected, usercode, userId, companyId);

                        if (!salePur.chkoldpur)
                        {
                            await InsertPurchaseJvEntriesAsync(
                                conn, trans, salePur, MaxSPCode, Curcode,
                                MCurBaseRate, SCurBaseRate, MCurOperator, MCurOperatorM, SCurOperatorM,
                                MToSCurRate, MCurCode, usercode, userId, companyId);
                        }

                        trans.Commit();
                        return (true, "Mix Purchase Record Saved.");
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
            MixPurchaseHeaderDto salePur,
            List<MixPurchaseItemDto> items,
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

                        bool outstanding = salePur.chkoldpur;
                        bool stockEffected = !salePur.chkoldpur;

                        int MaxSPCode;

                        if (string.Equals(salePur.Mode, "consignment", StringComparison.OrdinalIgnoreCase))
                        {
                            int MaxInvNo;
                            string InvnoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''CPUR'' ");
                            int.TryParse(InvnoStr, out MaxInvNo);
                            MaxInvNo = MaxInvNo + 1;

                            await InsertSalePurAsync(
                                conn, trans, MaxInvNo, salePur, Curcode, outstanding, stockEffected,
                                usercode, MCurBaseRate, MCurFinalAmt, SCurBaseRate, SCurFinalAmt,
                                FinalMAmount, userId, companyId);

                            string MaxSPCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "SPCode");
                            int.TryParse(MaxSPCodeStr, out MaxSPCode);
                        }
                        else
                        {
                            int invNo = 0;
                            int.TryParse(salePur.InvNo, out invNo);

                            await UpdateSalePurAsync(
                                conn, trans, Code, invNo, salePur, Curcode, outstanding, stockEffected,
                                usercode, MCurBaseRate, MCurFinalAmt, SCurBaseRate, SCurFinalAmt,
                                FinalMAmount, userId, companyId);

                            MaxSPCode = Code;

                            using (var cmd = new SqlCommand("SP_DeleteSalePurTran", conn, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@InvCode", MaxSPCode));
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        await ProcessItemsAsync(
                            conn, trans, salePur, items, MaxSPCode, Curcode,
                            MCurBaseRate, SCurBaseRate, MCurOperator, SCurOperator,
                            MCurCode, SCurCode, stockEffected, usercode, userId, companyId);

                        using (var cmd = new SqlCommand("SP_DeleteJV", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@vcode", MaxSPCode));
                            cmd.Parameters.Add(new SqlParameter("@vtype", "CPUR"));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        if (!salePur.chkoldpur)
                        {
                            await InsertPurchaseJvEntriesAsync(
                                conn, trans, salePur, MaxSPCode, Curcode,
                                MCurBaseRate, SCurBaseRate, MCurOperator, MCurOperatorM, SCurOperatorM,
                                MToSCurRate, MCurCode, usercode, userId, companyId);
                        }

                        using (var cmd = new SqlCommand("SP_UpdateItemMasterAtEditPrachase", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@InvCode", MaxSPCode));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        trans.Commit();
                        return (true, "Mix Purchase Record Updated.");
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
                            return (false, "Payment/Receipt exists against this invoice. Cannot delete.");

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

                        DateTime trnDate = DateTime.Now;
                        using (var cmd = new SqlCommand("SELECT Convert(varchar, TrnDate, 120) FROM TabSalePur WHERE Code = @Code", conn, trans))
                        {
                            cmd.Parameters.Add(new SqlParameter("@Code", id));
                            var dateStr = (await cmd.ExecuteScalarAsync())?.ToString();
                            if (!string.IsNullOrEmpty(dateStr))
                                trnDate = Convert.ToDateTime(dateStr);
                        }

                        var memoRows = new List<(float MemoTransCode, float Weight)>();
                        using (var cmd = new SqlCommand("GetSalepurTran_Sale", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Where", " AND InvCode = " + id));
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    float memo = 0;
                                    float weight = 0;
                                    try
                                    {
                                        int memoOrd = reader.GetOrdinal("MemoTransCode");
                                        if (!reader.IsDBNull(memoOrd))
                                            memo = Convert.ToSingle(reader.GetValue(memoOrd));
                                    }
                                    catch { /* column may be MemoTranCode */ }

                                    if (memo == 0)
                                    {
                                        try
                                        {
                                            int memoOrd = reader.GetOrdinal("MemoTranCode");
                                            if (!reader.IsDBNull(memoOrd))
                                                memo = Convert.ToSingle(reader.GetValue(memoOrd));
                                        }
                                        catch { }
                                    }

                                    try
                                    {
                                        int wOrd = reader.GetOrdinal("Weight");
                                        if (!reader.IsDBNull(wOrd))
                                            weight = Convert.ToSingle(reader.GetValue(wOrd));
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            int wOrd = reader.GetOrdinal("Carats");
                                            if (!reader.IsDBNull(wOrd))
                                                weight = Convert.ToSingle(reader.GetValue(wOrd));
                                        }
                                        catch { }
                                    }

                                    if (memo > 0)
                                        memoRows.Add((memo, weight));
                                }
                            }
                        }

                        foreach (var row in memoRows)
                        {
                            using (var cmd = new SqlCommand("SP_UpdateMemo", conn, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@code", row.MemoTransCode));
                                cmd.Parameters.Add(new SqlParameter("@RtnDate", trnDate));
                                cmd.Parameters.Add(new SqlParameter("@rtncts", 0f));
                                cmd.Parameters.Add(new SqlParameter("@spcts", -float.Parse(row.Weight.ToString("N3"))));
                                cmd.Parameters.Add(new SqlParameter("@Actiontype", "contopurtest"));
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

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

                        using (var cmd = new SqlCommand("SP_DeleteJV", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@vcode", id));
                            cmd.Parameters.Add(new SqlParameter("@vtype", "CPUR"));
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

        public async Task<MixPurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
        {
            // Prefer Code lookup (MVC Mix_Pur/MixPurPrintInv?ID=code); also allow InvNo
            string where;
            if (int.TryParse(invNo, out var codeId))
                where = $"tsp.TrnType = ''CPUR'' AND tsp.Code = {codeId}";
            else
                where = $"tsp.TrnType = ''CPUR'' AND tsp.InvNo = ''{invNo}''";

            string sql = $"EXEC GetMixSalepurRecordWithDetailsForINV @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectRawSql<PrintInvRow>(sql, companyId)).ToList();
            if (rows.Count == 0 && int.TryParse(invNo, out _))
            {
                // Fallback: treat value as InvNo when Code lookup returns nothing
                where = $"tsp.TrnType = ''CPUR'' AND tsp.InvNo = ''{invNo}''";
                sql = $"EXEC GetMixSalepurRecordWithDetailsForINV @Where = '{where}'";
                rows = (await _sqlDataAccess.SelectRawSql<PrintInvRow>(sql, companyId)).ToList();
            }
            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var result = new MixPurchasePrintViewDto
            {
                InvNo = header.InvNo,
                BillNo = header.BillNo,
                TrnDate = header.TrnDate.HasValue ? header.TrnDate.Value.ToString("MM/dd/yyyy") : null,
                DueDays = header.DueDays,
                PartyName = header.PartyName,
                AddressLine1 = header.AddressLine1,
                AddressLine2 = header.AddressLine2,
                AddressLine3 = header.AddressLine3,
                Items = new List<MixPurchasePrintItemDto>()
            };

            foreach (var dr in rows)
            {
                result.Items.Add(new MixPurchasePrintItemDto
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

        public async Task<MixPurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId)
        {
            // MVC uses Code with TrnType 'CPur'
            string where;
            if (int.TryParse(invNo, out var codeId))
                where = $"tsp.TrnType = ''CPur'' AND tsp.Code = {codeId}";
            else
                where = $"tsp.TrnType = ''CPur'' AND tsp.InvNo = {invNo}";

            string sql = $"EXEC [SoliPrintCurStatusInv] @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectRawSql<StatusInvRow>(sql, companyId)).ToList();
            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var invoiceRow = rows.FirstOrDefault(x => string.Equals(x.Type, "Invoice", StringComparison.OrdinalIgnoreCase)) ?? header;

            var result = new MixPurchaseStatusDto
            {
                InvNo = invNo,
                InvDate = invoiceRow.Date,
                Status = invoiceRow.Status,
                Party = header.Party,
                AddLine1 = header.AddLine1,
                AddLine2 = header.AddLine2,
                AddLine3 = header.AddLine3,
                Mobile = header.Mobile,
                CPerson = header.CPerson,
                SalesPerson = header.SalesPerson,
                ShipVia = header.ShipVia,
                Items = new List<MixPurchaseStatusItemDto>()
            };

            foreach (var row in rows)
            {
                result.Items.Add(new MixPurchaseStatusItemDto
                {
                    Type = row.Type,
                    Date = row.Date,
                    Qty = row.Qty != null && row.Qty != 0 ? row.Qty : null,
                    Wt = row.Wt != null && row.Wt != 0 ? row.Wt : null,
                    Description = row.Description,
                    Rate = row.Rate != null && row.Rate != 0 ? row.Rate : null,
                    Tax = row.Tax != null && row.Tax != 0 ? row.Tax : null,
                    Amount = row.Amount != null && row.Amount != 0 ? row.Amount : null,
                    IsPayment = row.IsPayment
                });
            }

            return result;
        }

        #region Private insert/update helpers

        private async Task InsertSalePurAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int maxInvNo,
            MixPurchaseHeaderDto salePur,
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
                cmd.Parameters.Add(new SqlParameter("@trntype", "CPUR"));
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
            MixPurchaseHeaderDto salePur,
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
                cmd.Parameters.Add(new SqlParameter("@TrnType", "CPUR"));
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
            MixPurchaseHeaderDto salePur,
            List<MixPurchaseItemDto> items,
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
            string companyId)
        {
            string generateLotno = await GetSettingValueAsync(conn, trans, "GenerateMixPurchaseLotno");
            string city = await GetSettingValueAsync(conn, trans, "StoneCity");

            foreach (var im in items)
            {
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
                        cmd.Parameters.Add(new SqlParameter("@Weight", (object)0));
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
                    cmd.Parameters.Add(new SqlParameter("@trntype", "CPUR"));
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
                    float issueCarats = im.IssueCarats != 0 ? im.IssueCarats : im.Weight;
                    cmd.Parameters.Add(new SqlParameter("@IssueCarats", Convert.ToDouble(issueCarats.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@amount", Convert.ToDouble(im.PurAmount.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ExportNo", ""));
                    cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                    cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                    cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                    cmd.Parameters.Add(new SqlParameter("@CurCode", salePur.CurCode.ToString()));
                    cmd.Parameters.Add(new SqlParameter("@memotrancode", im.MemoTransCode));
                    cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                    cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
                    float spCarats = im.MemoTransCode != 0 ? float.Parse(im.Weight.ToString("N3")) : 0f;
                    cmd.Parameters.Add(new SqlParameter("@spcarats", spCarats));
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
                    string pShape = im.PShape ?? im.Shape ?? im.ShapeId ?? "";
                    string pSize = im.PSize ?? im.Size ?? "";
                    string pSieve = im.PSieve ?? im.Sieve ?? im.SieveId ?? "";
                    string pColor = im.PColor ?? im.Color ?? im.ColorId ?? "";
                    string pClarity = im.PClarity ?? im.Clarity ?? im.ClarityId ?? "";
                    cmd.Parameters.Add(new SqlParameter("@PShape", pShape));
                    cmd.Parameters.Add(new SqlParameter("@PSize", pSize));
                    cmd.Parameters.Add(new SqlParameter("@PSieve", pSieve));
                    cmd.Parameters.Add(new SqlParameter("@PColor", pColor));
                    cmd.Parameters.Add(new SqlParameter("@PClarity", pClarity));
                    cmd.Parameters.Add(new SqlParameter("@COGS", 0.0f));
                    cmd.Parameters.Add(new SqlParameter("@StockType", im.StockType ?? "Diamond"));
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
                        cmd.Parameters.Add(new SqlParameter("@rtncts", float.Parse(rtnCarats.ToString("N3"))));
                        cmd.Parameters.Add(new SqlParameter("@spcts", float.Parse(im.Weight.ToString("N3"))));
                        cmd.Parameters.Add(new SqlParameter("@Actiontype", "contopurtest"));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                if (im.Size == null)
                {
                    using (var cmd = new SqlCommand("SP_UpdateSizeid", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@LotNo", im.LotNo));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                if (salePur.chkGrading)
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
                        cmd.Parameters.Add(new SqlParameter("@Status", "CPUR"));
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

                if (string.Equals(salePur.Mode, "consignment", StringComparison.OrdinalIgnoreCase))
                {
                    using (var cmd = new SqlCommand("SP_UpdateonsignmentDetails", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Date", salePur.TrnDate));
                        cmd.Parameters.Add(new SqlParameter("@Carats", float.Parse(im.Weight.ToString("N3"))));
                        cmd.Parameters.Add(new SqlParameter("@LotNo", im.LotNo));
                        try
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch
                        {
                            // SP name/params may vary; consignment fill is best-effort
                        }
                    }
                }
            }
        }

        private async Task InsertPurchaseJvEntriesAsync(
            SqlConnection conn,
            SqlTransaction trans,
            MixPurchaseHeaderDto salePur,
            int maxSPCode,
            int curCode,
            float mCurBaseRate,
            float sCurBaseRate,
            string mCurOperator,
            string mCurOperatorM,
            string sCurOperatorM,
            float mToSCurRate,
            double mCurCode,
            int usercode,
            string userId,
            string companyId)
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
            var jvConvert = GetConvertAmount(mCurCode, PurJvAmount, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
            double PurJvMAmount = jvConvert.MCurAmt;
            double PurJvSAmount = jvConvert.SCurAmt;

            float PurchaseJVAMount = 0;
            if (mCurOperator == "*")
                PurchaseJVAMount = mCurBaseRate != 0 ? PurJvAmount / mCurBaseRate : 0f;
            if (mCurOperator == "/")
                PurchaseJVAMount = PurJvAmount * mCurBaseRate;

            await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, PurchaseCode, salePur.PartyCode, Math.Abs(PurchaseJVAMount), curCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, PurJvMAmount, sCurBaseRate, PurJvSAmount, 0, usercode, userId, companyId);

            if ((Convert.ToDouble(salePur.BrokerAmount) != 0) && salePur.BrokerCode != null)
            {
                float BroAmt = salePur.BrokerAmount;
                var broConvert = GetConvertAmount(mCurCode, BroAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, BrokerageCode, salePur.BrokerCode.Value, Math.Abs(BroAmt), mCurCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, broConvert.MCurAmt, sCurBaseRate, broConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if ((Convert.ToDouble(salePur.AdatAmount) != 0) && salePur.AdatCode != null)
            {
                float AdatAmt = salePur.AdatAmount;
                var adatConvert = GetConvertAmount(mCurCode, AdatAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, AdatCode, salePur.AdatCode.Value, AdatAmt, mCurCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, adatConvert.MCurAmt, sCurBaseRate, adatConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.VatAmount) != 0)
            {
                float VatAmt = salePur.VatAmount;
                var vatConvert = GetConvertAmount(mCurCode, VatAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, VatCode, PurchaseCode, VatAmt, mCurCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, vatConvert.MCurAmt, sCurBaseRate, vatConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.SGSTAmt) != 0)
            {
                float SGSTAmt = salePur.SGSTAmt;
                var sgstConvert = GetConvertAmount(mCurCode, SGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, SGSTCode, PurchaseCode, SGSTAmt, mCurCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, sgstConvert.MCurAmt, sCurBaseRate, sgstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.CGSTAmt) != 0)
            {
                float CGSTAmt = salePur.CGSTAmt;
                var cgstConvert = GetConvertAmount(mCurCode, CGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, CGSTCode, PurchaseCode, CGSTAmt, mCurCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, cgstConvert.MCurAmt, sCurBaseRate, cgstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.IGSTAmt) != 0)
            {
                float IGSTAmt = salePur.IGSTAmt;
                var igstConvert = GetConvertAmount(mCurCode, IGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, IGSTCode, PurchaseCode, IGSTAmt, mCurCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, igstConvert.MCurAmt, sCurBaseRate, igstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.RoundAmount) > 0)
            {
                float RoundAmt = salePur.RoundAmount;
                var roundConvert = GetConvertAmount(mCurCode, RoundAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, CashCode, salePur.PartyCode, salePur.RoundAmount, mCurCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, roundConvert.MCurAmt, sCurBaseRate, roundConvert.SCurAmt, 0, usercode, userId, companyId);
            }
            else if (Convert.ToDouble(salePur.RoundAmount) < 0)
            {
                float RoundAmt = salePur.RoundAmount;
                var roundConvert = GetConvertAmount(mCurCode, RoundAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.PartyCode, CashCode, Math.Abs(RoundAmt), mCurCode.ToString(), "CPUR", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, Math.Abs(roundConvert.MCurAmt), sCurBaseRate, Math.Abs(roundConvert.SCurAmt), 0, usercode, userId, companyId);
            }
        }

        #endregion

        #region Copied helpers from PurchaseParcelRepository

        private async Task<string> ExecuteCommonDataScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using (var cmd = new SqlCommand("GetAllCommonData", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Type", type));
                // Unescape double single-quotes since this is passed as a SqlParameter value to a dynamic SQL SP
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
                // Unescape double single-quotes since this is passed as a SqlParameter value to a dynamic SQL SP
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

        #endregion

        #region Raw row DTOs

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

        private class StatusInvRow
        {
            public string? Type { get; set; }
            public DateTime? Date { get; set; }
            public int? Qty { get; set; }
            public double? Wt { get; set; }
            public string? Description { get; set; }
            public double? Rate { get; set; }
            public double? Tax { get; set; }
            public double? Amount { get; set; }
            public int IsPayment { get; set; }
            public string? Party { get; set; }
            public string? AddLine1 { get; set; }
            public string? AddLine2 { get; set; }
            public string? AddLine3 { get; set; }
            public string? Mobile { get; set; }
            public string? CPerson { get; set; }
            public string? ShipVia { get; set; }
            public string? Status { get; set; }
            public string? SalesPerson { get; set; }
        }

        #endregion

        public async Task<List<object>> GetLotsAsync(string? searchBy, string companyId)
        {
            string safe = (searchBy ?? "").Replace("'", "''");
            string strWhere = string.IsNullOrWhiteSpace(safe)
                ? ""
                : $" AND (LotNo LIKE ''%{safe}%'' OR Itemname LIKE ''%{safe}%'') ";

            string sql = $"EXEC [GetMixlotDataForDropdown] @strWhere = '{strWhere}'";
            var rows = (await _sqlDataAccess.SelectRawSql<MixLotDropdownRow>(sql, companyId)).ToList();

            return rows.Select(r => (object)new
            {
                id = r.ID ?? r.Id ?? r.LotNo ?? "",
                ID = r.ID ?? r.Id ?? r.LotNo ?? "",
                name = r.Name ?? r.LotNo ?? "",
                Name = r.Name ?? r.LotNo ?? "",
                text = r.TextName ?? r.ItemName ?? r.Name ?? r.LotNo ?? "",
                TextName = r.TextName ?? r.ItemName ?? r.Name ?? r.LotNo ?? ""
            }).ToList();
        }

        private class MixLotDropdownRow
        {
            public string? ID { get; set; }
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? TextName { get; set; }
            public string? LotNo { get; set; }
            public string? ItemName { get; set; }
        }
    }
}
