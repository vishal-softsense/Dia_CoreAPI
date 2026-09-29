using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryTransaction;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryTransaction;
using Microsoft.Data.SqlClient;
using System.Data;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Jewellery.JewelleryTransaction
{
    public class JewelleryPurchaseRepository : IJewelleryPurchaseRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public JewelleryPurchaseRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<List<JewelleryPurchaseInvListDto>> GetListAsync(int pageNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@PageNo", pageNo }
            };

            var result = (await _sqlDataAccess.SelectDataFromSP<JewelleryPurchaseInvListDto>(
                "GetJewelleryPurchaseInvDataUsingPageNo",
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

        public async Task<JewelleryPurchasePageDataDto> GetPageDataAsync(string companyId)
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

            return new JewelleryPurchasePageDataDto
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

        public async Task<JewelleryPurchaseDetailDto?> GetDetailAsync(int id, string companyId)
        {
            var headerParams = new Dictionary<string, object>
            {
                { "@Where", $" tsp.TrnType = 'PJEW' AND Code = {id}" }
            };

            var header = await _sqlDataAccess.SelectDataFromSPSingle<JewelleryPurchaseHeaderDto>(
                "GetSalepurRecord",
                headerParams,
                companyId);

            if (header == null)
                return null;

            var itemParams = new Dictionary<string, object>
            {
                { "@Where", $" AND InvCode = {id}" }
            };

            var rawItems = (await _sqlDataAccess.SelectDataFromSP<JewPurchaseTranRow>(
                "GetSalepurTran",
                itemParams,
                companyId)).ToList();

            var items = rawItems.Select(MapTranRowToItem).ToList();

            return new JewelleryPurchaseDetailDto
            {
                Header = header,
                Items = items
            };
        }

        private static JewelleryPurchaseItemDto MapTranRowToItem(JewPurchaseTranRow row)
        {
            int pcs = row.Pcs;
            if (pcs == 0 && row.Carats != 0)
                pcs = (int)Math.Round(row.Carats);

            float amount = row.Amount;
            if (amount == 0 && row.PurAmount != 0)
                amount = row.PurAmount;

            float memo = row.MemoTransCode != 0 ? row.MemoTransCode : row.MemoTranCode;

            return new JewelleryPurchaseItemDto
            {
                LotNo = row.LotNo,
                ItemName = row.ItemName,
                Pcs = pcs,
                Rate = row.Rate,
                Less1 = row.Less1,
                Less2 = row.Less2,
                Less3 = row.Less3,
                Amount = amount,
                MemoTransCode = memo,
                ReturnCarats = row.ReturnCarats,
                ImagePath = row.ImagePath,
                Status = row.Status
            };
        }

        public async Task<List<JewelleryPurchaseStockOptionDto>> SearchStockAsync(string searchBy, string companyId)
        {
            var safe = (searchBy ?? string.Empty).Replace("'", "''");
            string sql = $"EXEC [GetJewStockDataForDropdown] @strWhere = ' AND (Stock_ID LIKE ''%{safe}%'')'";

            var rows = (await _sqlDataAccess.SelectRawSql<BasicDataDto>(sql, companyId)).ToList();

            return rows
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new JewelleryPurchaseStockOptionDto
                {
                    id = x.Name,
                    text = x.Name
                })
                .ToList();
        }

        public async Task<List<JewelleryPurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
        {
            var escapedLotNo = (lotNo ?? string.Empty).Replace("'", "''");
            string sql = $"EXEC [GetLotNoDataForAll] @Type = 'JewStockData',@Where = ' AND Stock_ID = ''{escapedLotNo}'' '";

            var result = (await _sqlDataAccess.SelectRawSql<JewelleryPurchaseLotDetailDto>(sql, companyId)).ToList();

            foreach (var lot in result)
            {
                if (string.IsNullOrWhiteSpace(lot.LotNo) && !string.IsNullOrWhiteSpace(lot.Stock_ID))
                    lot.LotNo = lot.Stock_ID;

                if (lot.Amount == 0 && lot.FinalAMount != 0)
                    lot.Amount = lot.FinalAMount;
            }

            return result;
        }

        public Task<JewelleryPurchaseGridViewDto> AddItemInGridAsync(
            JewelleryPurchaseAddLotRequest item,
            string companyId)
        {
            string alert = "";

            float purRate = item.Rate;

            if (purRate == 0 && item.Pcs == 0)
                throw new Exception("Rate and Pcs both can not be 0.");

            float amount = item.Pcs * purRate;
            float l1 = item.Less1 != 0 ? amount * (100 + item.Less1) / 100 : amount;
            float l2 = item.Less2 != 0 ? l1 * (100 + item.Less2) / 100 : l1;
            float l3 = item.Less3 != 0 ? l2 * (100 + item.Less3) / 100 : l2;

            var pur = new JewelleryPurchaseItemDto
            {
                LotNo = item.LotNo,
                ItemName = item.ItemName,
                Pcs = item.Pcs,
                Rate = item.Rate,
                Less1 = item.Less1,
                Less2 = item.Less2,
                Less3 = item.Less3,
                Amount = l3,
                MemoTransCode = item.MemoTransCode,
                ImagePath = item.ImagePath
            };

            var list = item.Items ?? new List<JewelleryPurchaseItemDto>();
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

            return Task.FromResult(BuildGridView(list, alert));
        }

        public async Task<JewelleryPurchaseGridViewDto> GetItemFromListAsync(
            JewelleryPurchaseGetItemFromListRequest request,
            string companyId)
        {
            var list = request.Items ?? new List<JewelleryPurchaseItemDto>();
            string alert = "";

            string strWhere;
            if (!string.IsNullOrWhiteSpace(request.LotNo))
            {
                var tmp = request.LotNo.Replace("\r\n", ",").Replace("\n", ",");
                tmp = tmp.Replace(",", "'',''");
                strWhere = $" AND Stock_Id IN (''{tmp}'')";
            }
            else
            {
                strWhere = " AND Stock_Id IN ('''')";
            }

            // Always load when lots provided (SBaseRate defaults to 1; ignore MVC SBaseRate==1-only gate)
            string sql = $"EXEC [GetLotNoDataForAll] @Type = 'JewPur',@Where = '{strWhere}'";
            var lots = (await _sqlDataAccess.SelectRawSql<JewelleryPurchaseLotDetailDto>(sql, companyId)).ToList();

            foreach (var row in lots)
            {
                if (string.IsNullOrWhiteSpace(row.LotNo) && !string.IsNullOrWhiteSpace(row.Stock_ID))
                    row.LotNo = row.Stock_ID;

                float amount = row.Amount != 0 ? row.Amount : row.FinalAMount;
                float l1 = request.Less1 != 0 ? amount * (100 + request.Less1) / 100 : amount;
                float l2 = request.Less2 != 0 ? l1 * (100 + request.Less2) / 100 : l1;
                float l3 = request.Less3 != 0 ? l2 * (100 + request.Less3) / 100 : l2;

                string stockId = row.Stock_ID ?? row.LotNo ?? string.Empty;

                if (list.Any(x => string.Equals(x.LotNo, stockId, StringComparison.OrdinalIgnoreCase)))
                {
                    alert += " Already Added : " + stockId;
                    continue;
                }

                list.Add(new JewelleryPurchaseItemDto
                {
                    LotNo = stockId,
                    ItemName = row.ItemName,
                    Pcs = row.Pcs,
                    Rate = row.Rate,
                    Less1 = request.Less1,
                    Less2 = request.Less2,
                    Less3 = request.Less3,
                    Amount = l3,
                    MemoTransCode = row.MemoTransCode,
                    ImagePath = row.ImagePath
                });
            }

            return BuildGridView(list, alert);
        }

        private static JewelleryPurchaseGridViewDto BuildGridView(List<JewelleryPurchaseItemDto> list, string alert)
        {
            int totalPcs = 0;
            double totalAmount = 0;
            double totalRate = 0;

            foreach (var row in list)
            {
                totalPcs += row.Pcs;
                totalRate += row.Rate;
                totalAmount += row.Amount;
            }

            return new JewelleryPurchaseGridViewDto
            {
                TotalPcs = totalPcs,
                TotalAmount = Math.Round(totalAmount, 2),
                TotalRate = Math.Round(totalRate, 3),
                Alert = alert,
                ExcelData = list
            };
        }

        public async Task<List<JewelleryPurchaseConsignmentRowDto>> GetConsignmentAsync(
            string? partyCode,
            string? memoNo,
            string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Party", partyCode ?? "" },
                { "@invno", memoNo ?? "" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<JewelleryPurchaseConsignmentRowDto>(
                "Sp_SelectJewConsignmentForJewPurchase",
                parameters,
                companyId);

            return result.ToList();
        }

        public async Task<(bool Success, string Message)> AddAsync(
            JewelleryPurchaseHeaderDto salePur,
            List<JewelleryPurchaseItemDto> items,
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
                        string InvnoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''PJEW'' ");
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
                        return (true, "Jewellery Purchase Record Saved.");
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
            JewelleryPurchaseHeaderDto salePur,
            List<JewelleryPurchaseItemDto> items,
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
                            string InvnoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''PJEW'' ");
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
                            cmd.Parameters.Add(new SqlParameter("@vtype", "PJEW"));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        if (!salePur.chkoldpur)
                        {
                            await InsertPurchaseJvEntriesAsync(
                                conn, trans, salePur, MaxSPCode, Curcode,
                                MCurBaseRate, SCurBaseRate, MCurOperator, MCurOperatorM, SCurOperatorM,
                                MToSCurRate, MCurCode, usercode, userId, companyId);
                        }

                        // Jewellery does not call SP_UpdateItemMasterAtEditPrachase

                        trans.Commit();
                        return (true, "Jewellery Purchase Record updated.");
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
                        string payRecCountStr = "0";
                        try
                        {
                            using (var cmd = new SqlCommand("GetPayRecCount", conn, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@Where", " And InvCode = " + id));
                                var result = await cmd.ExecuteScalarAsync();
                                payRecCountStr = result?.ToString() ?? "0";
                            }
                        }
                        catch
                        {
                            // Do not fail if GetPayRecCount SP is missing
                            payRecCountStr = "0";
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

                        DateTime trnDate = DateTime.Now;
                        using (var cmd = new SqlCommand("SELECT Convert(varchar, TrnDate, 120) FROM TabSalePur WHERE Code = @Code", conn, trans))
                        {
                            cmd.Parameters.Add(new SqlParameter("@Code", id));
                            var dateStr = (await cmd.ExecuteScalarAsync())?.ToString();
                            if (!string.IsNullOrEmpty(dateStr))
                                trnDate = Convert.ToDateTime(dateStr);
                        }

                        var memoRows = new List<(float MemoTransCode, float Pcs)>();
                        using (var cmd = new SqlCommand("GetSalepurTran_Sale", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Where", " AND InvCode = " + id));
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    float memo = 0;
                                    float pcs = 0;
                                    try
                                    {
                                        int memoOrd = reader.GetOrdinal("MemoTransCode");
                                        if (!reader.IsDBNull(memoOrd))
                                            memo = Convert.ToSingle(reader.GetValue(memoOrd));
                                    }
                                    catch { }

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
                                        int pOrd = reader.GetOrdinal("Pcs");
                                        if (!reader.IsDBNull(pOrd))
                                            pcs = Convert.ToSingle(reader.GetValue(pOrd));
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            int cOrd = reader.GetOrdinal("Carats");
                                            if (!reader.IsDBNull(cOrd))
                                                pcs = Convert.ToSingle(reader.GetValue(cOrd));
                                        }
                                        catch { }
                                    }

                                    if (memo > 0)
                                        memoRows.Add((memo, pcs));
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
                                cmd.Parameters.Add(new SqlParameter("@spcts", -float.Parse(row.Pcs.ToString("N3"))));
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
                            cmd.Parameters.Add(new SqlParameter("@vtype", "PJEW"));
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

        public async Task<JewelleryPurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
        {
            // MVC jewpurchase/Jewprintinv — SP_SaleInvData / GetSalepurRecordWithDetailsForINV
            string where = $"tsp.TrnType = ''PJEW'' AND tsp.InvNo = ''{invNo}''";
            string sql = $"EXEC GetSalepurRecordWithDetailsForINV @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectDataAdoDynamic(sql, companyId)).ToList();
            if (rows.Count == 0)
                return null;

            if (rows[0] is not IDictionary<string, object> header)
                return null;

            var stockSymbol = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'SSymbol'",
                new { },
                companyId) ?? "USD";
            var masterSymbol = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'MSymbol'",
                new { },
                companyId) ?? "INR";

            // MVC SqlDataAccess.GetSCurID → EXEC GetCurrencyDetail @Type='SCurID'
            var sCurParam = new Dictionary<string, object> { { "@Type", "SCurID" }, { "@Where", "" } };
            var sCurIdText = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", sCurParam, companyId);
            float.TryParse(sCurIdText, out var sCurId);

            string? entrySymbol = GetPrintDictString(header, "Symbol");
            if (string.IsNullOrWhiteSpace(entrySymbol))
            {
                var curCodeText = GetPrintDictString(header, "CurCode");
                if (float.TryParse(curCodeText, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var curCode))
                    entrySymbol = Math.Abs(curCode - sCurId) < 0.0001f ? stockSymbol : masterSymbol;
            }

            string currency =
                string.Equals(entrySymbol, stockSymbol, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entrySymbol, "USD", StringComparison.OrdinalIgnoreCase)
                    ? stockSymbol
                    : masterSymbol;

            bool useStockAmount =
                string.Equals(currency, stockSymbol, StringComparison.OrdinalIgnoreCase)
                || string.Equals(currency, "USD", StringComparison.OrdinalIgnoreCase);

            DateTime? trnDate = GetPrintDictDate(header, "TrnDate");
            DateTime? roundDate = GetPrintDictDate(header, "RoundDate");
            DateTime? dueDate = GetPrintDictDate(header, "DueDate");

            var result = new JewelleryPurchasePrintViewDto
            {
                InvNo = GetPrintDictString(header, "InvNo"),
                BillNo = GetPrintDictString(header, "BillNo"),
                TrnDate = trnDate?.ToString("MM/dd/yyyy"),
                RoundDate = roundDate?.ToString("MM/dd/yyyy"),
                DueDays = short.TryParse(GetPrintDictString(header, "DueDays"), out var dd) ? dd : (short)0,
                DueDate = dueDate?.ToString("MM/dd/yyyy"),
                Currency = currency,
                StockSymbol = stockSymbol,
                MasterSymbol = masterSymbol,
                PartyName = GetPrintDictString(header, "PartyName"),
                AddressLine1 = GetPrintDictString(header, "AddressLine1"),
                AddressLine2 = GetPrintDictString(header, "AddressLine2"),
                AddressLine3 = GetPrintDictString(header, "AddressLine3"),
                SalesPerson = GetPrintDictString(header, "SalesPerson"),
                ContactPerson = GetPrintDictString(header, "ContactPerson"),
                ShipVia = GetPrintDictString(header, "ShipVia"),
                Tracking = GetPrintDictString(header, "Tracking"),
                bnm = GetPrintDictString(header, "bnm"),
                BankAccNo = GetPrintDictString(header, "BankAccNo"),
                ABARoutingNumber = GetPrintDictString(header, "ABARoutingNumber"),
                SwiftCode = GetPrintDictString(header, "SwiftCode"),
                ZelleID = GetPrintDictString(header, "ZelleID"),
                IbanNo = GetPrintDictString(header, "IbanNo"),
                Items = new List<JewelleryPurchasePrintItemDto>()
            };

            foreach (var rowObj in rows)
            {
                if (rowObj is not IDictionary<string, object> row)
                    continue;

                double jewelleryFAmount = GetPrintDictDouble(row, "JewelleryFAmount");
                double masterAmt = GetPrintDictDouble(row, "MasterCurrencyAmount");
                // MVC: display amount = JewelleryFAmount when stock/USD else MasterCurrencyAmount
                double displayAmount = useStockAmount ? jewelleryFAmount : masterAmt;

                byte[]? imgBytes = null;
                foreach (var kvp in row)
                {
                    if (string.Equals(kvp.Key, "ImageFullPath", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(kvp.Key, "ImagePath", StringComparison.OrdinalIgnoreCase))
                    {
                        if (kvp.Value is byte[] b && b.Length > 0)
                        {
                            imgBytes = b;
                            break;
                        }
                    }
                }

                string? imagePath = imgBytes != null
                    ? $"data:image/png;base64,{Convert.ToBase64String(imgBytes)}"
                    : GetPrintDictString(row, "ImagePath");

                double gross = GetPrintDictDouble(row, "GrossWeight");
                double net = GetPrintDictDouble(row, "NetWeight");
                double carats = GetPrintDictDouble(row, "Carats");
                double finalJew = GetPrintDictDouble(row, "FinalJewAmount");

                result.Items.Add(new JewelleryPurchasePrintItemDto
                {
                    StockNo = GetPrintDictString(row, "lotno", "LotNo", "Lotno"),
                    Description = GetPrintDictString(row, "ItemName"),
                    Qty = (int)GetPrintDictDouble(row, "Pcs"),
                    Weight = carats,
                    GrossWeight = gross != 0 ? gross : null,
                    NetWeight = net != 0 ? net : null,
                    PricePerCarat = finalJew,
                    FinalJewAmount = finalJew,
                    Amount = displayAmount,
                    ImagePath = imagePath
                });
            }

            return result;
        }

        private static string? GetPrintDictString(IDictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                foreach (var kvp in row)
                {
                    if (!string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (kvp.Value == null || kvp.Value == DBNull.Value)
                        continue;
                    var text = Convert.ToString(kvp.Value)?.Trim();
                    if (!string.IsNullOrEmpty(text))
                        return text;
                }
            }
            return null;
        }

        private static double GetPrintDictDouble(IDictionary<string, object> row, params string[] keys)
        {
            var text = GetPrintDictString(row, keys);
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            return double.TryParse(text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var n) ? n : 0;
        }

        private static DateTime? GetPrintDictDate(IDictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                foreach (var kvp in row)
                {
                    if (!string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (kvp.Value is DateTime dt)
                        return dt;
                    if (kvp.Value != null && kvp.Value != DBNull.Value
                        && DateTime.TryParse(Convert.ToString(kvp.Value), out var parsed))
                        return parsed;
                }
            }
            return null;
        }

        public async Task<JewelleryPurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId)
        {
            string where = $"tsp.TrnType = ''PJEW'' AND tsp.InvNo = {invNo}";
            string sql = $"EXEC [SoliPrintCurStatusInv] @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectRawSql<StatusInvRow>(sql, companyId)).ToList();
            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var invoiceRow = rows.FirstOrDefault(x => string.Equals(x.Type, "Invoice", StringComparison.OrdinalIgnoreCase)) ?? header;

            var result = new JewelleryPurchaseStatusDto
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
                Items = new List<JewelleryPurchaseStatusItemDto>()
            };

            foreach (var row in rows)
            {
                result.Items.Add(new JewelleryPurchaseStatusItemDto
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
            JewelleryPurchaseHeaderDto salePur,
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
                cmd.Parameters.Add(new SqlParameter("@trntype", "PJEW"));
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
            JewelleryPurchaseHeaderDto salePur,
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
                cmd.Parameters.Add(new SqlParameter("@TrnType", "PJEW"));
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
            JewelleryPurchaseHeaderDto salePur,
            List<JewelleryPurchaseItemDto> items,
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
            foreach (var im in items)
            {
                float PurAmount = im.Amount;
                var purAmtRes = GetConvertAmount(curCode, PurAmount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);
                double PurMCurAmt = purAmtRes.MCurAmt;
                double PurSCurAmt = purAmtRes.SCurAmt;

                string lotNo = im.LotNo ?? string.Empty;
                string ItemCodeStr = await ExecuteCommonDataScalarAsync(
                    conn, trans, "StockCode", " AND Stock_ID = ''" + lotNo.Replace("'", "''") + "''");
                int MaxItemCode = 0;
                int.TryParse(ItemCodeStr, out MaxItemCode);

                float spcarats = im.MemoTransCode != 0 ? im.Pcs : 0f;

                using (var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPdate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@TrnType", "PJEW"));
                    cmd.Parameters.Add(new SqlParameter("@InvType", salePur.InvType ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@InvCode", maxSPCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemCode", MaxItemCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemName", im.ItemName ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@Carats", (float)im.Pcs));
                    cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                    cmd.Parameters.Add(new SqlParameter("@Rate", Convert.ToDouble(im.Rate.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@MRP", 0f));
                    cmd.Parameters.Add(new SqlParameter("@PMRPDown", 0d));
                    cmd.Parameters.Add(new SqlParameter("@ReturnCarats", Convert.ToDouble(im.ReturnCarats.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@IssueCarats", 0d));
                    cmd.Parameters.Add(new SqlParameter("@amount", Convert.ToDouble(im.Amount.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ExportNo", ""));
                    cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                    cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                    cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                    cmd.Parameters.Add(new SqlParameter("@CurCode", salePur.CurCode.ToString()));
                    cmd.Parameters.Add(new SqlParameter("@memotrancode", im.MemoTransCode));
                    cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                    cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
                    cmd.Parameters.Add(new SqlParameter("@spcarats", spcarats));
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
                    cmd.Parameters.Add(new SqlParameter("@InvDescription", ""));
                    cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));

                    await cmd.ExecuteNonQueryAsync();
                }

                // MVC: MemoTransCode != 0 → contopurtest; else still UpdateMemo with contopur
                string memoAction = im.MemoTransCode != 0 ? "contopurtest" : "contopur";
                using (var cmd = new SqlCommand("SP_UpdateMemo", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@code", im.MemoTransCode));
                    cmd.Parameters.Add(new SqlParameter("@RtnDate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@rtncts", float.Parse(im.ReturnCarats.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@spcts", float.Parse(im.Pcs.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@Actiontype", memoAction));
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task InsertPurchaseJvEntriesAsync(
            SqlConnection conn,
            SqlTransaction trans,
            JewelleryPurchaseHeaderDto salePur,
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

            await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, PurchaseCode, salePur.PartyCode, Math.Abs(PurchaseJVAMount), curCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, PurJvMAmount, sCurBaseRate, PurJvSAmount, 0, usercode, userId, companyId);

            if ((Convert.ToDouble(salePur.BrokerAmount) != 0) && salePur.BrokerCode != null)
            {
                float BroAmt = salePur.BrokerAmount;
                var broConvert = GetConvertAmount(mCurCode, BroAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, BrokerageCode, salePur.BrokerCode.Value, Math.Abs(BroAmt), mCurCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, broConvert.MCurAmt, sCurBaseRate, broConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if ((Convert.ToDouble(salePur.AdatAmount) != 0) && salePur.AdatCode != null)
            {
                float AdatAmt = salePur.AdatAmount;
                var adatConvert = GetConvertAmount(mCurCode, AdatAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, AdatCode, salePur.AdatCode.Value, AdatAmt, mCurCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, adatConvert.MCurAmt, sCurBaseRate, adatConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.VatAmount) != 0)
            {
                float VatAmt = salePur.VatAmount;
                var vatConvert = GetConvertAmount(mCurCode, VatAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, VatCode, PurchaseCode, VatAmt, mCurCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, vatConvert.MCurAmt, sCurBaseRate, vatConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.SGSTAmt) != 0)
            {
                float SGSTAmt = salePur.SGSTAmt;
                var sgstConvert = GetConvertAmount(mCurCode, SGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, SGSTCode, PurchaseCode, SGSTAmt, mCurCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, sgstConvert.MCurAmt, sCurBaseRate, sgstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.CGSTAmt) != 0)
            {
                float CGSTAmt = salePur.CGSTAmt;
                var cgstConvert = GetConvertAmount(mCurCode, CGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, CGSTCode, PurchaseCode, CGSTAmt, mCurCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, cgstConvert.MCurAmt, sCurBaseRate, cgstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.IGSTAmt) != 0)
            {
                float IGSTAmt = salePur.IGSTAmt;
                var igstConvert = GetConvertAmount(mCurCode, IGSTAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, IGSTCode, PurchaseCode, IGSTAmt, mCurCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, igstConvert.MCurAmt, sCurBaseRate, igstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.RoundAmount) > 0)
            {
                float RoundAmt = salePur.RoundAmount;
                var roundConvert = GetConvertAmount(mCurCode, RoundAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, CashCode, salePur.PartyCode, salePur.RoundAmount, mCurCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, roundConvert.MCurAmt, sCurBaseRate, roundConvert.SCurAmt, 0, usercode, userId, companyId);
            }
            else if (Convert.ToDouble(salePur.RoundAmount) < 0)
            {
                float RoundAmt = salePur.RoundAmount;
                var roundConvert = GetConvertAmount(mCurCode, RoundAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.PartyCode, CashCode, Math.Abs(RoundAmt), mCurCode.ToString(), "PJEW", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, mCurBaseRate, Math.Abs(roundConvert.MCurAmt), sCurBaseRate, Math.Abs(roundConvert.SCurAmt), 0, usercode, userId, companyId);
            }
        }

        #endregion

        #region Copied helpers from SolitairePurchaseRepository

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

        private class JewPurchaseTranRow
        {
            public string? LotNo { get; set; }
            public string? ItemName { get; set; }
            public int Pcs { get; set; }
            public float Carats { get; set; }
            public float Rate { get; set; }
            public float Less1 { get; set; }
            public float Less2 { get; set; }
            public float Less3 { get; set; }
            public float Amount { get; set; }
            public float PurAmount { get; set; }
            public float MemoTransCode { get; set; }
            public float MemoTranCode { get; set; }
            public float ReturnCarats { get; set; }
            public string? ImagePath { get; set; }
            public string? Status { get; set; }
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
    }
}
