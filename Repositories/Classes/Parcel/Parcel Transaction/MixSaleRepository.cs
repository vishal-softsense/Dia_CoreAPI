using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction
{
    public class MixSaleRepository : IMixSaleRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public MixSaleRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<List<MixSaleInvListDto>> GetListAsync(int pageNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@PageNo", pageNo }
            };

            var result = (await _sqlDataAccess.SelectDataFromSP<MixSaleInvListDto>(
                "GetMixSaleParceJewlInvDataUsingPageNo",
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

        public async Task<MixSalePageDataDto> GetPageDataAsync(string companyId)
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

            var caratFormatNew = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'CaratFormatNew'",
                new { },
                companyId);

            return new MixSalePageDataDto
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
                Address = MapBasic(list.Where(x => x.Type == "Bank")),
                DefaultCurrencyCode = defaultCurrencyCode,
                CaratFormatNew = caratFormatNew
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

        public async Task<MixSaleDetailDto?> GetDetailAsync(int id, string companyId)
        {
            var headerParams = new Dictionary<string, object>
            {
                { "@Where", $" tsp.TrnType = 'CSALE' AND Code = {id}" }
            };

            var header = await _sqlDataAccess.SelectDataFromSPSingle<MixSaleHeaderDto>(
                "GetSalepurRecord",
                headerParams,
                companyId);

            if (header == null)
                return null;

            var itemParams = new Dictionary<string, object>
            {
                { "@Where", $" AND InvCode = {id}" }
            };

            var items = (await _sqlDataAccess.SelectDataFromSP<MixSaleItemDto>(
                "GetSalepurTran_Sale",
                itemParams,
                companyId)).ToList();

            foreach (var item in items)
                NormalizePurchaseItem(item);

            return new MixSaleDetailDto
            {
                Header = header,
                Items = items
            };
        }

        public async Task<List<MixSaleLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
        {
            var escaped = EscapeSql(lotNo);

            // Jewellery stock uses Stock_ID (MVC GetMaxJewStockCode path on save)
            if (IsJewelleryLotNo(lotNo))
            {
                string jewSql = $"EXEC [GetLotNoDataForAll] @Type = 'JewStockData',@Where = ' AND Stock_ID = ''{escaped}'' '";
                var jewResult = (await _sqlDataAccess.SelectRawSql<MixSaleLotDetailDto>(jewSql, companyId)).ToList();
                if (jewResult.Count > 0)
                {
                    foreach (var lot in jewResult)
                    {
                        if (string.IsNullOrWhiteSpace(lot.LotNo))
                            lot.LotNo = lot.Stock_ID ?? lotNo;
                        lot.StockType ??= "Jewellery";
                        NormalizeLotDetail(lot);
                    }
                    return jewResult;
                }
            }

            var parameters = new Dictionary<string, object>
            {
                { "@Type", "Mix" },
                { "@Where", $" AND LotNo = '{escaped}' " }
            };

            var result = (await _sqlDataAccess.SelectDataFromSP<MixSaleLotDetailDto>(
                "GetLotNoDataForAll",
                parameters,
                companyId)).ToList();

            foreach (var lot in result)
                NormalizeLotDetail(lot);

            return result;
        }

        private static void NormalizePurchaseItem(MixSaleItemDto item)
        {
            if (item == null) return;

            if (item.MemoTransCode == 0 && item.MemoTranCode != 0)
                item.MemoTransCode = item.MemoTranCode;

            if (string.IsNullOrWhiteSpace(item.SRtnStatus) && !string.IsNullOrWhiteSpace(item.Status))
                item.SRtnStatus = item.Status;
            else if (string.IsNullOrWhiteSpace(item.Status) && !string.IsNullOrWhiteSpace(item.SRtnStatus))
                item.Status = item.SRtnStatus;

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

            if (string.IsNullOrWhiteSpace(item.PShape))
                item.PShape = item.ShapeId ?? item.Shape;

            if (string.IsNullOrWhiteSpace(item.PColor))
                item.PColor = item.ColorId ?? item.Color;

            if (string.IsNullOrWhiteSpace(item.PClarity))
                item.PClarity = item.ClarityId ?? item.Clarity;
        }

        private static string EscapeSql(string? value) => (value ?? string.Empty).Replace("'", "''");

        private static bool IsJewelleryLotNo(string? lotNo)
        {
            var lot = (lotNo ?? string.Empty).Trim();
            return lot.StartsWith("Jew_", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsJewelleryItem(MixSaleItemDto item)
        {
            if (string.Equals(item.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase))
                return true;
            return IsJewelleryLotNo(item.LotNo);
        }

        private static bool IsJewelleryLotRequest(MixSaleAddLotRequest item)
        {
            if (string.Equals(item.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase))
                return true;
            return IsJewelleryLotNo(item.LotNo);
        }

        private static void NormalizeLotDetail(MixSaleLotDetailDto lot)
        {
            if (lot == null) return;

            if (!string.IsNullOrWhiteSpace(lot.Stock_ID) && string.IsNullOrWhiteSpace(lot.StockType))
                lot.StockType = "Jewellery";

            // GetLotNoDataForAll Type=Mix returns Carats / PurchaseMRP (MVC LotNoData)
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
            // Match MVC Sp_SelectMixMemoForMixSale:
            // EXEC [Sp_SelectMixMemoForMixSale] @Party = '...', @invno = '...'
            static string Esc(string? s) => (s ?? string.Empty).Replace("'", "''");
            var sql =
                $"EXEC [Sp_SelectMixMemoForMixSale] @Party = '{Esc(partyCode)}',@invno = '{Esc(memoNo)}'";

            var result = await _sqlDataAccess.SelectRawSql<MixConsignmentRowDto>(sql, companyId);
            return result?.ToList() ?? new List<MixConsignmentRowDto>();
        }

        public async Task<MixSaleGridViewDto> AddItemInGridAsync(
            MixSaleAddLotRequest item,
            string companyId)
        {
            var caratFormat = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'CaratFormatNew'",
                new { },
                companyId) ?? "0.000";

            double totalCarats = 0;
            double totalAmount = 0;
            double totalMRPAmount = 0;
            double totalAmountWithoutless = 0;
            string alert = "";

            bool isJewellery = IsJewelleryLotRequest(item);
            float selectionCarats = FormatSelectionCarats(item.Weight, caratFormat);
            float rate = item.Rate;
            float back = item.Back;
            float mrp = item.MRP;
            float issueCarats = item.IssueCarats != 0 ? item.IssueCarats : selectionCarats;
            float returnCarats = item.ReturnCarats != 0 ? item.ReturnCarats : item.RtnCarats;

            if (isJewellery)
            {
                if (!(item.Pcs > 0 || selectionCarats == 0))
                    throw new Exception("Carat is invalid.");
            }
            else if (!(selectionCarats > 0 || selectionCarats == 0))
            {
                throw new Exception("Carat is invalid.");
            }

            float amount;
            if (rate != 0)
            {
                amount = isJewellery ? item.Pcs * rate : selectionCarats * rate;
                float l1 = item.Less1 != 0 ? amount * (100 + item.Less1) / 100 : amount;
                float l2 = item.Less2 != 0 ? l1 * (100 + item.Less2) / 100 : l1;
                float l3 = item.Less3 != 0 ? l2 * (100 + item.Less3) / 100 : l2;
                amount = l3;
            }
            else
            {
                amount = item.Amount;
            }

            var row = new MixSaleItemDto
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
                Weight = item.Weight != 0 ? item.Weight : selectionCarats,
                Carats = item.Weight != 0 ? item.Weight : selectionCarats,
                IssueCarats = issueCarats,
                Pcs = item.Pcs,
                MRP = mrp,
                PurchaseRate = rate,
                Rate = rate,
                PurchaseBack = back,
                Less1 = item.Less1,
                Less2 = item.Less2,
                Less3 = item.Less3,
                PurAmount = amount,
                Amount = amount,
                MemoTransCode = item.MemoTransCode,
                RtnCarats = returnCarats,
                ReturnCarats = returnCarats,
                StockType = string.IsNullOrWhiteSpace(item.StockType) ? "Diamond" : item.StockType,
                InvDescription = item.InvDescription,
                SuppLotNo = item.SuppLotNo,
                AskingRate = item.AskingRate,
                ActualRate = item.ActualRate,
                GroupId = item.GroupId,
                StoneType = item.StoneType,
                RMK = item.RMK,
                COGS = item.COGS,
                ExportNo = item.ExportNo,
                SRate = item.SRate
            };

            var list = item.Items ?? new List<MixSaleItemDto>();
            bool isEdit = item.editIndex >= 0 && item.editIndex < list.Count;

            if (isEdit)
                list[item.editIndex] = row;
            else
                list.Add(row);

            foreach (var gridRow in list)
            {
                totalCarats += FormatSelectionCarats(gridRow.Weight, caratFormat);
                totalAmount += gridRow.PurAmount != 0 ? gridRow.PurAmount : gridRow.Amount;
                totalAmountWithoutless += FormatSelectionCarats(gridRow.Weight, caratFormat) * gridRow.PurchaseRate;
            }

            return new MixSaleGridViewDto
            {
                TotalCarats = Math.Round(totalCarats, 3),
                TotalAmount = Math.Round(totalAmount, 2),
                TotalMRPAmount = Math.Round(totalMRPAmount, 3),
                TotalAmountWithoutless = Math.Round(totalAmountWithoutless, 3),
                Alert = alert,
                ExcelData = list
            };
        }

        private static float FormatSelectionCarats(float weight, string caratFormat)
        {
            string weight3D = Convert.ToDouble(weight).ToString("N3");
            if (caratFormat == "0.000")
                return float.Parse(weight3D);
            string weight2D = weight3D.Substring(0, weight3D.Length - 1);
            return float.Parse(weight2D);
        }

        public async Task<(bool Success, string Message)> AddAsync(
            MixSaleHeaderDto salePur,
            List<MixSaleItemDto> items,
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
                        string InvnoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''CSALE'' ");
                        int.TryParse(InvnoStr, out MaxInvNo);
                        MaxInvNo = MaxInvNo + 1;

                        bool outstanding = salePur.chkoldsale;
                        bool stockEffected = !salePur.chkoldsale;

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
                            MCurCode, SCurCode, stockEffected, usercode, userId, companyId, isEdit: false);

                        if (!salePur.chkoldsale)
                        {
                            await InsertSaleJvEntriesAsync(
                                conn, trans, salePur, MaxSPCode, Curcode,
                                MCurBaseRate, SCurBaseRate, MCurOperator, MCurOperatorM, SCurOperatorM,
                                MToSCurRate, MCurCode, usercode, userId, companyId);
                        }

                        trans.Commit();
                        return (true, "Mix Sale Record Saved.");
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
            MixSaleHeaderDto salePur,
            List<MixSaleItemDto> items,
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

                        bool outstanding = salePur.chkoldsale;
                        bool stockEffected = !salePur.chkoldsale;

                        int MaxSPCode;

                        if (string.Equals(salePur.Mode, "consignment", StringComparison.OrdinalIgnoreCase))
                        {
                            int MaxInvNo;
                            string InvnoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''CSALE'' ");
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
                            MCurCode, SCurCode, stockEffected, usercode, userId, companyId, isEdit: true);

                        using (var cmd = new SqlCommand("SP_DeleteJV", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@vcode", MaxSPCode));
                            cmd.Parameters.Add(new SqlParameter("@vtype", "CSALE"));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        if (!salePur.chkoldsale)
                        {
                            await InsertSaleJvEntriesAsync(
                                conn, trans, salePur, MaxSPCode, Curcode,
                                MCurBaseRate, SCurBaseRate, MCurOperator, MCurOperatorM, SCurOperatorM,
                                MToSCurRate, MCurCode, usercode, userId, companyId);
                        }

                        trans.Commit();
                        return (true, "Mix Sale Record Updated.");
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
                                cmd.Parameters.Add(new SqlParameter("@Actiontype", "memotosaletest"));
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
                            cmd.Parameters.Add(new SqlParameter("@vtype", "CSALE"));
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

        public async Task<MixSalePrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
        {
            var escapedInvNo = (invNo ?? "").Replace("'", "''");
            string where;
            if (int.TryParse(invNo, out var codeId))
                where = $"tsp.TrnType = ''CSALE'' AND tsp.Code = {codeId}";
            else
                where = $"tsp.TrnType = ''CSALE'' AND tsp.InvNo = ''{escapedInvNo}''";

            string sql = $"EXEC GetMixSalepurRecordWithDetailsForINV @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectDataAdoDynamic(sql, companyId)).ToList();
            if (rows.Count == 0 && int.TryParse(invNo, out _))
            {
                where = $"tsp.TrnType = ''CSALE'' AND tsp.InvNo = ''{escapedInvNo}''";
                sql = $"EXEC GetMixSalepurRecordWithDetailsForINV @Where = '{where}'";
                rows = (await _sqlDataAccess.SelectDataAdoDynamic(sql, companyId)).ToList();
            }
            if (rows.Count == 0)
                return null;

            if (rows[0] is not IDictionary<string, object> header)
                return null;

            var trnDate = GetPrintDateTime(header, "TrnDate");
            var roundDate = GetPrintDateTime(header, "RoundDate");

            var result = new MixSalePrintViewDto
            {
                InvNo = GetPrintString(header, "InvNo"),
                BillNo = GetPrintString(header, "BillNo"),
                TrnDate = trnDate?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                RoundDate = roundDate?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                PartyName = GetPrintString(header, "PartyName"),
                AddressLine1 = GetPrintString(header, "AddressLine1"),
                AddressLine2 = GetPrintString(header, "AddressLine2"),
                AddressLine3 = GetPrintString(header, "AddressLine3"),
                InvDisc = GetPrintString(header, "InvDisc"),
                SalesPersons = GetPrintString(header, "SalesPersons", "SalesPerson"),
                ShipVia = GetPrintString(header, "ShipVia"),
                Tracking = GetPrintString(header, "Tracking"),
                FinalAmount = GetPrintDouble(header, "FinalAmount"),
                bnm = GetPrintString(header, "bnm"),
                BankAccNo = GetPrintString(header, "BankAccNo"),
                ABARoutingNumber = GetPrintString(header, "ABARoutingNumber"),
                SwiftCode = GetPrintString(header, "SwiftCode"),
                ZelleID = GetPrintString(header, "ZelleID"),
                InvFeesDescription = GetPrintString(header, "InvFeesDescription"),
                Items = new List<MixSalePrintItemDto>()
            };

            foreach (var dr in rows)
            {
                if (dr is not IDictionary<string, object> rowDict)
                    continue;

                string? lotNo = ResolvePrintLotNo(rowDict);
                byte[]? imageFromRow = GetPrintBytes(rowDict, "ImageFullPath", "ImagePath", "AssetImagePath");
                var imageResult = await ResolvePrintItemImageAsync(lotNo, imageFromRow, companyId);
                string? itemName = GetPrintString(rowDict, "ItemName");
                var pcs = GetPrintInt(rowDict, "Pcs");
                var carats = GetPrintDouble(rowDict, "Carats");
                var rate = GetPrintDouble(rowDict, "Rate");
                var amount = GetPrintDouble(rowDict, "MasterCurrencyAmount", "Amount");

                result.Items.Add(new MixSalePrintItemDto
                {
                    MemoNo = GetPrintString(rowDict, "MemoNo", "ExportNo"),
                    Lotno = lotNo,
                    StockNo = lotNo,
                    ItemName = itemName,
                    Description = itemName,
                    Pcs = pcs,
                    Qty = pcs,
                    Carats = carats,
                    Weight = carats,
                    TotalDiamondCarat = GetPrintNullableDouble(rowDict, "TotalDiamondCarat"),
                    Rate = rate,
                    PricePerCarat = rate,
                    MasterCurrencyAmount = amount,
                    Amount = amount,
                    StockType = GetPrintString(rowDict, "StockType", "SubType", "Category"),
                    ImageFullPath = imageResult.Item1,
                    ImagePath = imageResult.Item2
                });
            }

            return result;
        }

        private static string? ResolvePrintLotNo(IDictionary<string, object>? row)
        {
            if (row == null)
                return null;

            string? lot = GetPrintString(row, "lotno", "Lotno", "LotNo", "Stock_ID", "StockId", "JewItem", "StockNo");
            if (!string.IsNullOrWhiteSpace(lot))
                return lot;

            if (!IsJewelleryPrintRow(row))
                return null;

            string? itemName = GetPrintString(row, "ItemName");
            if (string.IsNullOrWhiteSpace(itemName))
                return null;

            string[] parts = itemName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : null;
        }

        private static bool IsJewelleryPrintRow(IDictionary<string, object> row)
        {
            string? stockType = GetPrintString(row, "StockType", "SubType", "Category")?.ToLowerInvariant();
            if (stockType is "jewellery" or "jew" or "jewelry")
                return true;

            if (GetPrintNullableDouble(row, "GrossWeight").HasValue || GetPrintNullableDouble(row, "TotalDiamondCarat").HasValue)
                return true;

            string? shape = GetPrintString(row, "ShapeId", "Shape");
            string? clarity = GetPrintString(row, "ClarityId", "Clarity");
            double carats = GetPrintDouble(row, "Carats");
            return string.IsNullOrWhiteSpace(shape) && string.IsNullOrWhiteSpace(clarity) && carats == 0;
        }

        #region Print mapping helpers
        private static bool IsPrintColumnMatch(string dictKey, string searchKey)
        {
            if (string.Equals(dictKey, searchKey, StringComparison.OrdinalIgnoreCase))
                return true;
            if (!dictKey.StartsWith(searchKey, StringComparison.OrdinalIgnoreCase))
                return false;
            if (dictKey.Length == searchKey.Length)
                return true;
            for (int i = searchKey.Length; i < dictKey.Length; i++)
            {
                if (!char.IsDigit(dictKey[i]))
                    return false;
            }
            return true;
        }

        private static string? GetPrintString(IDictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                foreach (var kvp in row)
                {
                    if (!IsPrintColumnMatch(kvp.Key, key))
                        continue;
                    var formatted = FormatPrintScalarString(kvp.Value);
                    if (!string.IsNullOrWhiteSpace(formatted))
                        return formatted;
                }
            }
            return null;
        }

        private static string? FormatPrintScalarString(object? value)
        {
            if (value == null || value == DBNull.Value)
                return null;
            var text = Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        private static byte[]? GetPrintBytes(IDictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                foreach (var kvp in row)
                {
                    if (!IsPrintColumnMatch(kvp.Key, key))
                        continue;
                    if (kvp.Value is byte[] bytes && bytes.Length > 0)
                        return bytes;
                }
            }
            return null;
        }

        private static int GetPrintInt(IDictionary<string, object> row, params string[] keys)
        {
            string? text = GetPrintString(row, keys);
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : 0;
        }

        private static double GetPrintDouble(IDictionary<string, object> row, params string[] keys)
        {
            string? text = GetPrintString(row, keys);
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double n) ? n : 0;
        }

        private static double? GetPrintNullableDouble(IDictionary<string, object> row, params string[] keys)
        {
            string? text = GetPrintString(row, keys);
            if (string.IsNullOrWhiteSpace(text))
                return null;
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double n) ? n : null;
        }

        private static DateTime? GetPrintDateTime(IDictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                foreach (var kvp in row)
                {
                    if (!IsPrintColumnMatch(kvp.Key, key))
                        continue;
                    if (kvp.Value is DateTime dt)
                        return dt;
                    if (kvp.Value != null && kvp.Value != DBNull.Value &&
                        DateTime.TryParse(Convert.ToString(kvp.Value, CultureInfo.InvariantCulture),
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                        return parsed;
                }
            }
            return null;
        }
        #endregion

        /// <summary>MVC Jewellery_SaleUSA_Print: ImageFullPath byte[]; JewStockData fallback uses Stock_ID only.</summary>
        private async Task<(byte[]? ImageFullPath, string? ImagePath)> ResolvePrintItemImageAsync(
            string? lotNo,
            byte[]? imageBytes,
            string companyId)
        {
            if (imageBytes != null && imageBytes.Length > 0)
                return (imageBytes, null);

            if (string.IsNullOrWhiteSpace(lotNo))
                return (null, null);

            try
            {
                // MVC GetLotNoDataForAll JewStockData — ViewJewelleryClosingStockDateWise has Stock_ID, not LOTNO
                var escaped = lotNo.Replace("'", "''");
                var sql = $"EXEC [GetLotNoDataForAll] @Type = 'JewStockData', @Where = ' AND Stock_ID = ''{escaped}'' '";
                var lotRows = (await _sqlDataAccess.SelectDataAdoDynamic(sql, companyId)).ToList();
                foreach (var row in lotRows)
                {
                    if (row is not IDictionary<string, object> dict)
                        continue;

                    byte[]? foundBytes = null;
                    string? foundPath = null;

                    foreach (var kvp in dict)
                    {
                        if (kvp.Key.StartsWith("ImageFullPath", StringComparison.OrdinalIgnoreCase) &&
                            kvp.Value is byte[] bytes && bytes.Length > 0)
                        {
                            foundBytes = bytes;
                            break;
                        }
                    }

                    if (foundBytes != null)
                        return (foundBytes, null);

                    foreach (var kvp in dict)
                    {
                        if (!kvp.Key.StartsWith("ImagePath", StringComparison.OrdinalIgnoreCase) ||
                            kvp.Key.StartsWith("ImageFullPath", StringComparison.OrdinalIgnoreCase))
                            continue;
                        var path = kvp.Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(path) && !string.Equals(path, "null", StringComparison.OrdinalIgnoreCase))
                        {
                            foundPath = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(foundPath))
                        return (null, foundPath);
                }
            }
            catch
            {
                // Image lookup must not block invoice print
            }

            return (null, null);
        }

        public async Task<MixSaleStatusDto?> GetCurrentStatusAsync(string invNo, string companyId)
        {
            // MVC uses Code with TrnType 'CSALE'
            string where;
            if (int.TryParse(invNo, out var codeId))
                where = $"tsp.TrnType = ''CSALE'' AND tsp.Code = {codeId}";
            else
                where = $"tsp.TrnType = ''CSALE'' AND tsp.InvNo = {invNo}";

            string sql = $"EXEC [SoliPrintCurStatusInv] @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectRawSql<StatusInvRow>(sql, companyId)).ToList();
            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var invoiceRow = rows.FirstOrDefault(x => string.Equals(x.Type, "Invoice", StringComparison.OrdinalIgnoreCase)) ?? header;

            var result = new MixSaleStatusDto
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
                Items = new List<MixSaleStatusItemDto>()
            };

            foreach (var row in rows)
            {
                result.Items.Add(new MixSaleStatusItemDto
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
            MixSaleHeaderDto salePur,
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
                string billNo = string.IsNullOrWhiteSpace(salePur.BillNo) ? maxInvNo.ToString() : salePur.BillNo;
                cmd.Parameters.Add(new SqlParameter("@invno", maxInvNo));
                cmd.Parameters.Add(new SqlParameter("@billno", billNo));
                cmd.Parameters.Add(new SqlParameter("@trntype", "CSALE"));
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
                cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.RoundDate ?? salePur.DueDate));
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
                cmd.Parameters.Add(new SqlParameter("@SalesPerson", salePur.SalesPerson ?? ""));
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
                cmd.Parameters.Add(new SqlParameter("@Address", salePur.Address > 0 ? salePur.Address : 1));
                cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateSalePurAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int code,
            int invNo,
            MixSaleHeaderDto salePur,
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
                cmd.Parameters.Add(new SqlParameter("@TrnType", "CSALE"));
                cmd.Parameters.Add(new SqlParameter("@invtype", salePur.InvType ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@SubType", salePur.SubType ?? ""));
                cmd.Parameters.Add(new SqlParameter("@BankID", salePur.Address > 0 ? salePur.Address : 0));
                cmd.Parameters.Add(new SqlParameter("@Category", salePur.Category ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Trndate", salePur.TrnDate));
                cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.RoundDate ?? salePur.DueDate));
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
                cmd.Parameters.Add(new SqlParameter("@SalesPerson", salePur.SalesPerson ?? ""));
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
            MixSaleHeaderDto salePur,
            List<MixSaleItemDto> items,
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
            bool isEdit)
        {
            foreach (var im in items)
            {
                float rate = im.PurchaseRate != 0 ? im.PurchaseRate : im.Rate;
                float amount = im.PurAmount != 0 ? im.PurAmount : im.Amount;
                var amtRes = GetConvertAmount(curCode, amount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);

                bool isJewellery = IsJewelleryItem(im);
                string stockType = isJewellery ? "Jewellery" : (im.StockType ?? "Diamond");
                string lotKey = EscapeSql(im.LotNo);

                int maxItemCode = 0;
                if (isJewellery)
                {
                    string itemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "StockCode", $" AND Stock_ID = ''{lotKey}''");
                    int.TryParse(itemCodeStr, out maxItemCode);
                    if (maxItemCode == 0)
                        throw new Exception($"Jewellery stock '{im.LotNo}' not found.");
                }
                else
                {
                    string itemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "ItemCode", $" AND LotNo = ''{lotKey}''");
                    int.TryParse(itemCodeStr, out maxItemCode);
                    if (maxItemCode == 0)
                        throw new Exception($"Lot number '{im.LotNo}' not found in Item Master.");
                }

                string itemName = !string.IsNullOrWhiteSpace(im.ItemName) ? im.ItemName : im.LotNo ?? "";

                float memoTransCode = im.MemoTransCode;
                float returnCarats = im.RtnCarats != 0 ? im.RtnCarats : im.ReturnCarats;
                float issueCarats = im.IssueCarats != 0 ? im.IssueCarats : im.Weight;
                float back = im.PurchaseBack;
                float spCarats = memoTransCode != 0 ? float.Parse(im.Weight.ToString("N3")) : 0f;

                string pShape = im.PShape ?? im.Shape ?? im.ShapeId ?? "";
                string pSize = im.PSize ?? im.Size ?? "";
                string pSieve = im.PSieve ?? im.Sieve ?? im.SieveId ?? "";
                string pColor = im.PColor ?? im.Color ?? im.ColorId ?? "";
                string pClarity = im.PClarity ?? im.Clarity ?? im.ClarityId ?? "";

                using (var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPdate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@trntype", "CSALE"));
                    cmd.Parameters.Add(new SqlParameter("@InvType", salePur.InvType ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@InvCode", maxSPCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemCode", maxItemCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemName", itemName));
                    cmd.Parameters.Add(new SqlParameter("@Carats", float.Parse(im.Weight.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                    cmd.Parameters.Add(new SqlParameter("@Rate", Convert.ToDouble(rate.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@MRP", im.MRP));
                    cmd.Parameters.Add(new SqlParameter("@PMRPDown", Convert.ToDouble(back.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ReturnCarats", Convert.ToDouble(returnCarats.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@IssueCarats", Convert.ToDouble(issueCarats.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@amount", Convert.ToDouble(amount.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ExportNo", im.ExportNo ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                    cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                    cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                    cmd.Parameters.Add(new SqlParameter("@CurCode", salePur.CurCode.ToString()));
                    cmd.Parameters.Add(new SqlParameter("@memotrancode", memoTransCode));
                    cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                    cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
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
                    cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", Convert.ToDouble(amtRes.MCurAmt.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(sCurBaseRate, 4)));
                    cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", Convert.ToDouble(amtRes.SCurAmt.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
                    cmd.Parameters.Add(new SqlParameter("@PShape", pShape));
                    cmd.Parameters.Add(new SqlParameter("@PSize", pSize));
                    cmd.Parameters.Add(new SqlParameter("@PSieve", pSieve));
                    cmd.Parameters.Add(new SqlParameter("@PColor", pColor));
                    cmd.Parameters.Add(new SqlParameter("@PClarity", pClarity));
                    cmd.Parameters.Add(new SqlParameter("@COGS", im.COGS));
                    cmd.Parameters.Add(new SqlParameter("@StockType", stockType));
                    cmd.Parameters.Add(new SqlParameter("@COGS", 0.0f));
                    cmd.Parameters.Add(new SqlParameter("@StockType", im.StockType ?? "Diamond"));
                    cmd.Parameters.Add(new SqlParameter("@InvDescription", im.InvDescription ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
                    await cmd.ExecuteNonQueryAsync();
                }

                string memoAction;
                if (memoTransCode != 0 && returnCarats != 0)
                    memoAction = "csalertn";
                else if (memoTransCode != 0)
                    memoAction = isEdit ? "memotosale" : "memotosaletest";
                else
                    memoAction = isEdit ? "memotosaletest" : "memotosale";

                using (var cmd = new SqlCommand("SP_UpdateMemo", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@code", memoTransCode));
                    cmd.Parameters.Add(new SqlParameter("@RtnDate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@rtncts", float.Parse(returnCarats.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@spcts", float.Parse(im.Weight.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@Actiontype", memoAction));
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>MVC MixController SubmitMixLot JV: Party?SALE, taxes from SALE, Round Party?Cash.</summary>
        private async Task InsertSaleJvEntriesAsync(
            SqlConnection conn,
            SqlTransaction trans,
            MixSaleHeaderDto salePur,
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
            long SaleCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''SALE''"), out SaleCode);
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

            float SaleJvAmount = salePur.FinalMAmount;
            var jvConvert = GetConvertAmount(mCurCode, SaleJvAmount, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
            double SaleJvMAmount = jvConvert.MCurAmt;
            double SaleJvSAmount = jvConvert.SCurAmt;

            float SaleJvAmount_F = 0;
            if (mCurOperator == "*")
                SaleJvAmount_F = mCurBaseRate != 0 ? SaleJvAmount / mCurBaseRate : 0f;
            if (mCurOperator == "/")
                SaleJvAmount_F = SaleJvAmount * mCurBaseRate;

            // Sale JV: Party Dr / SALE Cr
            await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.PartyCode, SaleCode, Math.Abs(SaleJvAmount_F), curCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, SaleJvMAmount, sCurBaseRate, SaleJvSAmount, 0, usercode, userId, companyId);

            if ((Convert.ToDouble(salePur.BrokerAmount) != 0) && salePur.BrokerCode != null)
            {
                float BroAmt = salePur.BrokerAmount;
                var broConvert = GetConvertAmount(mCurCode, BroAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, BrokerageCode, salePur.BrokerCode.Value, Math.Abs(BroAmt), mCurCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, broConvert.MCurAmt, sCurBaseRate, broConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if ((Convert.ToDouble(salePur.AdatAmount) != 0) && salePur.AdatCode != null)
            {
                float AdatAmt = salePur.AdatAmount;
                var adatConvert = GetConvertAmount(mCurCode, AdatAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, AdatCode, salePur.AdatCode.Value, AdatAmt, mCurCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, adatConvert.MCurAmt, sCurBaseRate, adatConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.VatAmount) != 0)
            {
                float VatAmt = salePur.VatAmount;
                var vatConvert = GetConvertAmount(mCurCode, VatAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, SaleCode, VatCode, VatAmt, mCurCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, vatConvert.MCurAmt, sCurBaseRate, vatConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.SGSTAmt) != 0)
            {
                float SGSTAmt = salePur.SGSTAmt;
                var sgstConvert = GetConvertAmount(mCurCode, SGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, SaleCode, SGSTCode, SGSTAmt, mCurCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, sgstConvert.MCurAmt, sCurBaseRate, sgstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.CGSTAmt) != 0)
            {
                float CGSTAmt = salePur.CGSTAmt;
                var cgstConvert = GetConvertAmount(mCurCode, CGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, SaleCode, CGSTCode, CGSTAmt, mCurCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, cgstConvert.MCurAmt, sCurBaseRate, cgstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.IGSTAmt) != 0)
            {
                float IGSTAmt = salePur.IGSTAmt;
                var igstConvert = GetConvertAmount(mCurCode, IGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, SaleCode, IGSTCode, IGSTAmt, mCurCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, igstConvert.MCurAmt, sCurBaseRate, igstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.RoundAmount) > 0)
            {
                float RoundAmt = salePur.RoundAmount;
                var roundConvert = GetConvertAmount(mCurCode, RoundAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.PartyCode, CashCode, salePur.RoundAmount, mCurCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, roundConvert.MCurAmt, sCurBaseRate, roundConvert.SCurAmt, 0, usercode, userId, companyId);
            }
            else if (Convert.ToDouble(salePur.RoundAmount) < 0)
            {
                float RoundAmt = salePur.RoundAmount;
                var roundConvert = GetConvertAmount(mCurCode, RoundAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, CashCode, salePur.PartyCode, Math.Abs(RoundAmt), mCurCode.ToString(), "CSALE", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, Math.Abs(roundConvert.MCurAmt), sCurBaseRate, Math.Abs(roundConvert.SCurAmt), 0, usercode, userId, companyId);
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
            public string? lotno { get; set; }
            public string? ItemName { get; set; }
            public int Pcs { get; set; }
            public double Carats { get; set; }
            public double Rate { get; set; }
            public double MasterCurrencyAmount { get; set; }
            public byte[]? ImageFullPath { get; set; }
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
            // MVC MixController GetMixLotNoList ? { id = Name, text = TextName }
            if (string.IsNullOrWhiteSpace(searchBy) || searchBy.Trim().Length < 2)
                return new List<object>();

            string safe = EscapeSql(searchBy.Trim());
            var results = new List<object>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // Jewellery stock (MVC GetJewStockDataForDropdown — search by Stock_ID only)
                string jewSql = $"EXEC [GetJewStockDataForDropdown] @strWhere = ' AND (Stock_ID LIKE ''%{safe}%'')'";
                var jewRows = (await _sqlDataAccess.SelectRawSql<StockBasic>(jewSql, companyId)).ToList();
                foreach (var r in jewRows)
                {
                    var lotNo = (r.Name ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(lotNo) || !seen.Add(lotNo)) continue;
                    var text = !string.IsNullOrWhiteSpace(r.TextName) ? r.TextName.Trim() : lotNo;
                    results.Add(new { id = lotNo, text, stockType = "Jewellery" });
                }
            }
            catch
            {
                // Jewellery search is optional; diamond lots can still be returned.
            }

            try
            {
                string strWhere = $" AND (LotNo LIKE ''%{safe}%'' OR Itemname LIKE ''%{safe}%'') ";
                string sql = $"EXEC [GetMixlotDataForDropdown] @strWhere = '{strWhere}'";
                var rows = (await _sqlDataAccess.SelectRawSql<MixLotDropdownRow>(sql, companyId)).ToList();
                foreach (var r in rows)
                {
                    var lotNo = (r.Name ?? r.LotNo ?? r.ID ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(lotNo) || !seen.Add(lotNo)) continue;
                    var text = (r.TextName ?? r.ItemName ?? r.Name ?? r.LotNo ?? lotNo).Trim();
                    results.Add(new { id = lotNo, text, stockType = "Diamond" });
                }
            }
            catch
            {
                // Return jewellery matches even if diamond search fails.
            }

            return results;
        }

        private class MixLotDropdownRow
        {
            public string? ID { get; set; }
            public string? Name { get; set; }
            public string? TextName { get; set; }
            public string? Type { get; set; }
            public string? LotNo { get; set; }
            public string? ItemName { get; set; }
        }
    }
}
